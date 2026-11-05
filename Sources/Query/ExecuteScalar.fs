namespace Belin.Sql

open System.Data
open System.Data.Common
open System.Threading

/// Provides extension members for database connections.
[<AutoOpen>]
module QueryExecuteScalarExtensions =

  type IDbConnection with
    /// Executes a parameterized SQL query that selects a single value.
    member this.ExecuteScalar(command: Command, ?parameters: ParameterCollection): 'T =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      let scalar = dbCommand.ExecuteScalar()
      Mapper.Instance.ChangeType(scalar, typeof<'T>, isNullable = true) :?> 'T

    /// Executes a parameterized SQL query that selects a single value.
    member this.ExecuteScalar(command: string, ?parameters: ParameterCollection): 'T =
      this.ExecuteScalar(Command command, ?parameters = parameters)

  type DbConnection with
    /// Executes a parameterized SQL query that selects a single value.
    member this.AsyncExecuteScalar(command: Command, ?parameters: ParameterCollection, ?cancellationToken: CancellationToken): Async<'T> = async {
      let! ambientToken = Async.CancellationToken
      let token = defaultArg cancellationToken ambientToken

      if this.State = ConnectionState.Closed then do! this.OpenAsync token |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      let! scalar = dbCommand.ExecuteScalarAsync token |> Async.AwaitTask
      return Mapper.Instance.ChangeType(scalar, typeof<'T>, isNullable = true) :?> 'T
    }

    /// Executes a parameterized SQL query that selects a single value.
    member this.AsyncExecuteScalar(command: string, ?parameters: ParameterCollection, ?cancellationToken: CancellationToken): Async<'T> =
      this.AsyncExecuteScalar(Command command, ?parameters = parameters, ?cancellationToken = cancellationToken)
