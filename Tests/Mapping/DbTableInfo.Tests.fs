namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

/// Tests the features of the `DbTableInfo` class.
[<TestClass>]
type DbTableInfoTests() =

  [<TestMethod>]
  member _.Columns() =
    DbTableInfo(typeof<ConsoleKeyInfo>).Columns |> Should.beEmpty

    let columns = DbTableInfo(typeof<Character>).Columns
    columns.Count |> Should.be 5
    columns.Keys |> Should.beSequence ["firstName"; "fullName"; "gender"; "ID"; "lastName"]

  [<TestMethod>]
  member _.IdentityColumn() =
    DbTableInfo(typeof<ConsoleKeyInfo>).IdentityColumn |> Should.beNone

    let identityColumn = DbTableInfo(typeof<Character>).IdentityColumn
    identityColumn |> Should.beSome
    identityColumn |> Option.get |> _.Name |> Should.be "ID"

  [<TestMethod>]
  member _.Name() =
    // It should return the class name when there is no [Table] attribute.
    DbTableInfo(typeof<ConsoleKeyInfo>).Name |> Should.be typeof<ConsoleKeyInfo>.Name

    // It should return the value of the [Table] attribute when it is present.
    DbTableInfo(typeof<Character>).Name |> Should.be "Characters"

  [<TestMethod>]
  member _.Schema() =
    // It should return `null` when there is no [Table] attribute.
    DbTableInfo(typeof<ConsoleKeyInfo>).Schema |> Should.beNone

    // It should return the value of the [Table] attribute when it is present.
    DbTableInfo(typeof<Character>).Schema |> Should.beSomeValue "main"
