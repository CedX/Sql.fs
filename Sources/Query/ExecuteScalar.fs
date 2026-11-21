namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module QueryExecuteScalarExtensions =

  type IDbConnection with
    /// Executes a parameterized SQL query that selects a single value.
    /// Returns the first column of the first row.
    member this.ExecuteScalar<'T>(command: Command, ?parameters: ParameterCollection): 'T =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      let scalar = dbCommand.ExecuteScalar()
      Mapper.Instance.ChangeType<'T>(scalar, isNullable = true)

    /// Executes a parameterized SQL query that selects a single value.
    /// Returns the first column of the first row.
    member this.ExecuteScalar<'T>(command: string, ?parameters: (string * objnull) seq): 'T =
      this.ExecuteScalar<'T>(Command command, ParameterCollection(defaultArg parameters Seq.empty))

  type DbConnection with
    /// Executes a parameterized SQL query that selects a single value.
    /// Returns the first column of the first row.
    member this.AsyncExecuteScalar<'T>(command: Command, ?parameters: ParameterCollection): Async<'T> = async {
      let! cancellationToken = Async.CancellationToken
      if this.State = ConnectionState.Closed then do! this.OpenAsync cancellationToken |> Async.Await
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      let! scalar = dbCommand.ExecuteScalarAsync cancellationToken |> Async.Await
      return Mapper.Instance.ChangeType<'T>(scalar, isNullable = true)
    }

    /// Executes a parameterized SQL query that selects a single value.
    /// Returns the first column of the first row.
    member this.AsyncExecuteScalar<'T>(command: string, ?parameters: (string * objnull) seq): Async<'T> =
      this.AsyncExecuteScalar<'T>(Command command, ParameterCollection(defaultArg parameters Seq.empty))
