namespace Belin.Sql
// fsharplint:disable MaxCharactersOnLine

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
  member _.GetTable(entityType: Type) =
    mapping.GetOrAdd(entityType, TableInfo)

  /// Gets the table information associated with the specified type.
  member this.GetTable<'T>() =
    this.GetTable typeof<'T>

  /// Creates a new object of a given type from the specified dictionary.
  member this.CreateInstance(entityType: Type, record: IDictionary<string, objnull>): obj =
    if entityType = typeof<Map<string, objnull>> then
      record |> Seq.map (fun entry -> entry.Key, entry.Value) |> Map.ofSeq :> obj
    else
      let instance = nonNull (Activator.CreateInstance entityType)
      let table = this.GetTable entityType
      for name in record.Keys |> Seq.filter table.Columns.ContainsKey do
        let column = table.Columns[name]
        if column.CanWrite then column.SetValue(instance, this.ChangeType(record[name], column))
      instance

  /// Creates a new object of a given type from the specified dictionary.
  member this.CreateInstance<'T>(record: IDictionary<string, objnull>): 'T =
    this.CreateInstance(typeof<'T>, record) :?> 'T

  /// Creates a new map from the specified dictionary.
  member this.CreateInstance(record: IDictionary<string, objnull>): Map<string, objnull> =
    this.CreateInstance(typeof<Map<string, objnull>>, record) :?> Map<string, objnull>

  /// Creates a new object of the given type from the specified data record.
  member this.CreateInstance(entityType: Type, record: IDataRecord): obj =
    this.CreateInstance(entityType, Mapper.SplitOn record |> Seq.head)

  /// Creates a new object of the given type from the specified data record.
  member this.CreateInstance<'T>(record: IDataRecord): 'T =
    this.CreateInstance<'T>(Mapper.SplitOn record |> Seq.head)

  /// Creates a new map from the specified data record.
  member this.CreateInstance(record: IDataRecord): Map<string, objnull> =
    this.CreateInstance<Map<string, objnull>>(Mapper.SplitOn record |> Seq.head)

  /// Creates a new object tuple of the given types from the specified data record.
  member this.CreateInstance(types: Type array, record: IDataRecord, [<ParamArray>] splitOn: string array): ITuple =
    if types.Length < 2 || types.Length > 7 then invalidArg (nameof types) "The number of object types is invalid."

    let fields =
      match splitOn.Length with
      | 0 -> types |> Array.skip 1 |> Array.map (fun _ -> "Id")
      | length when length <> types.Length - 1 -> invalidArg (nameof splitOn) "The number of split fields is invalid."
      | _ -> splitOn

    let records = Mapper.SplitOn(record, fields)
    let objects = ResizeArray<objnull> records.Count
    for index = 0 to types.Length - 1 do
      objects.Add(if records.Count <= index || Mapper.IsNullRecord(records[index]) then null else this.CreateInstance(types[index], records[index]))

    match objects.Count with
    | 2 -> objects[0], objects[1]
    | 3 -> objects[0], objects[1], objects[2]
    | 4 -> objects[0], objects[1], objects[2], objects[3]
    | 5 -> objects[0], objects[1], objects[2], objects[3], objects[4] // fsharplint:disable-line MaxNumberOfItemsInTuple
    | 6 -> objects[0], objects[1], objects[2], objects[3], objects[4], objects[5] // fsharplint:disable-line MaxNumberOfItemsInTuple
    | _ -> objects[0], objects[1], objects[2], objects[3], objects[4], objects[5], objects[6] // fsharplint:disable-line MaxNumberOfItemsInTuple

  /// Creates a new object pair of the given types from the specified data record.
  member this.CreateInstance<'TItem1, 'TItem2>(record: IDataRecord, ?splitOn: string): 'TItem1 * 'TItem2 =
    let records = Mapper.SplitOn(record, defaultArg splitOn "Id")
    let item1 = if Mapper.IsNullRecord(records[0]) then Unchecked.defaultof<'TItem1> else this.CreateInstance<'TItem1>(records[0])
    let item2 = if records.Count <= 1 || Mapper.IsNullRecord(records[1]) then Unchecked.defaultof<'TItem2> else this.CreateInstance<'TItem2>(records[1])
    item1, item2

  /// Creates a new object tuple of the given types from the specified data record.
  member this.CreateInstance<'TItem1, 'TItem2, 'TItem3>(record: IDataRecord, ?splitOn: string * string): 'TItem1 * 'TItem2 * 'TItem3 =
    let firstField, secondField = defaultArg splitOn ("Id", "Id")
    let records = Mapper.SplitOn(record, firstField, secondField)
    let item1 = if Mapper.IsNullRecord(records[0]) then Unchecked.defaultof<'TItem1> else this.CreateInstance<'TItem1>(records[0])
    let item2 = if records.Count <= 1 || Mapper.IsNullRecord(records[1]) then Unchecked.defaultof<'TItem2> else this.CreateInstance<'TItem2>(records[1])
    let item3 = if records.Count <= 2 || Mapper.IsNullRecord(records[2]) then Unchecked.defaultof<'TItem3> else this.CreateInstance<'TItem3>(records[2])
    item1, item2, item3

  /// Creates a new object tuple of the given types from the specified data record.
  member this.CreateInstance<'TItem1, 'TItem2, 'TItem3, 'TItem4>(record: IDataRecord, ?splitOn: string * string * string): 'TItem1 * 'TItem2 * 'TItem3 * 'TItem4 =
    let firstField, secondField, thirdField = defaultArg splitOn ("Id", "Id", "Id")
    let records = Mapper.SplitOn(record, firstField, secondField, thirdField)
    let item1 = if Mapper.IsNullRecord(records[0]) then Unchecked.defaultof<'TItem1> else this.CreateInstance<'TItem1>(records[0])
    let item2 = if records.Count <= 1 || Mapper.IsNullRecord(records[1]) then Unchecked.defaultof<'TItem2> else this.CreateInstance<'TItem2>(records[1])
    let item3 = if records.Count <= 2 || Mapper.IsNullRecord(records[2]) then Unchecked.defaultof<'TItem3> else this.CreateInstance<'TItem3>(records[2])
    let item4 = if records.Count <= 3 || Mapper.IsNullRecord(records[3]) then Unchecked.defaultof<'TItem4> else this.CreateInstance<'TItem4>(records[3])
    item1, item2, item3, item4

  /// Creates new objects of the given type from the specified data reader.
  member this.CreateInstances(entityType: Type, reader: IDataReader): obj seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance(entityType, record)
  }

  /// Creates new objects of the given type from the specified data reader.
  member this.CreateInstances<'T>(reader: IDataReader): 'T seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance<'T> record
  }

  /// Creates new maps from the specified data reader.
  member this.CreateInstances(reader: IDataReader): Map<string, objnull> seq =
    this.CreateInstances<Map<string, objnull>> reader

  /// Creates new object tuples of the given types from the specified data reader.
  member this.CreateInstances(types: Type array, reader: IDataReader, [<ParamArray>] splitOn: string array): ITuple seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance(types, record, splitOn)
  }

  /// Creates new object pairs of the given types from the specified data reader.
  member this.CreateInstances<'TItem1, 'TItem2>(reader: IDataReader, ?splitOn: string): ('TItem1 * 'TItem2) seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance<'TItem1, 'TItem2>(record, ?splitOn = splitOn)
  }

  /// Creates new object tuples of the given types from the specified data reader.
  member this.CreateInstances<'TItem1, 'TItem2, 'TItem3>(reader: IDataReader, ?splitOn: string * string): ('TItem1 * 'TItem2 * 'TItem3) seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance<'TItem1, 'TItem2, 'TItem3>(record, ?splitOn = splitOn)
  }

  /// Creates new object tuples of the given types from the specified data reader.
  member this.CreateInstances<'TItem1, 'TItem2, 'TItem3, 'TItem4>(reader: IDataReader, ?splitOn: string * string * string): ('TItem1 * 'TItem2 * 'TItem3 * 'TItem4) seq = seq {
    use record = reader
    while record.Read() do yield this.CreateInstance<'TItem1, 'TItem2, 'TItem3, 'TItem4>(record, ?splitOn = splitOn)
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
