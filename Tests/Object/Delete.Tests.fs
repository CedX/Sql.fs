namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `ObjectDeleteExtensions` module.
[<TestClass>]
type ObjectDeleteExtensionsTests() =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.Delete() =
    let sql = "SELECT * FROM Characters WHERE ID = @Id"
    let parameters = ParameterCollection (Parameter("Id", 1))
    let record = connection.QuerySingle<Character>(sql, parameters) |> Option.get
    connection.Delete record |> Should.beTrue
    connection.Delete record |> Should.beFalse
    connection.QueryFirst<Character>(sql, parameters) |> Should.be None

  [<TestMethod>]
  member _.DeleteAll() =
    let sql = "SELECT COUNT(*) FROM Characters"
    connection.ExecuteScalar<int> sql |> Should.beGreaterThan 0
    connection.DeleteAll<Character>(truncate = true)
    connection.ExecuteScalar<int> sql |> Should.be 0

  // [<TestMethod>]
  member _.AsyncDelete(): Task = task {
    let sql = "SELECT * FROM Characters WHERE ID = @Id"
    let parameters = ParameterCollection (Parameter("Id", 2))
    let! record = connection.AsyncQuerySingle<Character>(sql, parameters)

    let character = Option.get record
    do! connection.AsyncDelete character |> ShouldAsync.beTrue
    do! connection.AsyncDelete character |> ShouldAsync.beFalse
    do! connection.AsyncQueryFirst<Character>(sql, parameters) |> ShouldAsync.be None
  }

  [<TestMethod>]
  member _.AsyncDeleteAll(): Task = task {
    let sql = "SELECT COUNT(*) FROM Characters"
    do! connection.AsyncExecuteScalar<int> sql |> ShouldAsync.beGreaterThan 0
    do! connection.AsyncDeleteAll<Character>(truncate = true)
    do! connection.AsyncExecuteScalar<int> sql |> ShouldAsync.be 0
  }
