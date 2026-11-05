namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Collections.Generic
open System.Data

/// Tests the features of the `Parameter` class.
[<TestClass>]
type ParameterTests() =

  [<TestMethod>]
  [<DataRow("", "?")>]
  [<DataRow("?", "?")>]
  [<DataRow("?1", "?1")>]
  [<DataRow("foo", "@foo")>]
  [<DataRow("@bar", "@bar")>]
  [<DataRow(":baz", ":baz")>]
  [<DataRow("$qux", "$qux")>]
  member _.Name(name: string, expected: string) =
    Parameter(name).Name |> Should.be expected

  [<TestMethod>]
  member _.Value() =
    Parameter("Name", null).Value |> Should.be DBNull.Value
    Parameter("Name", DBNull.Value).Value |> Should.be DBNull.Value
    Parameter("Name", 123).Value |> Should.be 123
    Parameter("Name", -123.456).Value |> Should.be -123.456
    Parameter("Name", "").Value |> Should.be ""
    Parameter("Name", "Foo").Value |> Should.be "Foo"
    Parameter("Name", DateTime.UnixEpoch).Value |> Should.be DateTime.UnixEpoch

/// Tests the features of the `ParameterCollection` class.
[<TestClass>]
type ParameterCollectionTests() =

  [<TestMethod>]
  member _.Constructor() =
    // It should create an empty collection by default.
    let collection = ParameterCollection()
    Should.beEmpty collection

    // It should create a collection from a single parameter.
    let collection = ParameterCollection(Parameter("?1", 123, DbType = Some DbType.Int64))
    collection.Count |> Should.be 1

    let parameter = Seq.head collection
    parameter.Name |> Should.be "?1"
    parameter.Value |> Should.be 123
    parameter.DbType |> Should.beSome DbType.Int64

    // It should create a collection from a list of parameters.
    let collection = ParameterCollection(Parameter("?1", 123), Parameter("@Key", "Unique", DbType = Some DbType.AnsiString))
    collection.Count |> Should.be 2

    let parameter = Seq.last collection
    parameter.Name |> Should.be "@Key"
    parameter.Value |> Should.be "Unique"
    parameter.DbType |> Should.beSome DbType.AnsiString

  [<TestMethod>]
  member _.Indexer() =
    let collection = ParameterCollection ["?1", 123 :> objnull; "@Key", "Unique"]

    // It should return the parameter with the specified name.
    let parameter = collection["Key"]
    parameter.Name |> Should.be "@Key"
    parameter.Value |> Should.be "Unique"
    collection[1] |> Should.be parameter

    // It should throw an error if the specified name does not exist.
    (fun () -> collection["@Foo"] |> ignore<Parameter>) |> Should.throw typeof<KeyNotFoundException>

  [<TestMethod>]
  member _.AddWithValue() =
    let collection = ParameterCollection()
    Should.beEmpty collection

    let parameter = collection.AddWithValue("Name", "Value1")
    collection.Count |> Should.be 1
    parameter.Name |> Should.be "@Name"
    parameter.Value |> Should.be "Value1"

    let parameter = collection.AddWithValue "Value2"
    collection.Count |> Should.be 2
    parameter.Name |> Should.be "?2"
    parameter.Value |> Should.be "Value2"

  [<TestMethod>]
  member _.Contains() =
    let collection = ParameterCollection(Parameter("@Key", null))
    collection.Contains "Key" |> Should.beTrue
    collection.Contains "@Key" |> Should.beTrue
    collection.Contains "Foo" |> Should.beFalse
    collection.Contains "@Foo" |> Should.beFalse

  [<TestMethod>]
  member _.IndexOf() =
    let collection = ParameterCollection ["?1", 123 :> objnull; "@Key", "Unique"]
    collection.IndexOf "Key" |> Should.be 1
    collection.IndexOf "@Key" |> Should.be 1
    collection.IndexOf "Foo" |> Should.be -1
    collection.IndexOf "@Foo" |> Should.be -1

  [<TestMethod>]
  member _.RemoveAt() =
    // It should remove the parameter with the specified name.
    let collection = ParameterCollection ["?1", 123 :> objnull; "@Key", "Unique"]
    collection.Count |> Should.be 2
    collection.RemoveAt "Key"
    collection.Count |> Should.be 1
    collection.RemoveAt "?1"
    Should.beEmpty collection

    // It should throw an error if the specified name does not exist.
    let collection = ParameterCollection ["?1", 123 :> objnull; "@Key", "Unique"]
    (fun () -> collection.RemoveAt "Foo") |> Should.throw typeof<KeyNotFoundException>
