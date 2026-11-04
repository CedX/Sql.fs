namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

/// Tests the features of the `TableInfo` class.
[<TestClass>]
type TableInfoTests() =

  [<TestMethod>]
  member _.Columns() =
    TableInfo(typeof<ConsoleKeyInfo>).Columns |> Should.beEmpty

    let columns = TableInfo(typeof<Character>).Columns
    columns.Count |> Should.be 5
    columns.Keys |> Should.beSequence ["firstName"; "fullName"; "gender"; "ID"; "lastName"]

  [<TestMethod>]
  member _.IdentityColumn() =
    TableInfo(typeof<ConsoleKeyInfo>).IdentityColumn |> Should.beNone

    let identityColumn = TableInfo(typeof<Character>).IdentityColumn
    identityColumn |> Should.beSome
    identityColumn |> Option.get |> _.Name |> Should.be "ID"

  [<TestMethod>]
  member _.Name() =
    // It should return the type name when there is no [Table] attribute.
    TableInfo(typeof<ConsoleKeyInfo>).Name |> Should.be typeof<ConsoleKeyInfo>.Name

    // It should return the value of the [Table] attribute when it is present.
    TableInfo(typeof<Character>).Name |> Should.be "Characters"

  [<TestMethod>]
  member _.Schema() =
    // It should return `None` when there is no [Table] attribute.
    TableInfo(typeof<ConsoleKeyInfo>).Schema |> Should.beNone

    // It should return the value of the [Table] schema when it is present.
    TableInfo(typeof<Character>).Schema |> Should.beSomeValue "main"
