namespace Belin.Sql
// fsharplint:disable MaxCharactersOnLine

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
    member this.Query(command: string, ?parameters: ParameterCollection): 'T list =
      this.Query<'T>(Command command, ?parameters = parameters)

    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.Query(command: Command, ?parameters: ParameterCollection): Map<string, objnull> list =
      this.Query<Map<string, objnull>>(command, ?parameters = parameters)

    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.Query(command: string, ?parameters: ParameterCollection): Map<string, objnull> list =
      this.Query<Map<string, objnull>>(Command command, ?parameters = parameters)

    /// Executes a parameterized SQL query and returns a sequence of object pairs whose properties correspond to the columns.
    member this.Query(command: Command, ?parameters: ParameterCollection, ?splitOn: string): ('Item1 * 'Item2) list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'Item1, 'Item2>(reader, ?splitOn = splitOn) |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of object pairs whose properties correspond to the columns.
    member this.Query(command: string, ?parameters: ParameterCollection, ?splitOn: string): ('Item1 * 'Item2) list =
      this.Query<'Item1, 'Item2>(Command command, ?parameters = parameters, ?splitOn = splitOn)

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string): ('Item1 * 'Item2 * 'Item3) list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'Item1, 'Item2, 'Item3>(reader, ?splitOn = splitOn) |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query(command: string, ?parameters: ParameterCollection, ?splitOn: string * string): ('Item1 * 'Item2 * 'Item3) list =
      this.Query<'Item1, 'Item2, 'Item3>(Command command, ?parameters = parameters, ?splitOn = splitOn)

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string * string): ('Item1 * 'Item2 * 'Item3 * 'Item4) list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'Item1, 'Item2, 'Item3, 'Item4>(reader, ?splitOn = splitOn) |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query(command: string, ?parameters: ParameterCollection, ?splitOn: string * string * string): ('Item1 * 'Item2 * 'Item3 * 'Item4) list =
      this.Query<'Item1, 'Item2, 'Item3, 'Item4>(Command command, ?parameters = parameters, ?splitOn = splitOn)

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
    member this.AsyncQuery(command: Command, ?parameters: ParameterCollection, ?splitOn: string, ?cancellationToken: CancellationToken): Async<('Item1 * 'Item2) list> = async {
      let! ambientToken = Async.CancellationToken
      let token = defaultArg cancellationToken ambientToken

      if this.State = ConnectionState.Closed then do! this.OpenAsync token |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync token |> Async.AwaitTask
      return Mapper.Instance.CreateInstances<'Item1, 'Item2>(reader, ?splitOn = splitOn) |> Seq.toList
    }

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.AsyncQuery(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string, ?cancellationToken: CancellationToken): Async<('Item1 * 'Item2 * 'Item3) list> = async {
      let! ambientToken = Async.CancellationToken
      let token = defaultArg cancellationToken ambientToken

      if this.State = ConnectionState.Closed then do! this.OpenAsync token |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync token |> Async.AwaitTask
      return Mapper.Instance.CreateInstances<'Item1, 'Item2, 'Item3>(reader, ?splitOn = splitOn) |> Seq.toList
    }

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.AsyncQuery(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string * string, ?cancellationToken: CancellationToken): Async<('Item1 * 'Item2 * 'Item3 * 'Item4) list> = async {
      let! ambientToken = Async.CancellationToken
      let token = defaultArg cancellationToken ambientToken

      if this.State = ConnectionState.Closed then do! this.OpenAsync token |> Async.AwaitTask
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync token |> Async.AwaitTask
      return Mapper.Instance.CreateInstances<'Item1, 'Item2, 'Item3, 'Item4>(reader, ?splitOn = splitOn) |> Seq.toList
    }
