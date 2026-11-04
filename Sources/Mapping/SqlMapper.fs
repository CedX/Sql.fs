namespace Belin.Sql

open System
open System.Collections
open System.Collections.Concurrent
open System.Data
open System.Dynamic
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
  member this.ChangeType(value: objnull, conversionType: Type, isNullable: bool): objnull =
    match value with
    | null | :? DBNull -> this.changeNullValue(conversionType, isNullable)
    | notNull -> this.changeNotNullValue(notNull, conversionType)

  /// Converts the specified object into an equivalent value of the specified type.
  member this.ChangeType(value: objnull, column: DbColumnInfo) =
    this.ChangeType(value, column.PropertyType, column.IsNullable)

  /// TODO conversionType => quand conversionType est une option
  member private _.changeNotNullValue (value: obj, conversionType: Type): obj =
    let nullableType = Nullable.GetUnderlyingType conversionType
    let targetType = nullableType |> defaultIfNull conversionType

    if targetType.IsEnum then
      match value with
      | :? string as stringValue -> Enum.Parse(targetType, stringValue, ignoreCase = true)
      | enumValue -> Enum.ToObject(targetType, Convert.ChangeType(enumValue, Enum.GetUnderlyingType targetType, CultureInfo.InvariantCulture) |> nonNull)
    elif targetType.IsInstanceOfType value then value
    else Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture) |> nonNull

  /// TODO conversionType => quand conversionType est une option
  member private _.changeNullValue (conversionType: Type, isNullable: bool): objnull =
    let nullableType = Nullable.GetUnderlyingType conversionType
    let targetType = nullableType |> defaultIfNull conversionType

    if not (isNull nullableType) then null
    elif targetType.IsValueType then RuntimeHelpers.GetUninitializedObject targetType
    elif targetType = typeof<string> then if isNullable then null else ""
    elif isNullable then null
    else Activator.CreateInstance targetType
