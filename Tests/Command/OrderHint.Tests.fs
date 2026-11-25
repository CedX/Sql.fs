namespace Belin.Sql

open Belin.Should
open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Collections.Generic

/// Tests the features of the `OrderHintCollection` class.
[<TestClass>]
type OrderHintCollectionTests() =

  [<TestMethod>]
  member _.Constructor() =
    // It should create an empty collection by default.
    let collection = OrderHintCollection()
    Should.beEmpty collection

    // It should create a collection from a single order hint.
    let collection = OrderHintCollection { Column = "ID"; SortOrder = Descending }
    collection.Count |> Should.be 1

    let orderHint = Seq.head collection
    orderHint.Column |> Should.be "ID"
    orderHint.SortOrder |> Should.be Descending

    // It should create a collection from a list of order hints.
    let collection = OrderHintCollection({ Column = "ID"; SortOrder = Descending }, { Column = "Name"; SortOrder = Ascending})
    collection.Count |> Should.be 2

    let orderHint = Seq.last collection
    orderHint.Column |> Should.be "Name"
    orderHint.SortOrder |> Should.be Ascending

    // It should create a collection from a sequence of order-like arguments.
    let collection = OrderHintCollection ["FirstName", Ascending; "LastName", Descending]
    collection.Count |> Should.be 2
    collection |> Seq.map _.Column |> Should.beSequence ["FirstName"; "LastName"]
    collection |> Seq.map _.SortOrder |> Should.beSequence [Ascending; Descending]

    let collection = OrderHintCollection [| "FirstName"; "LastName" |]
    collection.Count |> Should.be 2
    collection |> Seq.map _.Column |> Should.beSequence ["FirstName"; "LastName"]
    collection |> Seq.map _.SortOrder |> Should.beSequence [Ascending; Ascending]

  [<TestMethod>]
  member _.Indexer() =
    let collection = OrderHintCollection ["ID", Descending; "Name", Ascending]

    // It should return the order hint with the specified column name.
    let orderHint = collection["id"]
    orderHint.Column |> Should.be "ID"
    orderHint.SortOrder |> Should.be Descending
    collection[0] |> Should.be orderHint

    // It should throw an error if the specified column does not exist.
    Should.throw<KeyNotFoundException> (fun () -> collection["foo"] |> ignore<OrderHint>)

  [<TestMethod>]
  member _.Add() =
    let collection = OrderHintCollection()
    Should.beEmpty collection

    let parameter = collection.Add("Name1", Descending)
    collection.Count |> Should.be 1
    parameter.Column |> Should.be "Name1"
    parameter.SortOrder |> Should.be Descending

    let parameter = collection.Add "Name2"
    collection.Count |> Should.be 2
    parameter.Column |> Should.be "Name2"
    parameter.SortOrder |> Should.be Ascending

  [<TestMethod>]
  member _.Contains() =
    let collection = OrderHintCollection { Column = "Key"; SortOrder = Ascending }
    collection.Contains "key" |> Should.beTrue
    collection.Contains "KEY" |> Should.beTrue
    collection.Contains "foo" |> Should.beFalse

  [<TestMethod>]
  member _.IndexOf() =
    let collection = OrderHintCollection ["ID", Descending; "Name", Ascending]
    collection.IndexOf "id" |> Should.be 0
    collection.IndexOf "name" |> Should.be 1
    collection.IndexOf "foo" |> Should.be -1

  [<TestMethod>]
  member _.Parse() =
    // It should return an empty sort for an empty string.
    OrderHintCollection.Parse "" |> Should.beEmpty

    // It should return an ascending direction for a property without prefix, a descending direction for a property with a "-" prefix.
    let expected = OrderHintCollection ["foo", Ascending; "bar", Descending]
    OrderHintCollection.Parse "foo,-bar" |> Should.beSequence expected

  [<TestMethod>]
  member _.RemoveAt() =
    // It should remove the order hint with the specified column name.
    let collection = OrderHintCollection ["ID", Descending; "Name", Ascending]
    collection.Count |> Should.be 2
    collection.RemoveAt "name"
    collection.Count |> Should.be 1
    collection.RemoveAt "id"
    Should.beEmpty collection

    // It should throw an error if the specified column does not exist.
    let collection = OrderHintCollection ["ID", Descending; "Name", Ascending]
    Should.throw<KeyNotFoundException> (fun () -> collection.RemoveAt "Foo")

  [<TestMethod>]
  member _.TestToString() =
    // It should return an empty string for an empty sort.
    OrderHintCollection() |> string |> Should.beEmpty

    // It should return the property for an ascending direction.
    OrderHintCollection [("foo", Ascending)]|> string |> Should.be "foo"

    // It should return the property with a "-" prefix for a descending direction.
    OrderHintCollection.Parse "foo,-bar" |> string |> Should.be "foo,-bar"
