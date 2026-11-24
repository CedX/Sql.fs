namespace Belin.Sql

open Belin.Should
open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `ObjectFindExtensions` module.
[<TestClass>]
type ObjectFindExtensionsTests() =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.Find() =
    // It should find the record with the specified identifier.
    let record = connection.Find<Character> 2
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.Id |> Should.be 2
      character.FullName |> Should.be "Balin"

    let record = connection.Find<Character> 14
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.Id |> Should.be 14
      character.FullName |> Should.be "Sam Gamgee"

    // It should allow selecting a specific set of columns.
    let record = connection.Find<Character>(2, [| "gender" |])
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      Should.beNull character.FullName
      character.Gender |> Should.be CharacterGender.Dwarf

    let record = connection.Find<Character>(14, [| "gender" |])
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      Should.beNull character.FullName
      character.Gender |> Should.be CharacterGender.Hobbit

    // It should return `None` if the record is not found.
    connection.Find<Character> 666 |> Should.be None

  [<TestMethod>]
  member _.AsyncFind(): Task = task {
    // It should find the record with the specified identifier.
    let! record = connection.AsyncFind<Character> 2
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.Id |> Should.be 2
      character.FullName |> Should.be "Balin"

    let! record = connection.AsyncFind<Character> 14
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.Id |> Should.be 14
      character.FullName |> Should.be "Sam Gamgee"

    // It should allow selecting a specific set of columns.
    let! record = connection.AsyncFind<Character>(2, [| "gender" |])
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      Should.beNull character.FullName
      character.Gender |> Should.be CharacterGender.Dwarf

    let! record = connection.AsyncFind<Character>(14, [| "gender" |])
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      Should.beNull character.FullName
      character.Gender |> Should.be CharacterGender.Hobbit

    // It should return `None` if the record is not found.
    do! connection.AsyncFind<Character> 666 |> AsyncShould.be None
  }

  [<TestMethod>]
  member _.FindAll() =
    // It should return the complete list of entities, sorted by default according to the identity column.
    let records = connection.FindAll<Character>()
    records.Length |> Should.be 16
    records[0].Id |> Should.be 1
    records[0].FullName |> Should.be "Aragorn"
    records[15].Id |> Should.be 16
    records[15].FullName |> Should.be "Sauron"

    // It should allow sorting the results by a specific set of columns.
    let records = connection.FindAll<Character> ["gender", Ascending; "fullName", Descending]
    records.Length |> Should.be 16
    records[0].Id |> Should.be 11
    records[0].FullName |> Should.be "Gothmog"
    records[15].Id |> Should.be 8
    records[15].FullName |> Should.be "Gandalf"

    // It should allow selecting a specific set of columns.
    let records = connection.FindAll<Character>(columns = [| "gender" |])
    records[0].Id |> Should.be 1
    records[0].Gender |> Should.be CharacterGender.Human
    Should.beNull records[0].FullName
    records[15].Id |> Should.be 16
    records[15].Gender |> Should.be CharacterGender.DarkLord
    Should.beNull records[15].FullName

  [<TestMethod>]
  member _.AsyncFindAll(): Task = task {
    // It should return the complete list of entities, sorted by default according to the identity column.
    let! records = connection.AsyncFindAll<Character>()
    records.Length |> Should.be 16
    records[0].Id |> Should.be 1
    records[0].FullName |> Should.be "Aragorn"
    records[15].Id |> Should.be 16
    records[15].FullName |> Should.be "Sauron"

    // It should allow sorting the results by a specific set of columns.
    let! records = connection.AsyncFindAll<Character> ["gender", Ascending; "fullName", Descending]
    records.Length |> Should.be 16
    records[0].Id |> Should.be 11
    records[0].FullName |> Should.be "Gothmog"
    records[15].Id |> Should.be 8
    records[15].FullName |> Should.be "Gandalf"

    // It should allow selecting a specific set of columns.
    let! records = connection.AsyncFindAll<Character>(columns = [| "gender" |])
    records[0].Id |> Should.be 1
    records[0].Gender |> Should.be CharacterGender.Human
    Should.beNull records[0].FullName
    records[15].Id |> Should.be 16
    records[15].Gender |> Should.be CharacterGender.DarkLord
    Should.beNull records[15].FullName
  }
