namespace Belin.Sql

open System
open System.Collections.Frozen
open System.Collections.Generic
open System.ComponentModel.DataAnnotations.Schema
open System.Reflection

/// Provides information about a database table.
type TableInfo(entityType: Type) =

  /// Creates new table information.
  let table = entityType.GetCustomAttribute<TableAttribute>() |> defaultIfNull (TableAttribute entityType.Name)
  let columns =
    query {
      for property in entityType.GetProperties(BindingFlags.Instance ||| BindingFlags.NonPublic ||| BindingFlags.Public) do
      where (not (property.IsDefined typeof<NotMappedAttribute>) && (property.IsDefined typeof<ColumnAttribute> || property.CanRead && property.CanWrite))
      select (ColumnInfo property)
    }
    |> Seq.map (fun column -> KeyValuePair(column.Name, column))
    |> Array.ofSeq
    |> fun source -> FrozenDictionary.Create<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase, source)

  /// The table columns.
  member val Columns: IReadOnlyDictionary<string, ColumnInfo> = columns

  /// The single identity column, if applicable.
  member val IdentityColumn: ColumnInfo option =
    match columns.Values |> Seq.filter (fun column -> column.IsIdentity) |> Seq.tryExactlyOne with
    | Some column -> Some column
    | None ->
      match columns.TryGetValue "Id" with
      | true, column -> Some column
      | false, _ -> None

  /// The table name.
  member _.Name: string = table.Name

  /// The table schema, if applicable.
  member val Schema: string option = Option.ofObj table.Schema

  /// The entity type associated with this table.
  member _.Type: Type = entityType
