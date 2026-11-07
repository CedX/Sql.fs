namespace Belin.Sql

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module ObjectInsertExtensions =

  type IDbConnection with
    /// Inserts the specified entity.
    member this.Insert(entity: 'T, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): int64 =
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetInsertCommand<'T> entity
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction

      let id = this.ExecuteScalar<int64>(command, parameters)
      match Mapper.Instance.GetTable<'T>().IdentityColumn with
      | None -> ()
      | Some column -> column.SetValue(entity, Mapper.Instance.ChangeType(id, column))
      id

  type DbConnection with
    /// Inserts the specified entity.
    member this.AsyncInsert(entity: 'T, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): Async<int64> = async {
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetInsertCommand<'T> entity
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction

      let! id = this.AsyncExecuteScalar<int64>(command, parameters)
      match Mapper.Instance.GetTable<'T>().IdentityColumn with
      | None -> ()
      | Some column -> column.SetValue(entity, Mapper.Instance.ChangeType(id, column))
      return id
    }
