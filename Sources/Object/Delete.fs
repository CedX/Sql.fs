namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module ObjectDeleteExtensions =

  type IDbConnection with
    /// Deletes the specified entity.
    member this.Delete(entity, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): bool =
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetDeleteCommand entity
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      this.Execute(command, parameters) > 0

    /// Deletes all entities.
    member this.DeleteAll<'T>(?truncate: bool, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder) =
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetDeleteAllCommand<'T>(?truncate = truncate)
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      this.Execute(command, parameters) |> ignore<int>

  type DbConnection with
    /// Deletes the specified entity.
    member this.AsyncDelete(entity, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): Async<bool> = async {
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetDeleteCommand entity
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      let! rowsAffected = this.AsyncExecute(command, parameters)
      return rowsAffected > 0
    }

    /// Deletes all entities.
    member this.AsyncDeleteAll<'T>(?truncate: bool, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): Async<unit> = async {
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetDeleteAllCommand<'T>(?truncate = truncate)
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      return! this.AsyncExecute(command, parameters) |> Async.Ignore<int>
    }
