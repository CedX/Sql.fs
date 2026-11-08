namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.SQLite

/// Tests the features of the `CommandBuilder` class.
[<TestClass>]
type CommandBuilderTests() =

  /// The test data.
  let character = { Character.Default with Id = 1_000; FirstName = "Cédric"; Gender = CharacterGender.DarkLord }

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<SQLiteConnection>
  [<TestInitialize>] member _.Initialize() = connection <- new SQLiteConnection "DataSource=:memory:"
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.GetCountAllCommand() =
    // It should return the SQL command to count all entities.
    let command, parameters = CommandBuilder.Create(connection).GetCountAllCommand<Character>()
    command.Text |> Should.be @"SELECT COUNT(*) FROM ""main"".""Characters"""

    // It should also return an empty parameter collection.
    Should.beEmpty parameters

  [<TestMethod>]
  member _.GetDeleteCommand() =
    // It should return the SQL command to delete an entity.
    let command, parameters = CommandBuilder.Create(connection).GetDeleteCommand character
    command.Text |> Should.startWith @"DELETE FROM ""main"".""Characters"""
    command.Text |> Should.endWith @"WHERE ""ID"" = @ID"

    // It should also return the parameters used by the SQL command.
    let parameter = Seq.exactlyOne parameters
    parameter.Name |> Should.be "@ID"
    parameter.Value |> Should.be 1_000

  [<TestMethod>]
  member _.GetDeleteAllCommand() =
    // It should return the SQL command to delete all entities.
    let command, parameters = CommandBuilder.Create(connection).GetDeleteAllCommand<Character>()
    command.Text |> Should.be @"DELETE FROM ""main"".""Characters"""

    // It should also return an empty parameter collection.
    Should.beEmpty parameters

  [<TestMethod>]
  member _.GetExistsCommand() =
    // It should return the SQL command to check the existence of an entity.
    let command, parameters = CommandBuilder.Create(connection).GetExistsCommand<Character> character.Id
    command.Text |> Should.startWith "SELECT 1"
    command.Text |> Should.containString @"FROM ""main"".""Characters"""
    command.Text |> Should.endWith @"WHERE ""ID"" = @ID"

    // It should also return the parameters used by the SQL command.
    let parameter = Seq.exactlyOne parameters
    parameter.Name |> Should.be "@ID"
    parameter.Value |> Should.be 1_000

  [<TestMethod>]
  member _.GetFindCommand() =
    let builder = CommandBuilder.Create connection

    // It should return the SQL command to find an entity.
    let command, parameters = builder.GetFindCommand<Character> character.Id
    command.Text |> Should.startWith @"SELECT """
    command.Text |> Should.notContainString "*"
    command.Text |> Should.containString @"FROM ""main"".""Characters"""
    command.Text |> Should.endWith @"WHERE ""ID"" = @ID"

    // It should also return the parameters used by the SQL command.
    let parameter = Seq.exactlyOne parameters
    parameter.Name |> Should.be "@ID"
    parameter.Value |> Should.be 1_000

    // It should allow selecting a specific set of columns.
    let command, _ = builder.GetFindCommand<Character>(character.Id, [| "firstName" |])
    command.Text |> Should.startWith @"SELECT ""firstName"""
    command.Text |> Should.notContainString "gender"
    command.Text |> Should.notContainString "lastName"
    command.Text |> Should.endWith @"WHERE ""ID"" = @ID"

  [<TestMethod>]
  member _.GetFindAllCommand() =
    let builder = CommandBuilder.Create connection

    // It should return the SQL command to find all entities.
    let command, parameters = builder.GetFindAllCommand<Character>()
    command.Text |> Should.startWith @"SELECT """
    command.Text |> Should.notContainString "*"
    command.Text |> Should.containString @"FROM ""main"".""Characters"""
    command.Text |> Should.endWith @"ORDER BY ""ID"" ASC"

    // It should also return an empty parameter collection.
    Should.beEmpty parameters

    // It should allow sorting the results by a specific set of columns.
    let command, _ = builder.GetFindAllCommand<Character>(OrderHintCollection ["gender", Ascending; "fullName", Descending])
    command.Text |> Should.startWith @"SELECT """
    command.Text |> Should.notContainString "*"
    command.Text |> Should.containString @"FROM ""main"".""Characters"""
    command.Text |> Should.endWith @"ORDER BY ""gender"" ASC, ""fullName"" DESC"

    // It should allow selecting a specific set of columns.
    let command, _ = builder.GetFindAllCommand<Character>(columns = [| "firstName"|])
    command.Text |> Should.startWith @"SELECT ""firstName"""
    command.Text |> Should.notContainString "gender"
    command.Text |> Should.notContainString "lastName"
    command.Text |> Should.endWith @"ORDER BY ""ID"" ASC"

  [<TestMethod>]
  member _.GetInsertCommand() =
    // It should return the SQL command to insert an entity.
    let command, parameters = CommandBuilder.Create(connection).GetInsertCommand character
    command.Text |> Should.startWith @"INSERT INTO ""main"".""Characters"" ("
    command.Text |> Should.containString "VALUES ("

    // It should also return the parameters used by the SQL command.
    parameters.Count |> Should.be 3
    parameters["firstName"].Value |> Should.be "Cédric"
    parameters["gender"].Value |> Should.be (nameof CharacterGender.DarkLord)
    parameters["lastName"].Value |> Should.be ""

  [<TestMethod>]
  member _.GetUpdateCommand() =
    let builder = CommandBuilder.Create connection

    // It should return the SQL command to update an entity.
    let command, parameters = builder.GetUpdateCommand character
    command.Text |> Should.startWith @"UPDATE ""main"".""Characters"""
    command.Text |> Should.containString @"SET """
    command.Text |> Should.endWith @"WHERE ""ID"" = @ID"

    // It should also return the parameters used by the SQL command.
    parameters.Count |> Should.be 4
    parameters["ID"].Value |> Should.be 1_000
    parameters["firstName"].Value |> Should.be "Cédric"
    parameters["gender"].Value |> Should.be (nameof CharacterGender.DarkLord)
    parameters["lastName"].Value |> Should.be ""

    // It should allow updating a specific set of columns.
    let _, parameters = builder.GetUpdateCommand(character, [| "firstName" |])
    parameters.Count |> Should.be 2
    parameters["ID"].Value |> Should.be 1_000
    parameters["firstName"].Value |> Should.be "Cédric"

  /// Given an unquoted identifier, returns the correct quoted form of that identifier.
  [<TestMethod>]
  member _.QuoteIdentifier() =
    let builder = CommandBuilder.Create connection
    builder.QuoteIdentifier "firstName" |> Should.be "\"firstName\""

  /// Given a quoted identifier, returns the correct unquoted form of that identifier.
  [<TestMethod>]
  member _.UnquoteIdentifier() =
    let builder = CommandBuilder.Create connection
    builder.UnquoteIdentifier "\"firstName\"" |> Should.be "firstName"
