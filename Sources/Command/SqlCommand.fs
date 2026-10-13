namespace Belin.Sql

open System.Data
open System.Data.Common

/// Represents an SQL statement that is executed while connected to a data source.
type SqlCommand(text: string) =

  /// The text of the SQL statement.
  member val Text = text with get, set

  /// The wait time, in seconds, before terminating the attempt to execute the command and generating an error.
  member val Timeout = 30 with get, set

  /// The transaction within which the command executes.
  member val Transaction: IDbTransaction option = None with get, set

  /// Value indicating how the command is interpreted.
  member val Type = CommandType.Text with get, set

  /// Creates a new command from the specified text.
  // TODO public static implicit operator SqlCommand(string text) => new(text);

  /// Converts this command into an `IDbCommand` object.
  // member this.ToDbCommand(connection: IDbConnection, ?parameters: SqlParameterCollection): IDbCommand =
  //   var command = connection.CreateCommand();
  //   command.CommandText = Text;
  //   command.CommandTimeout = Timeout;
  //   command.CommandType = Type;
  //   command.Transaction = Transaction;
  //   foreach (var parameter in parameters ?? []) command.Parameters.Add(parameter.ToDbParameter(command));
  //   return command;

/// Automatically generates single-table commands.
type SqlCommandBuilder() =

  /// The list of data types corresponding to a string.
  private static readonly DbType[] stringTypes = [DbType.AnsiString, DbType.AnsiStringFixedLength, DbType.String, DbType.StringFixedLength];

  /// The position of the catalog name in a qualified table name.
  member val CatalogLocation CatalogLocation with get, set = CatalogLocation.Start;

  /// The string used as the catalog separator.
  member val string CatalogSeparator with get, set = ".";

  /// The SQL function to use when the `RETURNING` clause is not supported.
  member val string LastInsertIdFunction with get, set = "SCOPE_IDENTITY()";

  /// The beginning string to use for naming parameters.
  member val string ParameterPrefix with get, set = "@";

  /// The beginning string to use when specifying database objects.
  member val string QuotePrefix with get, set = "[";

  /// The ending string to use when specifying database objects.
  member val string QuoteSuffix with get, set = "]";

  /// The string used as the schema separator.
  member val string SchemaSeparator with get, set = ".";

  /// Value indicating whether the ADO.NET provider supports the `RETURNING` clause.
  member val bool SupportsReturningClause with get, set

  /// Value indicating whether the ADO.NET provider supports the `TRUNCATE TABLE` statement.
  member val bool SupportsTruncateTable with get, set

  /// Value indicating whether the ADO.NET provider uses positional parameters.
  member val bool UsePositionalParameters with get, set

  /// Creates a new command builder and initializes its properties according to the specified database connection.
  /// <param name="connection">The connection to the data source.</param>
  /// <returns>The newly created command builder.</returns>
  /// <exception cref="NotSupportedException">The specified connection type is not supported.</exception>
  public static SqlCommandBuilder Create(IDbConnection connection) => connection.GetType().FullName switch {
    "FirebirdSql.Data.FirebirdClient.FbConnection" => new() {
      QuotePrefix = "\"",
      QuoteSuffix = "\"",
      SupportsReturningClause = true
    },
    "Microsoft.Data.SqlClient.SqlConnection" or "System.Data.SqlClient.SqlConnection" => new() {
      SupportsTruncateTable = true
    },
    "Microsoft.Data.Sqlite.SqliteConnection" or "System.Data.SQLite.SQLiteConnection" => new() {
      QuotePrefix = "\"",
      QuoteSuffix = "\"",
      SupportsReturningClause = true
    },
    "MySql.Data.MySqlClient.MySqlConnection" or "MySqlConnector.MySqlConnection" => new() {
      LastInsertIdFunction = "LAST_INSERT_ID()",
      QuotePrefix = "`",
      QuoteSuffix = "`",
      SupportsTruncateTable = true
    },
    "Npgsql.NpgsqlConnection" => new() {
      QuotePrefix = "\"",
      QuoteSuffix = "\"",
      SupportsReturningClause = true,
      SupportsTruncateTable = true
    },
    "Oracle.ManagedDataAccess.Client.OracleConnection" => new() {
      CatalogLocation = CatalogLocation.End,
      CatalogSeparator = "@",
      ParameterPrefix = ":",
      QuotePrefix = "\"",
      QuoteSuffix = "\"",
      SupportsReturningClause = true,
      SupportsTruncateTable = true
    },
    "System.Data.Odbc.OdbcConnection" or "System.Data.OleDb.OleDbConnection" => new() {
      SupportsTruncateTable = true,
      UsePositionalParameters = true
    },
    _ => throw new NotSupportedException("The specified connection type is not supported.")
  };

  /// Gets the generated command to count all entities.
  /// <typeparam name="T">The entity type.</typeparam>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetCountAllCommand<T>() where T: new() =>
    GetCountAllCommand(typeof<T>);

  /// Gets the generated command to count all entities.
  /// <param name="type">The entity type.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetCountAllCommand(Type type) {
    var tableName = GetTableName(SqlMapper.Instance.GetTable(type));
    return ($"SELECT COUNT(*) FROM {tableName}", []);
  }

  /// Gets the generated command to delete an entity.
  /// <typeparam name="T">The entity type.</typeparam>
  /// <param name="entity">The entity to delete.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetDeleteCommand<T>(T entity) where T: new() =>
    GetDeleteCommand(typeof<T>, entity!);

  /// Gets the generated command to delete an entity.
  /// <param name="type">The entity type.</param>
  /// <param name="entity">The entity to delete.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetDeleteCommand(Type type, object entity) {
    var table = SqlMapper.Instance.GetTable(type);
    var idColumn = table.IdentityColumn ?? throw new InvalidOperationException("The identity column could not be found.");

    var parameter = new SqlParameter(UsePositionalParameters ? "?1" : GetParameterName(idColumn), GetParameterValue(idColumn, entity));
    var text = $"""
      DELETE FROM {GetTableName(table)}
      WHERE {QuoteIdentifier(idColumn.Name)} = {(UsePositionalParameters ? "?" : parameter.Name)}
      """;

    return (text, [parameter]);
  }

  /// Gets the generated command to delete all entities.
  /// <typeparam name="T">The entity type.</typeparam>
  /// <param name="truncate">Value indicating whether to truncate the underlying table.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetDeleteAllCommand<T>(bool truncate = false) where T: new() =>
    GetDeleteAllCommand(typeof<T>, truncate);

  /// Gets the generated command to delete all entities.
  /// <param name="type">The entity type.</param>
  /// <param name="truncate">Value indicating whether to truncate the underlying table.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetDeleteAllCommand(Type type, bool truncate = false) {
    var tableName = GetTableName(SqlMapper.Instance.GetTable(type));
    return (truncate && SupportsTruncateTable ? $"TRUNCATE TABLE {tableName}" : $"DELETE FROM {tableName}", []);
  }

  /// Gets the generated command to check the existence of an entity.
  /// <typeparam name="T">The entity type.</typeparam>
  /// <param name="id">The value of the entity's primary key.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetExistsCommand<T>(object id) where T: new() =>
    GetExistsCommand(typeof<T>, id);

  /// Gets the generated command to check the existence of an entity.
  /// <param name="type">The entity type.</param>
  /// <param name="id">The value of the entity's primary key.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetExistsCommand(Type type, object id) {
    var table = SqlMapper.Instance.GetTable(type);
    var idColumn = table.IdentityColumn ?? throw new InvalidOperationException("The identity column could not be found.");

    var parameter = new SqlParameter(UsePositionalParameters ? "?1" : GetParameterName(idColumn), id);
    var text = $"""
      SELECT 1
      FROM {GetTableName(table)}
      WHERE {QuoteIdentifier(idColumn.Name)} = {(UsePositionalParameters ? "?" : parameter.Name)}
      """;

    return (text, [parameter]);
  }

  /// Gets the generated command to find an entity.
  /// <typeparam name="T">The entity type.</typeparam>
  /// <param name="id">The value of the entity's primary key.</param>
  /// <param name="columns">The list of columns to select. By default, all columns.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetFindCommand<T>(object id, params string[] columns) where T: new() =>
    GetFindCommand(typeof<T>, id, columns);

  /// Gets the generated command to find an entity.
  /// <param name="type">The entity type.</param>
  /// <param name="id">The value of the entity's primary key.</param>
  /// <param name="columns">The list of columns to select. By default, all columns.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetFindCommand(Type type, object id, params string[] columns) {
    var table = SqlMapper.Instance.GetTable(type);
    var idColumn = table.IdentityColumn ?? throw new InvalidOperationException("The identity column could not be found.");

    var fields = (columns.Length == 0 ? table.Columns.Values : table.Columns.Values.Where(column => columns.Contains(column.Name)))
      .Where(column => column.CanWrite)
      .Select(column => column.Name)
      .ToList();

    if (!fields.Contains(idColumn.Name)) fields.Add(idColumn.Name);

    var parameter = new SqlParameter(UsePositionalParameters ? "?1" : GetParameterName(idColumn), id);
    var text = $"""
      SELECT {string.Join(", ", fields.Select(QuoteIdentifier))}
      FROM {GetTableName(table)}
      WHERE {QuoteIdentifier(idColumn.Name)} = {(UsePositionalParameters ? "?" : parameter.Name)}
      """;

    return (text, [parameter]);
  }

  /// Gets the generated command to find all entities.
  /// <typeparam name="T">The entity type.</typeparam>
  /// <param name="columns">The list of columns to select. By default, all columns.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetFindAllCommand<T>(params string[] columns) where T: new() =>
    GetFindAllCommand<T>(new(), columns);

  /// Gets the generated command to find all entities.
  /// <typeparam name="T">The entity type.</typeparam>
  /// <param name="orderHints">The hints describing the sort order of columns.</param>
  /// <param name="columns">The list of columns to select. By default, all columns.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetFindAllCommand<T>(SqlOrderHintCollection orderHints, params string[] columns) where T: new() =>
    GetFindAllCommand(typeof<T>, orderHints, columns);

  /// Gets the generated command to find all entities.
  /// <param name="type">The entity type.</param>
  /// <param name="columns">The list of columns to select. By default, all columns.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetFindAllCommand(Type type, params string[] columns) =>
    GetFindAllCommand(type, new(), columns);

  /// Gets the generated command to find all entities.
  /// <param name="type">The entity type.</param>
  /// <param name="orderHints">The hints describing the sort order of columns.</param>
  /// <param name="columns">The list of columns to select. By default, all columns.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetFindAllCommand(Type type, SqlOrderHintCollection orderHints, params string[] columns) {
    var table = SqlMapper.Instance.GetTable(type);
    var idColumn = table.IdentityColumn ?? throw new InvalidOperationException("The identity column could not be found.");

    var fields = (columns.Length == 0 ? table.Columns.Values : table.Columns.Values.Where(column => columns.Contains(column.Name)))
      .Where(column => column.CanWrite)
      .Select(column => column.Name)
      .ToList();

    if (!fields.Contains(idColumn.Name)) fields.Add(idColumn.Name);

    var orderBy = orderHints.Count == 0
      ? $"{QuoteIdentifier(idColumn.Name)} ASC"
      : string.Join(", ", orderHints.Select(orderHint => $"{QuoteIdentifier(orderHint.Column)} {(orderHint.SortOrder == SortOrder.Descending ? "DESC" : "ASC")}"));

    return ($"SELECT {string.Join(", ", fields.Select(QuoteIdentifier))} FROM {GetTableName(table)} ORDER BY {orderBy}", []);
  }

  /// Gets the generated command to insert an entity.
  /// <typeparam name="T">The entity type.</typeparam>
  /// <param name="entity">The entity to insert.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetInsertCommand<T>(T entity) where T: new() =>
    GetInsertCommand(typeof<T>, entity!);

  /// Gets the generated command to insert an entity.
  /// <param name="type">The entity type.</param>
  /// <param name="entity">The entity to insert.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetInsertCommand(Type type, object entity) {
    var table = SqlMapper.Instance.GetTable(type);
    var idColumn = table.IdentityColumn ?? throw new InvalidOperationException("The identity column could not be found.");

    var fields = table.Columns.Values.Where(column => column.CanRead && !column.IsComputed).ToArray();
    var text = $"""
      INSERT INTO {GetTableName(table)} ({string.Join(", ", fields.Select(field => QuoteIdentifier(field.Name)))})
      VALUES ({string.Join(", ", fields.Select(field => UsePositionalParameters ? "?" : GetParameterName(field)))})
      {(SupportsReturningClause ? $"RETURNING {QuoteIdentifier(idColumn.Name)}" : $"; SELECT {LastInsertIdFunction};")}
      """;

    return (text, [.. fields.Select((field, index) => (UsePositionalParameters ? $"?{index + 1}" : GetParameterName(field), GetParameterValue(field, entity)))]);
  }

  /// Gets the generated command to update an entity.
  /// <typeparam name="T">The entity type.</typeparam>
  /// <param name="entity">The entity to update.</param>
  /// <param name="columns">The list of columns to update. By default, all columns.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetUpdateCommand<T>(T entity, params string[] columns) where T: new() =>
    GetUpdateCommand(typeof<T>, entity!, columns);

  /// Gets the generated command to update an entity.
  /// <param name="type">The entity type.</param>
  /// <param name="entity">The entity to update.</param>
  /// <param name="columns">The list of columns to update. By default, all columns.</param>
  /// <returns>A tuple providing the generated command and its parameters.</returns>
  /// <exception cref="InvalidOperationException">The identity column could not be found.</exception>
  public (SqlCommand Command, SqlParameterCollection Parameters) GetUpdateCommand(Type type, object entity, params string[] columns) {
    var table = SqlMapper.Instance.GetTable(type);
    var idColumn = table.IdentityColumn ?? throw new InvalidOperationException("The identity column could not be found.");

    var fields = (columns.Length == 0 ? table.Columns.Values : table.Columns.Values.Where(column => columns.Contains(column.Name)))
      .Where(column => column.CanRead && !column.IsComputed)
      .ToArray();

    var text = $"""
      UPDATE {GetTableName(table)}
      SET {string.Join(", ", fields.Select(field => $"{QuoteIdentifier(field.Name)} = {(UsePositionalParameters ? "?" : GetParameterName(field))}"))}
      WHERE {QuoteIdentifier(idColumn.Name)} = {(UsePositionalParameters ? "?" : GetParameterName(idColumn))}
      """;

    return (text, [
      .. fields.Select((field, index) => (UsePositionalParameters ? $"?{index + 1}" : GetParameterName(field), GetParameterValue(field, entity))),
      (UsePositionalParameters ? $"?{fields.Length + 1}" : GetParameterName(idColumn), GetParameterValue(idColumn, entity))
    ]);
  }

  /// Given an unquoted identifier, returns the correct quoted form of that identifier.
  /// <param name="unquotedIdentifier">The original unquoted identifier.</param>
  /// <returns>The quoted version of the identifier.</returns>
  public string QuoteIdentifier(string unquotedIdentifier) =>
    $"{QuotePrefix}{unquotedIdentifier.Replace(QuoteSuffix, QuoteSuffix + QuoteSuffix)}{QuoteSuffix}";

  /// Given a quoted identifier, returns the correct unquoted form of that identifier.
  /// <param name="quotedIdentifier">The original quoted identifier.</param>
  /// <returns>The unquoted version of the identifier.</returns>
  public string UnquoteIdentifier(string quotedIdentifier) {
    if (quotedIdentifier.StartsWith(QuotePrefix, StringComparison.Ordinal)) quotedIdentifier = quotedIdentifier[QuotePrefix.Length..];
    if (quotedIdentifier.EndsWith(QuoteSuffix, StringComparison.Ordinal)) quotedIdentifier = quotedIdentifier[..^QuoteSuffix.Length];
    return quotedIdentifier.Replace(QuoteSuffix + QuoteSuffix, QuoteSuffix);
  }

  /// Returns the parameter name corresponding to the specified column.
  /// <param name="column">The column providing the parameter name.</param>
  /// <returns>The parameter name corresponding to the specified column.</returns>
  private string GetParameterName(DbColumnInfo column) => $"{ParameterPrefix}{column.Name}";

  /// Returns the parameter value corresponding to the specified column.
  /// <param name="column">The column providing the parameter data type.</param>
  /// <param name="entity">The entity providing the parameter value.</param>
  /// <returns>The parameter value corresponding to the specified column.</returns>
  private object? GetParameterValue(DbColumnInfo column, object entity) {
    var value = column.GetValue(entity);
    return column.PropertyType.IsEnum && stringTypes.Contains(column.DbType) ? value?.ToString() : value;
  }

  /// Returns the fully qualified name corresponding to the specified table.
  /// <param name="table">The table.</param>
  /// <returns>The fully qualified name corresponding to the specified table.</returns>
  private string GetTableName(DbTableInfo table) =>
    string.IsNullOrEmpty(table.Schema) ? QuoteIdentifier(table.Name) : $"{QuoteIdentifier(table.Schema)}{SchemaSeparator}{QuoteIdentifier(table.Name)}";
}
