namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module ObjectUpdateExtensions =

  type IDbConnection with
    /// Updates the specified entity.
    member this.Update(entity: 'T, ?columns: string array, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): int =
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetUpdateCommand<'T>(entity, defaultArg columns [||])
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      this.Execute(command, parameters)

  type DbConnection with
    /// Updates the specified entity.
    member this.AsyncUpdate(entity: 'T, ?columns: string array, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): Async<int> = async {
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetUpdateCommand<'T>(entity, defaultArg columns [||])
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      return! this.AsyncExecute(command, parameters)
    }
