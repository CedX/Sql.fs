namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `QueryExecuteScalarExtensions` module.
[<TestClass>]
type QueryExecuteScalarExtensionsTests() =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.ExecuteScalar() =
    let sql = "SELECT COUNT(*) FROM Characters WHERE gender = @Gender"
    let parameters = ParameterCollection [Parameter("Gender", nameof CharacterGender.Balrog)]
    connection.ExecuteScalar<int>(sql, parameters) |> Should.be 2

    let sql = "SELECT tbl_name FROM sqlite_schema WHERE type = @Type AND name = @Name"
    let parameters = ParameterCollection [Parameter("Name", "Characters"); Parameter("Type", "table")]
    connection.ExecuteScalar<string>(sql, parameters) |> Should.be "Characters"

    let sql = "SELECT tbl_name FROM sqlite_schema WHERE name = @Name"
    let parameters = ParameterCollection [Parameter("Name", "FooBarBazQux")]
    connection.ExecuteScalar<string>(sql, parameters) |> Should.beNull

  // [<TestMethod>]
  member _.AsyncExecuteScalar(): Task = task {
    let sql = "SELECT COUNT(*) FROM Characters WHERE gender = @Gender"
    let parameters = ParameterCollection [Parameter("Gender", nameof CharacterGender.Balrog)]
    do! connection.AsyncExecuteScalar<int>(sql, parameters) |> ShouldAsync.be 2

    let sql = "SELECT tbl_name FROM sqlite_schema WHERE type = @Type AND name = @Name"
    let parameters = ParameterCollection [Parameter("Name", "Characters"); Parameter("Type", "table")]
    do! connection.AsyncExecuteScalar<string>(sql, parameters) |> ShouldAsync.be "Characters"

    let sql = "SELECT tbl_name FROM sqlite_schema WHERE name = @Name"
    let parameters = ParameterCollection [Parameter("Name", "FooBarBazQux")]
    do! connection.AsyncExecuteScalar<string | null>(sql, parameters) |> ShouldAsync.beNull
  }
