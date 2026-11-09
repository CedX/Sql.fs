namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module QueryFirstExtensions =

  type IDbConnection with
    /// Executes a parameterized SQL query and returns the first row.
    member this.QueryFirst<'T>(command: Command, ?parameters: ParameterCollection): 'T option =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      if reader.Read() then Some (Mapper.Instance.CreateInstance<'T> reader) else None

    /// Executes a parameterized SQL query and returns the first row.
    member this.QueryFirst<'T>(command: string, ?parameters: ParameterCollection): 'T option =
      this.QueryFirst<'T>(Command command, ?parameters = parameters)

  type DbConnection with
    /// Executes a parameterized SQL query and returns the first row.
    member this.AsyncQueryFirst<'T>(command: Command, ?parameters: ParameterCollection): Async<'T option> = async {
      let! cancellationToken = Async.CancellationToken
      if this.State = ConnectionState.Closed then do! this.OpenAsync cancellationToken |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync cancellationToken |> Async.AwaitTask
      let! ok = reader.ReadAsync cancellationToken |> Async.AwaitTask
      return if ok then Some (Mapper.Instance.CreateInstance<'T> reader) else None
    }

    /// Executes a parameterized SQL query and returns the first row.
    member this.AsyncQueryFirst<'T>(command: string, ?parameters: ParameterCollection): Async<'T option> =
      this.AsyncQueryFirst<'T>(Command command, ?parameters = parameters)
