namespace Belin.Sql

open System.Data
open System.Data.Common
open System.Threading

/// Provides extension members for database connections.
[<AutoOpen>]
module QueryExtensions =

  type IDbConnection with
    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.Query(command: Command, ?parameters: ParameterCollection): 'T list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'T> reader |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.Query(command: Command, ?parameters: ParameterCollection): Map<string, objnull> list =
      this.Query<Map<string, objnull>>(command, ?parameters = parameters)

    /// Executes a parameterized SQL query and returns a sequence of object pairs whose properties correspond to the columns.
    member this.Query(command: Command, ?parameters: ParameterCollection, ?splitOn: string): ('TItem1 * 'TItem2) list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'TItem1, 'TItem2>(reader, ?splitOn = splitOn) |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string): ('TItem1 * 'TItem2 * 'TItem3) list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'TItem1, 'TItem2, 'TItem3>(reader, ?splitOn = splitOn) |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string * string): ('TItem1 * 'TItem2 * 'TItem3 * 'TItem4) list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'TItem1, 'TItem2, 'TItem3, 'TItem4>(reader, ?splitOn = splitOn) |> Seq.toList

  type DbConnection with
    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.AsyncQuery(command: Command, ?parameters: ParameterCollection, ?cancellationToken: CancellationToken): Async<'T list> = async {
      let! ambientToken = Async.CancellationToken
      let token = defaultArg cancellationToken ambientToken

      if this.State = ConnectionState.Closed then do! this.OpenAsync token |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync token |> Async.AwaitTask
      return Mapper.Instance.CreateInstances<'T> reader |> Seq.toList
    }

    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.AsyncQuery(command: Command, ?parameters: ParameterCollection, ?cancellationToken: CancellationToken): Async<Map<string, objnull> list> =
      this.AsyncQuery<Map<string, objnull>>(command, ?parameters = parameters, ?cancellationToken = cancellationToken)

    /// Executes a parameterized SQL query and returns a sequence of object pairs whose properties correspond to the columns.
    member this.AsyncQuery(command: Command, ?parameters: ParameterCollection, ?splitOn: string, ?cancellationToken: CancellationToken): Async<('TItem1 * 'TItem2) list> = async {
      let! ambientToken = Async.CancellationToken
      let token = defaultArg cancellationToken ambientToken

      if this.State = ConnectionState.Closed then do! this.OpenAsync token |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync token |> Async.AwaitTask
      return Mapper.Instance.CreateInstances<'TItem1, 'TItem2>(reader, ?splitOn = splitOn) |> Seq.toList
    }

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.AsyncQuery(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string, ?cancellationToken: CancellationToken): Async<('TItem1 * 'TItem2 * 'TItem3) list> = async {
      let! ambientToken = Async.CancellationToken
      let token = defaultArg cancellationToken ambientToken

      if this.State = ConnectionState.Closed then do! this.OpenAsync token |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync token |> Async.AwaitTask
      return Mapper.Instance.CreateInstances<'TItem1, 'TItem2, 'TItem3>(reader, ?splitOn = splitOn) |> Seq.toList
    }

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.AsyncQuery(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string * string, ?cancellationToken: CancellationToken): Async<('TItem1 * 'TItem2 * 'TItem3 * 'TItem4) list> = async {
      let! ambientToken = Async.CancellationToken
      let token = defaultArg cancellationToken ambientToken

      if this.State = ConnectionState.Closed then do! this.OpenAsync token |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync token |> Async.AwaitTask
      return Mapper.Instance.CreateInstances<'TItem1, 'TItem2, 'TItem3, 'TItem4>(reader, ?splitOn = splitOn) |> Seq.toList
    }
