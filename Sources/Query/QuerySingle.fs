namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module QuerySingleExtensions =

  type IDbConnection with
    /// Executes a parameterized SQL query and returns the single row.
    member this.QuerySingle(command: Command, ?parameters: ParameterCollection): 'T option =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()

      let mutable record: 'T option = None
      let mutable rowCount = 0
      while reader.Read() && rowCount <= 2 do
        if rowCount = 0 then record <- Some (Mapper.Instance.CreateInstance<'T> reader)
        rowCount <- rowCount + 1

      if rowCount = 1 then record else None

    /// Executes a parameterized SQL query and returns the single row.
    member this.QuerySingle(command: string, ?parameters: ParameterCollection): 'T option =
      this.QuerySingle<'T>(Command command, ?parameters = parameters)

  type DbConnection with
    /// Executes a parameterized SQL query and returns the single row.
    member this.AsyncQuerySingle(command: Command, ?parameters: ParameterCollection): Async<'T option> = async {
      let! cancellationToken = Async.CancellationToken
      if this.State = ConnectionState.Closed then do! this.OpenAsync cancellationToken |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync cancellationToken |> Async.AwaitTask

      let mutable loop = true
      let mutable record: 'T option = None
      let mutable rowCount = 0
      while loop && rowCount <= 2 do
        let! ok = reader.ReadAsync cancellationToken |> Async.AwaitTask
        loop <- ok
        if ok then
          if rowCount = 0 then record <- Some (Mapper.Instance.CreateInstance<'T> reader)
          rowCount <- rowCount + 1

      return if rowCount = 1 then record else None
    }

    /// Executes a parameterized SQL query and returns the single row.
    member this.AsyncQuerySingle(command: string, ?parameters: ParameterCollection): Async<'T option> =
      this.AsyncQuerySingle<'T>(Command command, ?parameters = parameters)
