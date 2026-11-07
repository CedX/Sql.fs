namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `ObjectCountExtensions` module.
[<TestClass>]
type ObjectCountExtensionsTests() =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.CountAll() =
    // It should return the total number of entities from the underlying table.
    connection.CountAll<Character>() |> Should.be 16

  [<TestMethod>]
  member _.AsyncCountAll(): Task = task {
    // It should return the total number of entities from the underlying table.
    do! connection.AsyncCountAll<Character>() |> ShouldAsync.be 16
  }
