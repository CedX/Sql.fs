namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module ObjectCountExtensions =

  type IDbConnection with
    /// Counts all entities.
    member this.CountAll<'T>(?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): int =
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetCountAllCommand<'T>()
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      this.ExecuteScalar<int>(command, parameters)

  type DbConnection with
    /// Counts all entities.
    member this.CountAllAsync<'T>(?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): Async<int> = async {
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetCountAllCommand<'T>()
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      return! this.AsyncExecuteScalar<int>(command, parameters)
    }
