namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Collections.Generic

/// Tests the features of the `SqlMapper` class.
[<TestClass>]
type SqlMapperTests() =

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

    // TODO ajouter des tests pour les options!!!!
  ]

  [<TestMethod; DynamicData "ChangeTypeData">]
  member _.ChangeType(value: objnull, conversionType: Type, isNullable: bool, expected: objnull) =
    SqlMapper.Instance.ChangeType(value, conversionType, isNullable) |> Should.be expected

  // [<TestMethod>]
  // member _.CreateInstance() =
  //   var properties = new Dictionary<string, object?> {
  //     ["CLASS"] = "Bard/minstrel",
  //     ["firstName"] = "Cédric",
  //     ["gender"] = nameof(CharacterGender.Balrog),
  //     ["lastName"] = null
  //   }

  //   // It should create an `ExpandoObject` by default.
  //   dynamic expandoObject = SqlMapper.Instance.CreateInstance(properties)
  //   Assert.IsInstanceOfType<ExpandoObject>(expandoObject)
  //   Assert.AreEqual("Bard/minstrel", expandoObject.CLASS)
  //   Assert.AreEqual("Cédric", expandoObject.firstName)
  //   Assert.AreEqual(nameof(CharacterGender.Balrog), expandoObject.gender)
  //   expandoObject.lastName |> Should.beNull

  //   // It should support creating an object of type `PSObject`.
  //   dynamic psObject = SqlMapper.Instance.CreateInstance<PSObject>(properties)
  //   Assert.IsInstanceOfType<PSObject>(psObject)
  //   Assert.AreEqual("Bard/minstrel", psObject.CLASS)
  //   Assert.AreEqual("Cédric", psObject.firstName)
  //   Assert.AreEqual(nameof(CharacterGender.Balrog), psObject.gender)
  //   psObject.lastName |> Should.beNull

  //   // It should create an object of the specified type.
  //   var character = SqlMapper.Instance.CreateInstance<Character>(properties)
  //   Assert.IsInstanceOfType<Character>(character)
  //   Assert.AreEqual("Cédric", character.FirstName)
  //   Assert.AreEqual(CharacterGender.Balrog, character.Gender)
  //   Assert.AreEqual("", character.LastName)

  [<TestMethod>]
  member _.GetTable() =
    let table = SqlMapper.Instance.GetTable<Character>()
    table.Name |> Should.be "Characters"
    table.Schema |> Should.beSomeValue "main"
    table.Type |> Should.be typeof<Character>

    table.Columns.Keys |> Seq.length |> Should.be 5
    table.IdentityColumn |> Should.beSomeValue table.Columns["ID"]
    table.Columns["gender"].PropertyType |> Should.be typeof<CharacterGender>
    table.Columns["lastName"].PropertyType |> Should.be typeof<string>

    table.Columns["firstName"].CanWrite |> Should.beTrue
    table.Columns["fullName"].IsComputed |> Should.beTrue
    table.Columns["ID"].IsIdentity |> Should.beTrue

  [<TestMethod>]
  member _.IsNullRecord() =
    // It should return `true` if all values of the specified dictionary are `null`.
    Dictionary<string, objnull>() |> SqlMapper.IsNullRecord |> Should.beTrue
    dict ["Foo", null; "Bar", null] |> SqlMapper.IsNullRecord |> Should.beTrue

    // It should return `false` if at least one value of the specified dictionary is not `null`.
    dict ["Foo", "Bar" :> objnull; "Baz", null] |> SqlMapper.IsNullRecord |> Should.beFalse

  // [<TestMethod>]
  // member _.SplitOn() =
  //   let record = new List<KeyValuePair<string, object?>> {
  //     new("Id", 123),
  //     new("LongLabel", "Hello World!"),
  //     new("ShortLabel", null),
  //     new("Id", 456),
  //     new("FirstName", "Cédric"),
  //     new("LastName", "Belin"),
  //     new("RowID", 789)
  //   }

  //   var properties = new Dictionary<string, object?> {
  //     ["Id"] = 123,
  //     ["LongLabel"] = "Hello World!",
  //     ["ShortLabel"] = null,
  //     ["FirstName"] = "Cédric",
  //     ["LastName"] = "Belin",
  //     ["RowID"] = 789
  //   }

  //   // It should return a dictionary equivalent to the specified data row.
  //   let records = SqlMapper.SplitOn(record)
  //   Assert.HasCount(1, records)
  //   Assert.AreSequenceEqual(properties, records[0])

  //   // It should not split the data row if the specified field does not exist.
  //   records = SqlMapper.SplitOn(record, "_NonExistent_")
  //   Assert.HasCount(1, records)
  //   Assert.AreSequenceEqual(properties, records[0])

  //   // It should split the data row according to the specified fields.
  //   records = SqlMapper.SplitOn(record, "id")
  //   Assert.HasCount(2, records)
  //   Assert.AreSequenceEqual(new Dictionary<string, object?> { ["Id"] = 123, ["LongLabel"] = "Hello World!", ["ShortLabel"] = null }, records[0])
  //   Assert.AreSequenceEqual(new Dictionary<string, object?> { ["Id"] = 456, ["FirstName"] = "Cédric", ["LastName"] = "Belin", ["RowID"] = 789 }, records[1])

  //   records = SqlMapper.SplitOn(record, "id", "rowid", "_Unused_")
  //   Assert.HasCount(3, records)
  //   Assert.AreSequenceEqual(new Dictionary<string, object?> { ["Id"] = 123, ["LongLabel"] = "Hello World!", ["ShortLabel"] = null }, records[0])
  //   Assert.AreSequenceEqual(new Dictionary<string, object?> { ["Id"] = 456, ["FirstName"] = "Cédric", ["LastName"] = "Belin" }, records[1])
  //   Assert.AreSequenceEqual(new Dictionary<string, object?> { ["RowID"] = 789 }, records[2])
