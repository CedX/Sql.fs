namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module QueryExecuteExtensions =

  type IDbConnection with
    /// Executes a parameterized SQL statement.
    member this.Execute(command: Command, ?parameters: ParameterCollection): int =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      dbCommand.ExecuteNonQuery()

    /// Executes a parameterized SQL statement.
    member this.Execute(command: string, ?parameters: ParameterCollection): int =
      this.Execute(Command command, ?parameters = parameters)

  type DbConnection with
    /// Executes a parameterized SQL statement.
    member this.AsyncExecute(command: Command, ?parameters: ParameterCollection): Async<int> = async {
      let! cancellationToken = Async.CancellationToken
      if this.State = ConnectionState.Closed then do! this.OpenAsync cancellationToken |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      return! dbCommand.ExecuteNonQueryAsync cancellationToken |> Async.AwaitTask
    }

    /// Executes a parameterized SQL statement.
    member this.AsyncExecute(command: string, ?parameters: ParameterCollection): Async<int> =
      this.AsyncExecute(Command command, ?parameters = parameters)
