namespace Belin.Sql

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
      character.FullName |> Should.be (Some "Balin")

    let record = connection.Find<Character> 14
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.Id |> Should.be 14
      character.FullName |> Should.be (Some "Sam Gamgee")

    // It should allow selecting a specific set of columns.
    let record = connection.Find<Character>(2, [| "gender" |])
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.FullName |> Should.be None
      character.Gender |> Should.be CharacterGender.Dwarf

    let record = connection.Find<Character>(14, [| "gender" |])
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.FullName |> Should.be None
      character.Gender |> Should.be CharacterGender.Hobbit

    // It should return `None` if the record is not found.
    connection.Find<Character> 666 |> Should.be None

  [<TestMethod>]
  member _.AsyncFind(): Task = task {
    // It should find the record with the specified identifier.
    let! record = connection.FindAsync<Character> 2
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.Id |> Should.be 2
      character.FullName |> Should.be (Some "Balin")

    let! record = connection.FindAsync<Character> 14
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.Id |> Should.be 14
      character.FullName |> Should.be (Some "Sam Gamgee")

    // It should allow selecting a specific set of columns.
    let! record = connection.FindAsync<Character>(2, [| "gender" |])
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.FullName |> Should.be None
      character.Gender |> Should.be CharacterGender.Dwarf

    let! record = connection.FindAsync<Character>(14, [| "gender" |])
    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.FullName |> Should.be None
      character.Gender |> Should.be CharacterGender.Hobbit

    // It should return `None` if the record is not found.
    do! connection.FindAsync<Character> 666 |> ShouldAsync.be None
  }

  // [<TestMethod>]
  // member _.FindAll() =
  //   // It should return the complete list of entities, sorted by default according to the identity column.
  //   let records = connection.FindAll<Character>()
  //   Assert.HasCount(16, records)
  //   Assert.AreEqual(1, records[0].Id)
  //   Assert.AreEqual("Aragorn", records[0].FullName)
  //   Assert.AreEqual(16, records[15].Id)
  //   Assert.AreEqual("Sauron", records[15].FullName)

  //   // It should allow sorting the results by a specific set of columns.
  //   records = connection.FindAll<Character>([("gender", SortOrder.Ascending), ("fullName", SortOrder.Descending)])
  //   Assert.HasCount(16, records)
  //   Assert.AreEqual(11, records[0].Id)
  //   Assert.AreEqual("Gothmog", records[0].FullName)
  //   Assert.AreEqual(8, records[15].Id)
  //   Assert.AreEqual("Gandalf", records[15].FullName)

  //   // It should allow selecting a specific set of columns.
  //   records = connection.FindAll<Character>(columns: [| "gender" |])
  //   Assert.AreEqual(1, records[0].Id)
  //   Assert.AreEqual(CharacterGender.Human, records[0].Gender)
  //   records[0].FullName |> Should.beNull
  //   Assert.AreEqual(16, records[15].Id)
  //   Assert.AreEqual(CharacterGender.DarkLord, records[15].Gender)
  //   records[15].FullName |> Should.beNull

  // [<TestMethod>]
  // member _.AsyncFindAll(): Task = task {
  //   // It should return the complete list of entities, sorted by default according to the identity column.
  //   let records = await connection.FindAllAsync<Character>(
  //   Assert.HasCount(16, records)
  //   Assert.AreEqual(1, records[0].Id)
  //   Assert.AreEqual("Aragorn", records[0].FullName)
  //   Assert.AreEqual(16, records[15].Id)
  //   Assert.AreEqual("Sauron", records[15].FullName)

  //   // It should allow sorting the results by a specific set of columns.
  //   records = await connection.FindAllAsync<Character>([("gender", SortOrder.Ascending), ("fullName", SortOrder.Descending)],
  //   Assert.HasCount(16, records)
  //   Assert.AreEqual(11, records[0].Id)
  //   Assert.AreEqual("Gothmog", records[0].FullName)
  //   Assert.AreEqual(8, records[15].Id)
  //   Assert.AreEqual("Gandalf", records[15].FullName)

  //   // It should allow selecting a specific set of columns.
  //   records = await connection.FindAllAsync<Character>(columns: [| "gender" |],
  //   Assert.AreEqual(1, records[0].Id)
  //   Assert.AreEqual(CharacterGender.Human, records[0].Gender)
  //   records[0].FullName |> Should.beNull
  //   Assert.AreEqual(16, records[15].Id)
  //   Assert.AreEqual(CharacterGender.DarkLord, records[15].Gender)
  //   records[15].FullName |> Should.beNull
  // }
