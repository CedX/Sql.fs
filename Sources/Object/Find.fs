namespace Belin.Sql
// fsharplint:disable MaxCharactersOnLine

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module ObjectFindExtensions =

  type IDbConnection with
    /// Finds an entity with the specified primary key.
    member this.Find<'T>(id: obj, ?columns: string array, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): 'T option =
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetFindCommand<'T>(id, defaultArg columns [||])
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      this.QuerySingle<'T>(command, parameters)

    /// Finds all entities of the specified type.
    // public IList<'T> FindAll<'T>(OrderHintCollection? orderHints = null, ?columns: string array, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): 'T list =
    //   let builder = defaultArg builder (CommandBuilder.Create this)
    //   let command, parameters = builder.GetFindAllCommand(type, orderHints ?? [], columns ?? [])
    //   command.Timeout <- defaultArg timeout 30
    //   command.Transaction <- transaction
    //   this.Query(type, command, parameters)

  type DbConnection with
    /// Finds an entity with the specified primary key.
    member this.AsyncFind<'T>(id: obj, ?columns: string array, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder): Async<'T option> = async {
      let builder = defaultArg builder (CommandBuilder.Create this)
      let command, parameters = builder.GetFindCommand<'T>(id, defaultArg columns [||])
      command.Timeout <- defaultArg timeout 30
      command.Transaction <- transaction
      return! this.AsyncQuerySingle<'T>(command, parameters)
    }

    // /// Finds all entities of the specified type.
    // public async Task<IList<'T>> AsyncFindAll<'T>(OrderHintCollection? orderHints = null, ?columns: string array, ?timeout: int, ?transaction: IDbTransaction, ?builder: CommandBuilder) = async {
    //   let builder = defaultArg builder (CommandBuilder.Create this)
    //   let command, parameters = builder.GetFindAllCommand<'T>(orderHints ?? [], columns ?? [])
    //   command.Timeout <- defaultArg timeout 30
    //   command.Transaction <- transaction
    //   return! this.AsyncQuery<'T>(command, ?parameters = parameters)
