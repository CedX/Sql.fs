namespace Belin.Sql

open System
open System.Data.Common
open System.Data.SQLite
open System.IO

/// Manages the test data source.
[<AutoOpen>]
module internal Connection =

  /// Opens a connection to the data source.
  let openConnection (): DbConnection =
    let connection = new SQLiteConnection "DataSource=:memory:"
    connection.Open()

    use command = connection.CreateCommand()
    command.CommandText <- File.ReadAllText(Path.Join(AppContext.BaseDirectory, "../Resources/Schema.sql"))
    command.ExecuteNonQuery() |> ignore<int>
    connection
