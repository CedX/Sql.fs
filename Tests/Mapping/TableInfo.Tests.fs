namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

/// Tests the features of the `TableInfo` class.
[<TestClass>]
type TableInfoTests() =

  [<TestMethod>]
  member _.Columns() =
    Should.beEmpty (TableInfo(typeof<ConsoleKeyInfo>).Columns)

    let columns = TableInfo(typeof<Character>).Columns
    columns.Count |> Should.be 5
    columns.Keys |> Should.beSequence ["firstName"; "fullName"; "gender"; "ID"; "lastName"]

  [<TestMethod>]
  member _.IdentityColumn() =
    Should.beNone (TableInfo(typeof<ConsoleKeyInfo>).IdentityColumn)
    TableInfo(typeof<Character>).IdentityColumn |> Option.map _.Name |> Should.beSome "ID"

  [<TestMethod>]
  member _.Name() =
    // It should return the type name when there is no [Table] attribute.
    TableInfo(typeof<ConsoleKeyInfo>).Name |> Should.be typeof<ConsoleKeyInfo>.Name

    // It should return the value of the [Table] attribute when it is present.
    TableInfo(typeof<Character>).Name |> Should.be "Characters"

  [<TestMethod>]
  member _.Schema() =
    // It should return `None` when there is no [Table] attribute.
    Should.beNone (TableInfo(typeof<ConsoleKeyInfo>).Schema)

    // It should return the value of the [Table] schema when it is present.
    TableInfo(typeof<Character>).Schema |> Should.beSome "main"
