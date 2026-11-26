namespace Belin.Sql

open System
open System.Collections.Generic
open System.Text.RegularExpressions

/// Specifies how rows of data are sorted.
type SortOrder =
  | Ascending
  | Descending

/// Defines the sort order for a database column.
type OrderHint = {

  /// The name of the column for which the hint is being provided.
  Column: string

  /// The sort order of the column.
  SortOrder: SortOrder
}

/// A collection of hints describing the sort order of columns.
type OrderHintCollection([<ParamArray>] orderHints: OrderHint array) =
  inherit ResizeArray<OrderHint>(orderHints)

  /// The regular expression used to remove the sort order.
  static let sortOrderPattern = Regex(@" (ASC|DESC)$", RegexOptions.IgnoreCase)

  /// Creates a new order hint collection from the specified sequence of order hints.
  new(orderHints: OrderHint seq) =
    OrderHintCollection(Array.ofSeq orderHints)

  /// Creates a new order hint collection from the specified sequence of order hints.
  new(orderHints: (string * SortOrder) seq) =
    OrderHintCollection(orderHints |> Seq.map(fun (column, sortOrder) -> { Column = column; SortOrder = sortOrder }) |> Array.ofSeq)

  /// Creates a new order hint collection from the specified sequence of column names.
  new(columns: string seq) =
    OrderHintCollection(columns |> Seq.map(fun column -> { Column = column; SortOrder = Ascending }) |> Array.ofSeq)

  /// Gets the order hint with the specified column name.
  member this.Item with get(column: string) =
    match this.Find(fun orderHint -> orderHint.Column.Equals(column, StringComparison.OrdinalIgnoreCase)) with
    | Null -> raise (KeyNotFoundException column)
    | NonNull orderHint -> orderHint

  /// Adds a new order hint to the end of this collection.
  /// Returns the newly added order hint.
  member this.Add(column: string, ?sortOrder: SortOrder): OrderHint =
    let orderHint = { Column = column; SortOrder = defaultArg sortOrder Ascending }
    this.Add orderHint
    orderHint

  /// Gets a value indicating whether an order hint in this collection has the specified column name.
  member this.Contains(column: string): bool =
    this.Exists(fun orderHint -> orderHint.Column.Equals(column, StringComparison.OrdinalIgnoreCase))

  /// Returns the index of the order hint with the specified column name.
  member this.IndexOf(column: string): int =
    this.FindIndex(fun orderHint -> orderHint.Column.Equals(column, StringComparison.OrdinalIgnoreCase))

  /// Creates a new collection from the specified string.
  static member Parse(value: string): OrderHintCollection =
    value.Split(',', StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
    |> Array.map (fun token ->
      let column = sortOrderPattern.Replace(token, "")
      let sortOrder = if token.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase) then Descending else Ascending
      { Column = column; SortOrder = sortOrder })
    |> OrderHintCollection

  /// Removes the order hint with the specified column name from this collection.
  member this.RemoveAt(column: string) =
    try this.IndexOf column |> this.RemoveAt
    with :? ArgumentOutOfRangeException as ex -> raise (KeyNotFoundException(column, ex))

  /// Returns a string representation of this object.
  override this.ToString() =
    this
    |> Seq.map (fun orderHint -> $"""{orderHint.Column} {if orderHint.SortOrder = Descending then "DESC" else "ASC"}""")
    |> String.concat ", "
