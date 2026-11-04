namespace Belin.Sql

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Data
open System.Globalization
open System.Runtime.CompilerServices

/// Maps data records to entity objects.
type SqlMapper private () =

  /// The mapping between the entity types and their associated database tables.
  static let mapping = ConcurrentDictionary<Type, DbTableInfo>()

  /// The singleton instance of the data mapper.
  static member val Instance = SqlMapper()

  /// Converts the specified object into an equivalent value of the specified type.
  /// The `isNullable` argument indicates whether the specified conversion type is nullable.
  member _.ChangeType(value: objnull, conversionType: Type, ?isNullable: bool): objnull =
    match value with
    | null -> SqlMapper.GetNullEquivalent(conversionType, defaultArg isNullable false)
    | nullValue when Convert.IsDBNull nullValue -> SqlMapper.GetNullEquivalent(conversionType, defaultArg isNullable false)
    | nonNullValue -> SqlMapper.GetNonNullEquivalent(nonNullValue, conversionType)

  /// Converts the specified object into an equivalent value of the specified column type.
  member this.ChangeType(value: objnull, column: DbColumnInfo) =
    this.ChangeType(value, column.PropertyType, column.IsNullable)

  /// Creates a new object of a given type from the specified dictionary.
  // member _.CreateInstance(entityType: Type, record: IDictionary<string, objnull>) =
  //   ()

  /// Gets the table information associated with the specified type.
  member _.GetTable(entityType: Type) = mapping.GetOrAdd(entityType, fun entityType -> DbTableInfo entityType)

  /// Gets the table information associated with the specified type.
  member this.GetTable<'T when 'T: (new: unit -> 'T)>() = this.GetTable typeof<'T>

  /// Returns a value indicating whether all values of the specified dictionary are `null`.
  static member internal IsNullRecord(dictionary: IDictionary<string, objnull>) =
    dictionary.Values |> Seq.forall isNull

  /// Splits the specified data record according to the specified fields.
  static member internal SplitOn(record: ResizeArray<string * objnull>, [<ParamArray>] fields: string array): ResizeArray<Dictionary<string, objnull>> =
    if fields.Length = 0 then
      let properties = Dictionary<string, objnull> record.Count
      for key, value in record do properties.TryAdd(key, value) |> ignore<bool>
      ResizeArray [properties]
    else
      let fieldQueue = Queue<string> fields
      let mutable properties = Dictionary<string, objnull> record.Count
      let records = ResizeArray<Dictionary<string, objnull>>(fields.Length + 1)
      let mutable splitOn = fieldQueue.Dequeue()

      for index, (key, value) in record |> Seq.indexed do
        if index > 0 && key.Equals(splitOn, StringComparison.OrdinalIgnoreCase) then
          records.Add properties
          properties <- Dictionary<string, objnull>(record.Count - index)
          match fieldQueue.TryDequeue() with
          | false, _ -> ()
          | true, field -> splitOn <- field
        properties.TryAdd(key, value) |> ignore<bool>
      records.Add properties
      records

  /// Splits the specified data record according to the specified fields.
  static member internal SplitOn(record: IDataRecord, [<ParamArray>] fields: string array): ResizeArray<Dictionary<string, objnull>> =
    let properties = ResizeArray<string * objnull> record.FieldCount
    for index = 0 to record.FieldCount - 1 do
      let value = record[index]
      properties.Add (record.GetName index, if Convert.IsDBNull value then null else value)
    SqlMapper.SplitOn(properties, fields)

  /// Gets the equivalent value of the specified type corresponding to a given non-`NULL` value.
  static member private GetNonNullEquivalent(value: obj, conversionType: Type): obj =
    let targetType = Nullable.GetUnderlyingType conversionType |> defaultIfNull conversionType
    if targetType.IsEnum then
      match value with
      | :? string as stringValue -> Enum.Parse(targetType, stringValue, ignoreCase = true)
      | enumValue -> Enum.ToObject(targetType, Convert.ChangeType(enumValue, Enum.GetUnderlyingType targetType, CultureInfo.InvariantCulture) |> nonNull)
    elif targetType.IsInstanceOfType value then
      value
    else
      Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture) |> nonNull

  /// Gets the equivalent value of the specified type corresponding to a `NULL` value.
  static member private GetNullEquivalent(conversionType: Type, isNullable: bool): objnull =
    let nullableType = Nullable.GetUnderlyingType conversionType
    let targetType = nullableType |> defaultIfNull conversionType

    if not (isNull nullableType) then null
    elif targetType.IsValueType then RuntimeHelpers.GetUninitializedObject targetType
    elif targetType = typeof<string> then if isNullable then null else ""
    elif isNullable then null
    else Activator.CreateInstance targetType
