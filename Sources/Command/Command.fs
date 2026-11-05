namespace Belin.Sql

open System.Data

/// Represents an SQL statement that is executed while connected to a data source.
type Command(text: string) =

  /// The text of the SQL statement.
  member val Text = text with get, set

  /// The wait time, in seconds, before terminating the attempt to execute the command and generating an error.
  member val Timeout = 30 with get, set

  /// The transaction within which the command executes.
  member val Transaction: IDbTransaction option = None with get, set

  /// Value indicating how the command is interpreted.
  member val Type = CommandType.Text with get, set

  /// Converts this command into an `IDbCommand` object.
  member this.ToDbCommand(connection: IDbConnection, ?parameters: ParameterCollection): IDbCommand =
    let command = connection.CreateCommand()
    command.CommandText <- this.Text
    command.CommandTimeout <- this.Timeout
    command.CommandType <- this.Type
    match this.Transaction with None -> () | Some transaction -> command.Transaction <- transaction
    for parameter in defaultArg parameters (ParameterCollection()) do command.Parameters.Add(parameter.ToDbParameter command) |> ignore<int>
    command
