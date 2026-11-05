namespace Belin.Sql

open System
open System.Collections.Generic
open System.Data
open System.Diagnostics.CodeAnalysis

/// Represents a parameter of a parameterized SQL statement.
type Parameter(name: string, ?value: objnull) =
  let mutable paramName = Parameter.NormalizeName name
  let mutable paramValue = Parameter.NormalizeValue(defaultArg value null)

  /// The prefixes used for parameter placeholders.
  static let prefixes = [| '?'; '@'; ':'; '$' |]

  /// The database type of this parameter.
  member val DbType: DbType option = None with get, set

  /// Value indicating whether this parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
  member val Direction: ParameterDirection option = None with get, set

  /// The parameter name.
  member _.Name
    with get() = paramName
    and set(value: string) = paramName <- Parameter.NormalizeName value

  /// Indicates the precision of numeric parameters.
  member val Precision: byte option = None with get, set

  /// Indicates the scale of numeric parameters.
  member val Scale: byte option = None with get, set

  /// The maximum size of this parameter, in bytes.
  member val Size: int option = None with get, set

  /// The parameter value.
  [<NotNull>]
  member _.Value
    with get() = paramValue
    and set(value: objnull) = paramValue <- Parameter.NormalizeValue value

  /// Converts this parameter into an `IDbDataParameter` object.
  member this.ToDbParameter(command: IDbCommand): IDbDataParameter =
    let parameter = command.CreateParameter()
    parameter.ParameterName <- this.Name
    parameter.Value <- this.Value
    match this.DbType with None -> () | Some dbType -> parameter.DbType <- dbType
    match this.Direction with None -> () | Some direction -> parameter.Direction <- direction
    match this.Precision with None -> () | Some precision -> parameter.Precision <- precision
    match this.Scale with None -> () | Some scale -> parameter.Scale <- scale
    match this.Size with None -> () | Some size -> parameter.Size <- size
    parameter

  /// Normalizes the specified parameter name.
  static member internal NormalizeName(name: string): string =
    if name.Length = 0 then "?"
    elif prefixes |> Array.contains (name[0]) then name
    else $"@{name}"

  /// Normalizes the specified parameter value.
  static member internal NormalizeValue(value: objnull): obj =
    match value with
    | null -> DBNull.Value
    | nonNullValue -> nonNullValue

/// Collects all parameters relevant to a parameterized SQL statement.
type ParameterCollection([<ParamArray>] parameters: Parameter array) =
  inherit ResizeArray<Parameter>(parameters)

  /// Creates a new parameter collection from the specified sequence of parameters.
  new(parameters: Parameter seq) =
    ParameterCollection(Array.ofSeq parameters)

  /// Creates a new parameter collection from the specified sequence of named parameters.
  new(parameters: (string * objnull) seq) =
    ParameterCollection(parameters |> Seq.map(fun (key, value) -> Parameter(key, value)) |> Array.ofSeq)

  /// Creates a new parameter collection from the specified sequence of positional parameters.
  new(parameters: objnull seq) =
    ParameterCollection(parameters |> Seq.indexed |> Seq.map(fun (index, value) -> Parameter($"?{index + 1}", value)) |> Array.ofSeq)

  /// Gets the parameter with the specified name.
  member this.Item with get(name: string) =
    let normalizedName = Parameter.NormalizeName name
    match this.Find(fun parameter -> parameter.Name = normalizedName) with
    | null -> raise (KeyNotFoundException normalizedName)
    | parameter -> parameter

  /// Adds a new named parameter to the end of this collection.
  member this.AddWithValue(name: string, value: objnull): Parameter =
    let parameter = Parameter(name, value)
    this.Add parameter
    parameter

  /// Adds a new positional parameter to the end of this collection.
  member this.AddWithValue(value: objnull): Parameter =
    this.AddWithValue($"?{this.Count + 1}", value)

  /// Gets a value indicating whether a parameter in this collection has the specified name.
  member this.Contains(name: string): bool =
    let normalizedName = Parameter.NormalizeName name
    this.Exists(fun parameter -> parameter.Name = normalizedName)

  /// Returns the index of the parameter with the specified name.
  member this.IndexOf(name: string): int =
    let normalizedName = Parameter.NormalizeName name
    this.FindIndex(fun parameter -> parameter.Name = normalizedName)

  /// Removes the parameter with the specified name from this collection.
  member this.RemoveAt(name: string) =
    try this.IndexOf name |> this.RemoveAt
    with :? ArgumentOutOfRangeException as ex -> raise (KeyNotFoundException(Parameter.NormalizeName name, ex))
