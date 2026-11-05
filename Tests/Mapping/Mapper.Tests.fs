namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Collections.Generic

/// Tests the features of the `Mapper` class.
[<TestClass>]
type MapperTests() =

  /// The test data used by the `ChangeType` method.
  static member val private ChangeTypeData: objnull array list = [
    [| null; typeof<bool>; false; false |]
    [| null; typeof<Nullable<bool>>; true; null |]
    [| DBNull.Value; typeof<bool>; false; false |]
    [| DBNull.Value; typeof<Nullable<bool>>; true; null |]
    [| 0; typeof<bool>; false; false |]
    [| 0; typeof<Nullable<bool>>; true; false |]
    [| 1; typeof<bool>; false; true |]
    [| 1; typeof<Nullable<bool>>; true; true |]
    [| "false"; typeof<bool>; false; false |]
    [| "true"; typeof<bool>; false; true |]

    [| null; typeof<char>; false; Char.MinValue |]
    [| null; typeof<Nullable<char>>; true; null |]
    [| DBNull.Value; typeof<char>; false; Char.MinValue |]
    [| DBNull.Value; typeof<Nullable<char>>; true; null |]
    [| 0; typeof<char>; false; Char.MinValue |]
    [| 65_535; typeof<Nullable<char>>; true; Char.MaxValue |]
    [| 97; typeof<char>; false; 'a' |]
    [| 98; typeof<Nullable<char>>; true; 'b' |]
    [| "a"; typeof<char>; false; 'a' |]
    [| "b"; typeof<Nullable<char>>; true; 'b' |]

    [| null; typeof<DateTime>; false; DateTime.MinValue |]
    [| null; typeof<Nullable<DateTime>>; true; null |]
    [| DBNull.Value; typeof<DateTime>; false; DateTime.MinValue |]
    [| DBNull.Value; typeof<Nullable<DateTime>>; true; null |]
    [| DateTime.MaxValue; typeof<DateTime>; false; DateTime.MaxValue |]
    [| DateTime.UnixEpoch; typeof<Nullable<DateTime>>; true; DateTime.UnixEpoch |]
    [| DateTime(2025, 6, 7, 10, 45, 1); typeof<DateTime>; false; DateTime(2025, 6, 7, 10, 45, 1) |]
    [| DateTime(2026, 1, 31); typeof<Nullable<DateTime>>; true; DateTime(2026, 1, 31) |]
    [| "2025-06-07 10:45:01"; typeof<DateTime>; false; DateTime(2025, 6, 7, 10, 45, 1) |]
    [| "2025-06-07T10:45:01"; typeof<Nullable<DateTime>>; true; DateTime(2025, 6, 7, 10, 45, 1) |]

    [| null; typeof<DayOfWeek>; false; DayOfWeek.Sunday |]
    [| null; typeof<Nullable<DayOfWeek>>; true; null |]
    [| DBNull.Value; typeof<DayOfWeek>; false; DayOfWeek.Sunday |]
    [| DBNull.Value; typeof<Nullable<DayOfWeek>>; true; null |]
    [| 0; typeof<DayOfWeek>; false; DayOfWeek.Sunday |]
    [| 1; typeof<Nullable<DayOfWeek>>; true; DayOfWeek.Monday |]
    [| 5; typeof<DayOfWeek>; false; DayOfWeek.Friday |]
    [| 6; typeof<Nullable<DayOfWeek>>; true; DayOfWeek.Saturday |]
    [| "sunday"; typeof<DayOfWeek>; false; DayOfWeek.Sunday |]
    [| "friday"; typeof<Nullable<DayOfWeek>>; true; DayOfWeek.Friday |]

    [| null; typeof<double>; false; 0.0 |]
    [| null; typeof<Nullable<double>>; true; null |]
    [| DBNull.Value; typeof<double>; false; 0.0 |]
    [| DBNull.Value; typeof<Nullable<double>>; true; null |]
    [| 0; typeof<double>; false; 0.0 |]
    [| 0; typeof<Nullable<double>>; true; 0.0 |]
    [| 123; typeof<double>; false; 123.0 |]
    [| -123.456; typeof<Nullable<double>>; true; -123.456 |]
    [| "123"; typeof<double>; false; 123.0 |]
    [| "-123.456"; typeof<Nullable<double>>; true; -123.456 |]

    [| null; typeof<int>; false; 0 |]
    [| null; typeof<Nullable<int>>; true; null |]
    [| DBNull.Value; typeof<int>; false; 0 |]
    [| DBNull.Value; typeof<Nullable<int>>; true; null |]
    [| 0; typeof<int>; false; 0 |]
    [| 0; typeof<Nullable<int>>; true; 0 |]
    [| 123; typeof<int>; false; 123 |]
    [| -123.456; typeof<Nullable<int>>; true; -123 |]
    [| "123"; typeof<int>; false; 123 |]
    [| "-123"; typeof<Nullable<int>>; true; -123 |]

    [| null; typeof<string>; false; "" |]
    [| null; typeof<string>; true; null |]
    [| DBNull.Value; typeof<string>; false; "" |]
    [| DBNull.Value; typeof<string>; true; null |]
    [| 0; typeof<string>; false; "0" |]
    [| 0; typeof<string>; true; "0" |]
    [| 123; typeof<string>; false; "123" |]
    [| -123.456; typeof<string>; true; "-123.456" |]
    [| "123"; typeof<string>; false; "123" |]
    [| "-123.456"; typeof<string>; true; "-123.456" |]
  ]

  [<TestMethod; DynamicData "ChangeTypeData">]
  member _.ChangeType(value: objnull, conversionType: Type, isNullable: bool, expected: objnull) =
    Mapper.Instance.ChangeType(value, conversionType, isNullable) |> Should.be expected

  [<TestMethod>]
  member _.CreateInstance() =
    let properties = dict [
      "CLASS", "Bard/minstrel" :> objnull
      "firstName", "Cédric"
      "gender", nameof CharacterGender.Balrog
      "lastName", null
    ]

    // It should create an `ExpandoObject` by default.
    let map = Mapper.Instance.CreateInstance properties
    map |> Should.beOfType typeof<Map<string, objnull>>
    map["CLASS"] |> Should.be "Bard/minstrel"
    map["firstName"] |> Should.be "Cédric"
    map["gender"] |> Should.be (nameof CharacterGender.Balrog)
    map["lastName"] |> Should.beNull

    // It should create an object of the specified type.
    let character = Mapper.Instance.CreateInstance<Character> properties
    character |> Should.beOfType typeof<Character>
    character.FirstName |> Should.be "Cédric"
    character.Gender |> Should.be CharacterGender.Balrog
    character.LastName |> Should.be ""

  [<TestMethod>]
  member _.GetTable() =
    let table = Mapper.Instance.GetTable<Character>()
    table.Name |> Should.be "Characters"
    table.Schema |> Should.beSome "main"
    table.Type |> Should.be typeof<Character>

    table.Columns.Keys |> Seq.length |> Should.be 5
    table.IdentityColumn |> Should.beSome table.Columns["ID"]
    table.Columns["gender"].PropertyType |> Should.be typeof<CharacterGender>
    table.Columns["lastName"].PropertyType |> Should.be typeof<string>

    Should.beTrue (table.Columns["firstName"].CanWrite)
    Should.beTrue (table.Columns["fullName"].IsComputed)
    Should.beTrue (table.Columns["ID"].IsIdentity)

  [<TestMethod>]
  member _.IsNullRecord() =
    // It should return `true` if all values of the specified dictionary are `null`.
    Dictionary<string, objnull>() |> Mapper.IsNullRecord |> Should.beTrue
    dict ["Foo", null; "Bar", null] |> Mapper.IsNullRecord |> Should.beTrue

    // It should return `false` if at least one value of the specified dictionary is not `null`.
    dict ["Foo", "Bar" :> objnull; "Baz", null] |> Mapper.IsNullRecord |> Should.beFalse

  [<TestMethod>]
  member _.SplitOn() =
    let record = ResizeArray<string * objnull> [
      "Id", 123 :> objnull
      "LongLabel", "Hello World!"
      "ShortLabel", null
      "Id", 456
      "FirstName", "Cédric"
      "LastName", "Belin"
      "RowID", 789
    ]

    let properties = dict [
      "Id", 123 :> objnull
      "LongLabel", "Hello World!"
      "ShortLabel", null
      "FirstName", "Cédric"
      "LastName", "Belin"
      "RowID", 789
    ]

    // It should return a dictionary equivalent to the specified data row.
    let mutable records = Mapper.SplitOn record
    records.Count |> Should.be 1
    records[0] |> Should.beSequence properties

    // It should not split the data row if the specified field does not exist.
    records <- Mapper.SplitOn(record, "_NonExistent_")
    records.Count |> Should.be 1
    records[0] |> Should.beSequence properties

    // It should split the data row according to the specified fields.
    records <- Mapper.SplitOn(record, "id")
    records.Count |> Should.be 2
    records[0] |> Should.beSequence (dict ["Id", 123; "LongLabel", "Hello World!"; "ShortLabel", null])
    records[1] |> Should.beSequence (dict ["Id", 456; "FirstName", "Cédric"; "LastName", "Belin"; "RowID", 789])

    records <- Mapper.SplitOn(record, "id", "rowid", "_Unused_")
    records.Count |> Should.be 3
    records[0] |> Should.beSequence (dict ["Id", 123; "LongLabel", "Hello World!"; "ShortLabel", null])
    records[1] |> Should.beSequence (dict ["Id", 456; "FirstName", "Cédric"; "LastName", "Belin"])
    records[2] |> Should.beSequence (dict [("RowID", 789)])
