namespace Belin.Sql

open Belin.Should
open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `ObjectInsertExtensions` module.
[<TestClass>]
type ObjectInsertExtensionsTests() =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.Insert() =
    let sql = "SELECT * FROM Characters WHERE firstName = 'Cédric'"
    connection.Query<Character> sql |> Should.beEmpty

    let record = { Character.Default with FirstName = "Cédric"; LastName = "Belin"; Gender = CharacterGender.Istari }
    record.Id |> Should.be 0
    record.FullName |> Should.be None

    let id = int (connection.Insert record)
    id |> Should.beGreaterThan 16
    record.Id |> Should.be id

    let records = connection.Query<Character> sql
    records.Length |> Should.be 1

    let cedric = List.head records
    cedric.Id |> Should.be id
    cedric.FullName |> Should.be (Some "Cédric Belin")
    cedric.Gender |> Should.be record.Gender

  [<TestMethod>]
  member _.AsyncInsert(): Task = task {
    let sql = "SELECT * FROM Characters WHERE firstName = 'Cédric'"
    let! records = connection.AsyncQuery<Character> sql
    Should.beEmpty records

    let record = { Character.Default with FirstName = "Cédric"; LastName = "Belin"; Gender = CharacterGender.Istari }
    record.Id |> Should.be 0
    record.FullName |> Should.be None

    let! id = connection.AsyncInsert record
    let id = int id
    id |> Should.beGreaterThan 16
    record.Id |> Should.be id

    let! records = connection.AsyncQuery<Character> sql
    records.Length |> Should.be 1

    let cedric = List.head records
    cedric.Id |> Should.be id
    cedric.FullName |> Should.be (Some "Cédric Belin")
    cedric.Gender |> Should.be record.Gender
  }
