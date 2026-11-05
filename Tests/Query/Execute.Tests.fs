namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `QueryExecuteExtensions` module.
[<TestClass>]
type QueryExecuteExtensionsTests(testContext: TestContext) =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.Execute() =
    let parameters = ParameterCollection [Parameter("Gender", nameof CharacterGender.Balrog)]
    connection.ExecuteScalar<int> "SELECT COUNT(*) FROM Characters" |> Should.be 16
    connection.Execute("DELETE FROM Characters WHERE gender = @Gender", parameters) |> Should.be 2
    connection.ExecuteScalar<int> "SELECT COUNT(*) FROM Characters" |> Should.be 14

    let parameters = ParameterCollection [Parameter("Gender", nameof CharacterGender.Elf)]
    connection.Execute("DELETE FROM Characters WHERE gender = @Gender", parameters) |> Should.be 3
    connection.ExecuteScalar<int> "SELECT COUNT(*) FROM Characters" |> Should.be 11

  [<TestMethod>]
  member _.AsyncExecute(): Task = task {
    let parameters = ParameterCollection[Parameter("Gender", nameof CharacterGender.Balrog)]
    do! connection.AsyncExecuteScalar<int>("SELECT COUNT(*) FROM Characters", cancellationToken = testContext.CancellationToken) |> ShouldAsync.be 16
    do! connection.AsyncExecute("DELETE FROM Characters WHERE gender = @Gender", parameters, testContext.CancellationToken) |> ShouldAsync.be 2
    do! connection.AsyncExecuteScalar<int>("SELECT COUNT(*) FROM Characters", cancellationToken = testContext.CancellationToken) |> ShouldAsync.be 14

    let parameters = ParameterCollection[Parameter("Gender", nameof CharacterGender.Elf)]
    do! connection.AsyncExecute("DELETE FROM Characters WHERE gender = @Gender", parameters, testContext.CancellationToken) |> ShouldAsync.be 3
    do! connection.AsyncExecuteScalar<int>("SELECT COUNT(*) FROM Characters", cancellationToken = testContext.CancellationToken) |> ShouldAsync.be 11
  }
