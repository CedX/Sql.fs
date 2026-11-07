namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `ObjectExistsExtensions` module.
[<TestClass>]
type ObjectExistsExtensionsTests() =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.Exists() =
    connection.Exists<Character> 1 |> Should.beTrue
    connection.Exists<Character> 666 |> Should.beFalse

  [<TestMethod>]
  member _.AsyncExists(): Task = task {
    do! connection.AsyncExists<Character> 1 |> ShouldAsync.beTrue
    do! connection.AsyncExists<Character> 666 |> ShouldAsync.beFalse
  }
