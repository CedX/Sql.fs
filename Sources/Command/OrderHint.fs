namespace Belin.Sql

open System
open System.Collections.Generic

/// Specifies how rows of data are sorted.
type SortOrder =
  | Ascending
  | Descending

/// Defines the sort order for a database column.
type OrderHint(column: string, ?sortOrder: SortOrder) =

  /// The name of the column for which the hint is being provided.
  member val Column: string = column with get, set

  /// The sort order of the column.
  member val SortOrder: SortOrder = defaultArg sortOrder Ascending with get, set

/// A collection of hints describing the sort order of columns.
type OrderHintCollection([<ParamArray>] orderHints: OrderHint array) =
  inherit ResizeArray<OrderHint>(orderHints)

  /// Creates a new order hint collection from the specified sequence of order hints.
  new(orderHints: OrderHint seq) =
    OrderHintCollection(Array.ofSeq orderHints)

  /// Creates a new order hint collection from the specified sequence of order hints.
  new(orderHints: (string * SortOrder) seq) =
    OrderHintCollection(orderHints |> Seq.map(fun (column, sortOrder) -> OrderHint(column, sortOrder)) |> Array.ofSeq)

  /// Creates a new order hint collection from the specified sequence of column names.
  new(columns: string seq) =
    OrderHintCollection(columns |> Seq.map(fun column -> OrderHint(column, Ascending)) |> Array.ofSeq)

  /// Gets the order hint with the specified column name.
  member this.Item with get(column: string) =
    match this.Find(fun orderHint -> orderHint.Column.Equals(column, StringComparison.OrdinalIgnoreCase)) with
    | null -> raise (KeyNotFoundException column)
    | orderHint -> orderHint

  /// Adds a new order hint to the end of this collection.
  /// Returns the newly added order hint.
  member this.AddOrderHint(column: string, ?sortOrder: SortOrder): OrderHint =
    let orderHint = OrderHint(column, defaultArg sortOrder Ascending)
    this.Add orderHint
    orderHint

  /// Gets a value indicating whether an order hint in this collection has the specified column name.
  member this.Contains(column: string): bool =
    this.Exists(fun orderHint -> orderHint.Column.Equals(column, StringComparison.OrdinalIgnoreCase))

  /// Returns the index of the order hint with the specified column name.
  member this.IndexOf(column: string): int =
    this.FindIndex(fun orderHint -> orderHint.Column.Equals(column, StringComparison.OrdinalIgnoreCase))

  /// Removes the order hint with the specified column name from this collection.
  member this.RemoveAt(column: string) =
    try this.IndexOf column |> this.RemoveAt
    with :? ArgumentOutOfRangeException as ex -> raise (KeyNotFoundException(column, ex))
