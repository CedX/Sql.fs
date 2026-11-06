namespace Belin.Sql
// fsharplint:disable MaxCharactersOnLine

open Microsoft.FSharp.Reflection
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Data
open System.Globalization
open System.Runtime.CompilerServices

/// Maps data records to entity objects.
type Mapper private () =

  /// The mapping between the entity types and their associated database tables.
  static let mapping = ConcurrentDictionary<Type, TableInfo>()

  /// The singleton instance of the data mapper.
  static member val Instance = Mapper()

  /// Converts the specified object into an equivalent value of the specified type.
  /// The `isNullable` argument indicates whether the specified conversion type is nullable.
  member _.ChangeType(value: objnull, conversionType: Type, ?isNullable: bool): objnull =
    match value with
    | null -> Mapper.GetNullEquivalent(conversionType, defaultArg isNullable false)
    | nullValue when Convert.IsDBNull nullValue -> Mapper.GetNullEquivalent(conversionType, defaultArg isNullable false)
    | nonNullValue -> Mapper.GetNonNullEquivalent(nonNullValue, conversionType)

  /// Converts the specified object into an equivalent value of the specified column type.
  member this.ChangeType(value: objnull, column: ColumnInfo): objnull =
    this.ChangeType(value, column.PropertyType, column.IsNullable)

  /// Gets the table information associated with the specified type.
  member _.GetTable<'T>() =
    mapping.GetOrAdd(typeof<'T>, TableInfo)

  /// Creates a new object of a given type from the specified dictionary.
  member this.CreateInstance(record: IDictionary<string, objnull>): 'T =
    if typeof<'T> = typeof<Map<string, objnull>> then
      record
      |> Seq.map (fun entry -> entry.Key, entry.Value)
      |> Map.ofSeq
      |> fun map -> (map :> obj) :?> 'T
    else
      let instance = Activator.CreateInstance<'T>()
      let table = this.GetTable<'T>()
      for name in record.Keys |> Seq.filter table.Columns.ContainsKey do
        let column = table.Columns[name]
        if column.CanWrite then column.SetValue(instance, this.ChangeType(record[name], column))
      instance

  /// Creates a new map from the specified dictionary.
  member this.CreateInstance(record: IDictionary<string, objnull>): Map<string, objnull> =
    this.CreateInstance<Map<string, objnull>> record

  /// Creates a new object of the given type from the specified data record.
  member this.CreateInstance(record: IDataRecord): 'T =
    this.CreateInstance<'T>(Mapper.SplitOn record |> Seq.exactlyOne)

  /// Creates a new map from the specified data record.
  member this.CreateInstance(record: IDataRecord): Map<string, objnull> =
    this.CreateInstance<Map<string, objnull>> record

  /// Creates a new object pair of the given types from the specified data record.
  member this.CreateInstance(record: IDataRecord, ?splitOn: string): 'Item1 * 'Item2 =
    let records = Mapper.SplitOn(record, defaultArg splitOn "Id")
    let item1 = if Mapper.IsNullRecord(records[0]) then Unchecked.defaultof<'Item1> else this.CreateInstance<'Item1>(records[0])
    let item2 = if records.Count <= 1 || Mapper.IsNullRecord(records[1]) then Unchecked.defaultof<'Item2> else this.CreateInstance<'Item2>(records[1])
    item1, item2

  /// Creates a new object tuple of the given types from the specified data record.
  member this.CreateInstance(record: IDataRecord, ?splitOn: string * string): 'Item1 * 'Item2 * 'Item3 =
    let firstField, secondField = defaultArg splitOn ("Id", "Id")
    let records = Mapper.SplitOn(record, firstField, secondField)
    let item1 = if Mapper.IsNullRecord(records[0]) then Unchecked.defaultof<'Item1> else this.CreateInstance<'Item1>(records[0])
    let item2 = if records.Count <= 1 || Mapper.IsNullRecord(records[1]) then Unchecked.defaultof<'Item2> else this.CreateInstance<'Item2>(records[1])
    let item3 = if records.Count <= 2 || Mapper.IsNullRecord(records[2]) then Unchecked.defaultof<'Item3> else this.CreateInstance<'Item3>(records[2])
    item1, item2, item3

  /// Creates a new object tuple of the given types from the specified data record.
  member this.CreateInstance(record: IDataRecord, ?splitOn: string * string * string): 'Item1 * 'Item2 * 'Item3 * 'Item4 =
    let firstField, secondField, thirdField = defaultArg splitOn ("Id", "Id", "Id")
    let records = Mapper.SplitOn(record, firstField, secondField, thirdField)
    let item1 = if Mapper.IsNullRecord(records[0]) then Unchecked.defaultof<'Item1> else this.CreateInstance<'Item1>(records[0])
    let item2 = if records.Count <= 1 || Mapper.IsNullRecord(records[1]) then Unchecked.defaultof<'Item2> else this.CreateInstance<'Item2>(records[1])
    let item3 = if records.Count <= 2 || Mapper.IsNullRecord(records[2]) then Unchecked.defaultof<'Item3> else this.CreateInstance<'Item3>(records[2])
    let item4 = if records.Count <= 3 || Mapper.IsNullRecord(records[3]) then Unchecked.defaultof<'Item4> else this.CreateInstance<'Item4>(records[3])
    item1, item2, item3, item4

  /// Creates new objects of the given type from the specified data reader.
  member this.CreateInstances(reader: IDataReader): 'T seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance<'T> record
  }

  /// Creates new maps from the specified data reader.
  member this.CreateInstances(reader: IDataReader): Map<string, objnull> seq =
    this.CreateInstances<Map<string, objnull>> reader

  /// Creates new object pairs of the given types from the specified data reader.
  member this.CreateInstances(reader: IDataReader, ?splitOn: string): ('Item1 * 'Item2) seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance<'Item1, 'Item2>(record, ?splitOn = splitOn)
  }

  /// Creates new object tuples of the given types from the specified data reader.
  member this.CreateInstances(reader: IDataReader, ?splitOn: string * string): ('Item1 * 'Item2 * 'Item3) seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance<'Item1, 'Item2, 'Item3>(record, ?splitOn = splitOn)
  }

  /// Creates new object tuples of the given types from the specified data reader.
  member this.CreateInstances(reader: IDataReader, ?splitOn: string * string * string): ('Item1 * 'Item2 * 'Item3 * 'Item4) seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance<'Item1, 'Item2, 'Item3, 'Item4>(record, ?splitOn = splitOn)
  }

  /// Returns a value indicating whether all values of the specified dictionary are `null`.
  static member internal IsNullRecord(dictionary: IDictionary<string, objnull>) =
    dictionary.Values |> Seq.forall isNull

  /// Splits the specified data record according to the specified fields.
  static member internal SplitOn(record: IList<string * objnull>, [<ParamArray>] fields: string array): ResizeArray<Dictionary<string, objnull>> =
    if fields.Length = 0 then
      let properties = Dictionary<string, objnull> record.Count
      for key, value in record do properties.TryAdd(key, value) |> ignore<bool>
      ResizeArray [properties]
    else
      let fieldQueue = Queue<string> fields
      let mutable properties = Dictionary<string, objnull> record.Count
      let records = ResizeArray<Dictionary<string, objnull>>(fields.Length + 1)
      let mutable splitOn = fieldQueue.Dequeue()

      for index, (key, value) in Seq.indexed record do
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
      properties.Add((record.GetName index, if Convert.IsDBNull value then null else value))
    Mapper.SplitOn(properties, fields)

  /// Creates a `Some` option of the specified type for the given value.
  static member private CreateSome(optionType: Type, value: obj): obj =
    let someCase = FSharpType.GetUnionCases optionType |> Array.find (fun unionCase -> unionCase.Name = "Some")
    FSharpValue.MakeUnion(someCase, [| value |]) |> nonNull

  /// Gets the equivalent value of the specified type corresponding to a given non-`NULL` value.
  static member private GetNonNullEquivalent(value: obj, conversionType: Type): obj =
    let targetType = Nullable.GetUnderlyingType conversionType |> defaultIfNull conversionType
    if targetType.IsInstanceOfType value then
      value
    elif targetType.IsEnum then
      match value with
      | :? string as stringValue -> Enum.Parse(targetType, stringValue, ignoreCase = true)
      | enumValue -> Enum.ToObject(targetType, Convert.ChangeType(enumValue, Enum.GetUnderlyingType targetType, CultureInfo.InvariantCulture) |> nonNull)
    elif Mapper.IsOptionType targetType then
      Mapper.CreateSome(targetType, Mapper.GetNonNullEquivalent(value, targetType.GenericTypeArguments[0]))
    else
      Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture) |> nonNull

  /// Gets the equivalent value of the specified type corresponding to a `NULL` value.
  static member private GetNullEquivalent(conversionType: Type, isNullable: bool): objnull =
    let nullableType = Nullable.GetUnderlyingType conversionType
    let targetType = nullableType |> defaultIfNull conversionType

    if not (isNull nullableType) then null
    elif targetType.IsValueType then RuntimeHelpers.GetUninitializedObject targetType
    elif targetType = typeof<string> then if isNullable then null else ""
    elif Mapper.IsOptionType targetType then if isNullable then null else None
    else if isNullable then null else Activator.CreateInstance targetType

  /// Returns a value indicating whether the specified type is `'T option` or `'T voption`.
  static member private IsOptionType(targetType: Type): bool =
    if not targetType.IsGenericType then false
    else
      let typeDef = targetType.GetGenericTypeDefinition()
      typeDef = typedefof<option<_>> || typeDef = typedefof<voption<_>>
