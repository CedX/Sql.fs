namespace Belin.Sql
// fsharplint:disable MaxCharactersOnLine

open System.Data
open System.Data.Common

/// Provides extension members for database connections.
[<AutoOpen>]
module QueryExtensions =

  type IDbConnection with
    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.Query<'T>(command: Command, ?parameters: ParameterCollection): 'T list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'T> reader |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.Query<'T>(command: string, ?parameters: ParameterCollection): 'T list =
      this.Query<'T>(Command command, ?parameters = parameters)

    /// Executes a parameterized SQL query and returns a sequence of object pairs whose properties correspond to the columns.
    member this.Query<'Item1, 'Item2>(command: Command, ?parameters: ParameterCollection, ?splitOn: string): ('Item1 * 'Item2) list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'Item1, 'Item2>(reader, ?splitOn = splitOn) |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of object pairs whose properties correspond to the columns.
    member this.Query<'Item1, 'Item2>(command: string, ?parameters: ParameterCollection, ?splitOn: string): ('Item1 * 'Item2) list =
      this.Query<'Item1, 'Item2>(Command command, ?parameters = parameters, ?splitOn = splitOn)

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query<'Item1, 'Item2, 'Item3>(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string): ('Item1 * 'Item2 * 'Item3) list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'Item1, 'Item2, 'Item3>(reader, ?splitOn = splitOn) |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query<'Item1, 'Item2, 'Item3>(command: string, ?parameters: ParameterCollection, ?splitOn: string * string): ('Item1 * 'Item2 * 'Item3) list =
      this.Query<'Item1, 'Item2, 'Item3>(Command command, ?parameters = parameters, ?splitOn = splitOn)

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query<'Item1, 'Item2, 'Item3, 'Item4>(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string * string): ('Item1 * 'Item2 * 'Item3 * 'Item4) list =
      if this.State = ConnectionState.Closed then this.Open()
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters)
      use reader = dbCommand.ExecuteReader()
      Mapper.Instance.CreateInstances<'Item1, 'Item2, 'Item3, 'Item4>(reader, ?splitOn = splitOn) |> Seq.toList

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.Query<'Item1, 'Item2, 'Item3, 'Item4>(command: string, ?parameters: ParameterCollection, ?splitOn: string * string * string): ('Item1 * 'Item2 * 'Item3 * 'Item4) list =
      this.Query<'Item1, 'Item2, 'Item3, 'Item4>(Command command, ?parameters = parameters, ?splitOn = splitOn)

  type DbConnection with
    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.AsyncQuery<'T>(command: Command, ?parameters: ParameterCollection): Async<'T list> = async {
      let! cancellationToken = Async.CancellationToken
      if this.State = ConnectionState.Closed then do! this.OpenAsync cancellationToken |> Async.Await
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync cancellationToken |> Async.Await
      return Mapper.Instance.CreateInstances<'T> reader |> Seq.toList
    }

    /// Executes a parameterized SQL query and returns a sequence of objects whose properties correspond to the columns.
    member this.AsyncQuery<'T>(command: string, ?parameters: ParameterCollection): Async<'T list> =
      this.AsyncQuery<'T>(Command command, ?parameters = parameters)

    /// Executes a parameterized SQL query and returns a sequence of object pairs whose properties correspond to the columns.
    member this.AsyncQuery<'Item1, 'Item2>(command: Command, ?parameters: ParameterCollection, ?splitOn: string): Async<('Item1 * 'Item2) list> = async {
      let! cancellationToken = Async.CancellationToken
      if this.State = ConnectionState.Closed then do! this.OpenAsync cancellationToken |> Async.Await
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync cancellationToken |> Async.Await
      return Mapper.Instance.CreateInstances<'Item1, 'Item2>(reader, ?splitOn = splitOn) |> Seq.toList
    }

    /// Executes a parameterized SQL query and returns a sequence of object pairs whose properties correspond to the columns.
    member this.AsyncQuery<'Item1, 'Item2>(command: string, ?parameters: ParameterCollection, ?splitOn: string): Async<('Item1 * 'Item2) list> =
      this.AsyncQuery<'Item1, 'Item2>(Command command, ?parameters = parameters, ?splitOn = splitOn)

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.AsyncQuery<'Item1, 'Item2, 'Item3>(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string): Async<('Item1 * 'Item2 * 'Item3) list> = async {
      let! cancellationToken = Async.CancellationToken
      if this.State = ConnectionState.Closed then do! this.OpenAsync cancellationToken |> Async.Await
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync cancellationToken |> Async.Await
      return Mapper.Instance.CreateInstances<'Item1, 'Item2, 'Item3>(reader, ?splitOn = splitOn) |> Seq.toList
    }

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.AsyncQuery<'Item1, 'Item2, 'Item3>(command: string, ?parameters: ParameterCollection, ?splitOn: string * string): Async<('Item1 * 'Item2 * 'Item3) list> =
      this.AsyncQuery<'Item1, 'Item2, 'Item3>(Command command, ?parameters = parameters, ?splitOn = splitOn)

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.AsyncQuery<'Item1, 'Item2, 'Item3, 'Item4>(command: Command, ?parameters: ParameterCollection, ?splitOn: string * string * string): Async<('Item1 * 'Item2 * 'Item3 * 'Item4) list> = async {
      let! cancellationToken = Async.CancellationToken
      if this.State = ConnectionState.Closed then do! this.OpenAsync cancellationToken |> Async.Await
      use dbCommand = command.ToDbCommand(this, ?parameters = parameters) :?> DbCommand
      use! reader = dbCommand.ExecuteReaderAsync cancellationToken |> Async.Await
      return Mapper.Instance.CreateInstances<'Item1, 'Item2, 'Item3, 'Item4>(reader, ?splitOn = splitOn) |> Seq.toList
    }

    /// Executes a parameterized SQL query and returns a sequence of object tuples whose properties correspond to the columns.
    member this.AsyncQuery<'Item1, 'Item2, 'Item3, 'Item4>(command: string, ?parameters: ParameterCollection, ?splitOn: string * string * string): Async<('Item1 * 'Item2 * 'Item3 * 'Item4) list> =
      this.AsyncQuery<'Item1, 'Item2, 'Item3, 'Item4>(Command command, ?parameters = parameters, ?splitOn = splitOn)
