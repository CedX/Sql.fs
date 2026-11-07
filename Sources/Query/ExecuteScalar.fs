namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module QueryExecuteScalarExtensions =

  type IDbConnection with
    /// Executes a parameterized SQL query that selects a single value.
    member this.ExecuteScalar(command: Command, ?parameters: ParameterCollection): 'T =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      let scalar = dbCommand.ExecuteScalar()
      Mapper.Instance.ChangeType<'T>(scalar, isNullable = true)

    /// Executes a parameterized SQL query that selects a single value.
    member this.ExecuteScalar(command: string, ?parameters: ParameterCollection): 'T =
      this.ExecuteScalar<'T>(Command command, ?parameters = parameters)

  type DbConnection with
    /// Executes a parameterized SQL query that selects a single value.
    member this.AsyncExecuteScalar(command: Command, ?parameters: ParameterCollection): Async<'T> = async {
      let! cancellationToken = Async.CancellationToken
      if this.State = ConnectionState.Closed then do! this.OpenAsync cancellationToken |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      let! scalar = dbCommand.ExecuteScalarAsync cancellationToken |> Async.AwaitTask
      return Mapper.Instance.ChangeType<'T>(scalar, isNullable = true)
    }

    /// Executes a parameterized SQL query that selects a single value.
    member this.AsyncExecuteScalar(command: string, ?parameters: ParameterCollection): Async<'T> =
      this.AsyncExecuteScalar<'T>(Command command, ?parameters = parameters)
