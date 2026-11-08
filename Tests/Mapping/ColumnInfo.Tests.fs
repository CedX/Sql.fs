namespace Belin.Sql

open Belin.Should
open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Data

/// Tests the features of the `ColumnInfo` class.
[<TestClass>]
type ColumnInfoTests() =

  [<TestMethod>]
  [<DataRow "FirstName">]
  [<DataRow "FullName">]
  [<DataRow "Gender">]
  [<DataRow "Id">]
  member _.CanRead(name: string) =
    Should.beTrue (ColumnInfo(nonNull (typeof<Character>.GetProperty name)).CanRead)

  [<TestMethod>]
  [<DataRow "FirstName">]
  [<DataRow "FullName">]
  [<DataRow "Gender">]
  [<DataRow "Id">]
  member _.CanWrite(name: string) =
    Should.beTrue (ColumnInfo(nonNull (typeof<Character>.GetProperty name)).CanWrite)

  [<TestMethod>]
  [<DataRow("FirstName", DbType.String)>]
  [<DataRow("FullName", DbType.String)>]
  [<DataRow("Gender", DbType.AnsiString)>]
  [<DataRow("Id", DbType.Int32)>]
  member _.DbType(name: string, expected: DbType) =
    ColumnInfo(nonNull (typeof<Character>.GetProperty name)).DbType |> Should.be expected

  [<TestMethod>]
  [<DataRow("FirstName", false)>]
  [<DataRow("FullName", true)>]
  [<DataRow("Gender", false)>]
  [<DataRow("Id", true)>]
  member _.IsComputed(name: string, expected: bool) =
    ColumnInfo(nonNull (typeof<Character>.GetProperty name)).IsComputed |> Should.be expected

  [<TestMethod>]
  [<DataRow("FirstName", false)>]
  [<DataRow("FullName", false)>]
  [<DataRow("Gender", false)>]
  [<DataRow("Id", true)>]
  member _.IsIdentity(name: string, expected: bool) =
    ColumnInfo(nonNull (typeof<Character>.GetProperty name)).IsIdentity |> Should.be expected

  [<TestMethod>]
  [<DataRow("FirstName", false)>]
  [<DataRow("FullName", true)>]
  [<DataRow("Gender", false)>]
  [<DataRow("Id", false)>]
  member _.IsNullable(name: string, expected: bool) =
    ColumnInfo(nonNull (typeof<Character>.GetProperty name)).IsNullable |> Should.be expected

  [<TestMethod>]
  [<DataRow("FirstName", "firstName")>]
  [<DataRow("FullName", "fullName")>]
  [<DataRow("Gender", "gender")>]
  [<DataRow("Id", "ID")>]
  member _.Name(name: string, expected: string) =
    ColumnInfo(nonNull (typeof<Character>.GetProperty name)).Name |> Should.be expected

  [<TestMethod>]
  [<DataRow("FirstName", typeof<string>)>]
  [<DataRow("FullName", typeof<string option>)>]
  [<DataRow("Gender", typeof<CharacterGender>)>]
  [<DataRow("Id", typeof<int>)>]
  member _.PropertyType(name: string, expected: Type) =
    ColumnInfo(nonNull (typeof<Character>.GetProperty name)).PropertyType |> Should.be expected

  [<TestMethod>]
  member _.GetValue() =
    let record = { Character.Default with FirstName = "Cédric"; LastName = "Belin" }
    ColumnInfo(nonNull (typeof<Character>.GetProperty "FirstName")).GetValue record |> Should.be "Cédric"
    ColumnInfo(nonNull (typeof<Character>.GetProperty "LastName")).GetValue record |> Should.be "Belin"

  [<TestMethod>]
  member _.SetValue() =
    let record = { Character.Default with FirstName = "Cédric"; LastName = "Belin" }
    ColumnInfo(nonNull (typeof<Character>.GetProperty "FirstName")).SetValue(record, "Don")
    ColumnInfo(nonNull (typeof<Character>.GetProperty "LastName")).SetValue(record, "Syme")
    record.FirstName |> Should.be "Don"
    record.LastName |> Should.be "Syme"
