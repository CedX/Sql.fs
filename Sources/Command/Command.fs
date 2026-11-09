namespace Belin.Sql

open System
open System.Data
open System.Data.Common
open System.Text.RegularExpressions

/// Represents an SQL statement that is executed while connected to a data source.
type Command(text: string) =

  /// The text of the SQL statement.
  member val Text = text with get, set

  /// The wait time, in seconds, before terminating the attempt to execute the command and generating an error.
  member val Timeout = 30 with get, set

  /// The transaction within which the command executes.
  member val Transaction: IDbTransaction option = None with get, set

  /// Value indicating how the command is interpreted.
  member val Type = CommandType.Text with get, set

  /// Converts this command into an `IDbCommand` object.
  member this.ToDbCommand(connection: IDbConnection, ?parameters: ParameterCollection): IDbCommand =
    let command = connection.CreateCommand()
    command.CommandText <- this.Text
    command.CommandTimeout <- this.Timeout
    command.CommandType <- this.Type
    match this.Transaction with None -> () | Some transaction -> command.Transaction <- transaction
    for parameter in defaultArg parameters (ParameterCollection()) do command.Parameters.Add(parameter.ToDbParameter command) |> ignore<int>
    command

/// Automatically generates single-table commands.
type CommandBuilder() =

  /// The list of data types corresponding to a string.
  static let stringTypes = [| DbType.AnsiString; DbType.AnsiStringFixedLength; DbType.String; DbType.StringFixedLength |]

  /// The regular expression used to normalize the queries.
  static let whiteSpacePattern = Regex @"\s+"

  /// The position of the catalog name in a qualified table name.
  member val CatalogLocation = CatalogLocation.Start with get, set

  /// The string used as the catalog separator.
  member val CatalogSeparator = "." with get, set

  /// The SQL function to use when the `RETURNING` clause is not supported.
  member val LastInsertIdFunction = "SCOPE_IDENTITY()" with get, set

  /// The beginning string to use for naming parameters.
  member val ParameterPrefix = "@" with get, set

  /// The beginning string to use when specifying database objects.
  member val QuotePrefix = "[" with get, set

  /// The ending string to use when specifying database objects.
  member val QuoteSuffix = "]" with get, set

  /// The string used as the schema separator.
  member val SchemaSeparator = "." with get, set

  /// Value indicating whether the ADO.NET provider supports the `RETURNING` clause.
  member val SupportsReturningClause = false with get, set

  /// Value indicating whether the ADO.NET provider supports the `TRUNCATE TABLE` statement.
  member val SupportsTruncateTable = false with get, set

  /// Value indicating whether the ADO.NET provider uses positional parameters.
  member val UsePositionalParameters = false with get, set

  /// Creates a new command builder and initializes its properties according to the specified database connection.
  static member Create(connection: IDbConnection): CommandBuilder =
    match connection.GetType().FullName with
    | "FirebirdSql.Data.FirebirdClient.FbConnection" -> CommandBuilder(
      QuotePrefix = "\"",
      QuoteSuffix = "\"",
      SupportsReturningClause = true)
    | "Microsoft.Data.SqlClient.SqlConnection" | "System.Data.SqlClient.SqlConnection" -> CommandBuilder(
      SupportsTruncateTable = true)
    | "Microsoft.Data.Sqlite.SqliteConnection" | "System.Data.SQLite.SQLiteConnection" -> CommandBuilder(
      QuotePrefix = "\"",
      QuoteSuffix = "\"",
      SupportsReturningClause = true)
    | "MySql.Data.MySqlClient.MySqlConnection" | "MySqlConnector.MySqlConnection" -> CommandBuilder(
      LastInsertIdFunction = "LAST_INSERT_ID()",
      QuotePrefix = "`",
      QuoteSuffix = "`",
      SupportsTruncateTable = true)
    | "Npgsql.NpgsqlConnection" -> CommandBuilder(
      QuotePrefix = "\"",
      QuoteSuffix = "\"",
      SupportsReturningClause = true,
      SupportsTruncateTable = true)
    | "Oracle.ManagedDataAccess.Client.OracleConnection" -> CommandBuilder(
      CatalogLocation = CatalogLocation.End,
      CatalogSeparator = "@",
      ParameterPrefix = ":",
      QuotePrefix = "\"",
      QuoteSuffix = "\"",
      SupportsReturningClause = true,
      SupportsTruncateTable = true)
    | "System.Data.Odbc.OdbcConnection" | "System.Data.OleDb.OleDbConnection" -> CommandBuilder(
      SupportsTruncateTable = true,
      UsePositionalParameters = true)
    | _ -> raise (NotSupportedException "The specified connection type is not supported.")

  /// Gets the generated command to count all entities.
  member this.GetCountAllCommand<'T>(): Command * ParameterCollection =
    let tableName = Mapper.Instance.GetTable<'T>() |> this.GetTableName
    Command $"SELECT COUNT(*) FROM {tableName}", ParameterCollection()

  /// Gets the generated command to delete an entity.
  member this.GetDeleteCommand<'T>(entity: 'T): Command * ParameterCollection =
    let table = Mapper.Instance.GetTable<'T>()
    let idColumn = match table.IdentityColumn with Some column -> column | None -> invalidOp "The identity column could not be found."

    let parameter = Parameter(
      (if this.UsePositionalParameters then "?1" else this.GetParameterName idColumn),
      this.GetParameterValue(idColumn, entity))

    let text = CommandBuilder.NormalizeCommand $"""
      DELETE FROM {this.GetTableName table}
      WHERE {this.QuoteIdentifier idColumn.Name} = {if this.UsePositionalParameters then "?" else parameter.Name}
      """

    Command text, ParameterCollection parameter

  /// Gets the generated command to delete all entities.
  member this.GetDeleteAllCommand<'T>(?truncate: bool): Command * ParameterCollection =
    let tableName = this.GetTableName(Mapper.Instance.GetTable<'T>())
    let text =
      if defaultArg truncate false && this.SupportsTruncateTable then $"TRUNCATE TABLE {tableName}"
      else $"DELETE FROM {tableName}"

    Command text, ParameterCollection()

  /// Gets the generated command to check the existence of an entity.
  member this.GetExistsCommand<'T>(id: obj): Command * ParameterCollection =
    let table = Mapper.Instance.GetTable<'T>()
    let idColumn = match table.IdentityColumn with Some column -> column | None -> invalidOp "The identity column could not be found."

    let parameter = Parameter((if this.UsePositionalParameters then "?1" else this.GetParameterName idColumn), id)
    let text = CommandBuilder.NormalizeCommand $"""
      SELECT 1
      FROM {this.GetTableName table}
      WHERE {this.QuoteIdentifier idColumn.Name} = {if this.UsePositionalParameters then "?" else parameter.Name}
      """

    Command text, ParameterCollection parameter

  /// Gets the generated command to find an entity.
  member this.GetFindCommand<'T>(id: obj, ?columns: string array): Command * ParameterCollection =
    let table = Mapper.Instance.GetTable<'T>()
    let idColumn = match table.IdentityColumn with Some column -> column | None -> invalidOp "The identity column could not be found."

    let columns =
      match columns with
      | None -> table.Columns.Values
      | Some list when list.Length = 0 -> table.Columns.Values
      | Some list -> table.Columns.Values |> Seq.filter (fun column -> list |> Array.contains column.Name)

    let fields = ResizeArray(columns |> Seq.filter (fun column -> column.CanWrite) |> Seq.map (fun column -> column.Name))
    if not (fields.Contains idColumn.Name) then fields.Add idColumn.Name

    let parameter = Parameter((if this.UsePositionalParameters then "?1" else this.GetParameterName idColumn), id)
    let text = CommandBuilder.NormalizeCommand $"""
      SELECT {fields |> Seq.map this.QuoteIdentifier |> String.concat ", "}
      FROM {this.GetTableName table}
      WHERE {this.QuoteIdentifier idColumn.Name} = {if this.UsePositionalParameters then "?" else parameter.Name}
      """

    Command text, ParameterCollection parameter

  /// Gets the generated command to find all entities.
  member this.GetFindAllCommand<'T>(?orderHints: OrderHintCollection, ?columns: string array): Command * ParameterCollection =
    let table = Mapper.Instance.GetTable<'T>()
    let idColumn = match table.IdentityColumn with Some column -> column | None -> invalidOp "The identity column could not be found."

    let columns =
      match columns with
      | None -> table.Columns.Values
      | Some list when list.Length = 0 -> table.Columns.Values
      | Some list -> table.Columns.Values |> Seq.filter (fun column -> list |> Array.contains column.Name)

    let fields = ResizeArray(columns |> Seq.filter (fun column -> column.CanWrite) |> Seq.map (fun column -> column.Name))
    if not (fields.Contains idColumn.Name) then fields.Add idColumn.Name

    let orderHints = defaultArg orderHints (OrderHintCollection())
    let orderBy =
      if orderHints.Count = 0 then $"{this.QuoteIdentifier idColumn.Name} ASC"
      else
        orderHints
        |> Seq.map (fun orderHint -> $"""{this.QuoteIdentifier orderHint.Column} {if orderHint.SortOrder = Descending then "DESC" else "ASC"}""")
        |> String.concat ", "

    let text = CommandBuilder.NormalizeCommand $"""
      SELECT {fields |> Seq.map this.QuoteIdentifier |> String.concat ", "}
      FROM {this.GetTableName table}
      ORDER BY {orderBy}
      """

    Command text, ParameterCollection()

  /// Gets the generated command to insert an entity.
  member this.GetInsertCommand<'T>(entity: 'T): Command * ParameterCollection =
    let table = Mapper.Instance.GetTable<'T>()
    let idColumn = match table.IdentityColumn with Some column -> column | None -> invalidOp "The identity column could not be found."

    let fields = table.Columns.Values |> Seq.filter (fun column -> column.CanRead && not column.IsComputed) |> Seq.toArray
    let text = CommandBuilder.NormalizeCommand $"""
      INSERT INTO {this.GetTableName table} ({fields |> Seq.map (fun field -> this.QuoteIdentifier field.Name) |> String.concat ", "})
      VALUES ({fields |> Seq.map (fun field -> if this.UsePositionalParameters then "?" else this.GetParameterName field) |> String.concat ", "})
      {if this.SupportsReturningClause then $"RETURNING {this.QuoteIdentifier idColumn.Name}" else $"; SELECT {this.LastInsertIdFunction};"}
      """

    let parameters =
      fields
      |> Seq.indexed
      |> Seq.map (fun (index, field) -> Parameter(
        (if this.UsePositionalParameters then $"?{index + 1}" else this.GetParameterName field),
        this.GetParameterValue(field, entity)))

    Command text, ParameterCollection parameters

  /// Gets the generated command to update an entity.
  member this.GetUpdateCommand<'T>(entity: 'T, ?columns: string array): Command * ParameterCollection =
    let table = Mapper.Instance.GetTable<'T>()
    let idColumn = match table.IdentityColumn with Some column -> column | None -> invalidOp "The identity column could not be found."

    let columns =
      match columns with
      | None -> table.Columns.Values
      | Some list when list.Length = 0 -> table.Columns.Values
      | Some list -> table.Columns.Values |> Seq.filter (fun column -> list |> Array.contains column.Name)

    let formatAssignment (field: ColumnInfo) =
      $"""{this.QuoteIdentifier field.Name} = {if this.UsePositionalParameters then "?" else this.GetParameterName field}"""

    let fields = columns |> Seq.filter (fun column -> column.CanRead && not column.IsComputed) |> Seq.toArray
    let text = CommandBuilder.NormalizeCommand $"""
      UPDATE {this.GetTableName table}
      SET {fields |> Seq.map formatAssignment |> String.concat ", "}
      WHERE {this.QuoteIdentifier idColumn.Name} = {if this.UsePositionalParameters then "?" else this.GetParameterName idColumn}
      """

    let parameters =
      fields
      |> Seq.indexed
      |> Seq.map (fun (index, field) -> Parameter(
        (if this.UsePositionalParameters then $"?{index + 1}" else this.GetParameterName field),
        this.GetParameterValue(field, entity)))

    let idParameter = Parameter(if this.UsePositionalParameters then $"?{fields.Length + 1}" else this.GetParameterName idColumn)
    idParameter.Value <- this.GetParameterValue(idColumn, entity)
    Command text, ParameterCollection(Seq.append parameters (Seq.singleton idParameter))

  /// Given an unquoted identifier, returns the correct quoted form of that identifier.
  member this.QuoteIdentifier(unquotedIdentifier: string): string =
    $"{this.QuotePrefix}{unquotedIdentifier.Replace(this.QuoteSuffix, this.QuoteSuffix + this.QuoteSuffix)}{this.QuoteSuffix}"

  /// Given a quoted identifier, returns the correct unquoted form of that identifier.
  member this.UnquoteIdentifier(quotedIdentifier: string): string =
    let mutable identifier = quotedIdentifier
    if identifier.StartsWith(this.QuotePrefix, StringComparison.Ordinal) then identifier <- identifier[this.QuotePrefix.Length..]
    if identifier.EndsWith(this.QuoteSuffix, StringComparison.Ordinal) then identifier <- identifier.Substring(0, identifier.Length - this.QuoteSuffix.Length)
    identifier.Replace(this.QuoteSuffix + this.QuoteSuffix, this.QuoteSuffix)

  /// Normalizes the specified command text.
  static member internal NormalizeCommand(text: string): string =
    whiteSpacePattern.Replace(text, " ").Trim()

  /// Returns the parameter name corresponding to the specified column.
  member private this.GetParameterName(column: ColumnInfo): string =
    $"{this.ParameterPrefix}{column.Name}"

  /// Returns the parameter value corresponding to the specified column.
  member private _.GetParameterValue<'T>(column: ColumnInfo, entity: 'T): objnull =
    let value = column.GetValue entity
    if column.PropertyType.IsEnum && stringTypes |> Array.contains column.DbType then string value else value

  /// Returns the fully qualified name corresponding to the specified table.
  member private this.GetTableName(table: TableInfo): string =
    match table.Schema with
    | None -> this.QuoteIdentifier table.Name
    | Some schema -> $"{this.QuoteIdentifier schema}{this.SchemaSeparator}{this.QuoteIdentifier table.Name}"
