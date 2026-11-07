namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module ObjectExistsExtensions =

  type IDbConnection with
    /// Checks whether an entity with the specified primary key exists.
    member this.Exists<'T>(id: obj, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): bool =
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetExistsCommand<'T> id
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      this.ExecuteScalar<bool>(command, parameters)

  type DbConnection with
    /// Checks whether an entity with the specified primary key exists.
    member this.AsyncExists<'T>(id: obj, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): Async<bool> = async {
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetExistsCommand<'T> id
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      return! this.AsyncExecuteScalar<bool>(command, parameters)
    }
