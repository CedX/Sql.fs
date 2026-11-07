namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.SQLite

/// Tests the features of the `CommandBuilder` class.
[<TestClass>]
type CommandBuilderTests() =

  /// The test data.
  let character = Character(Id = 1_000, FirstName = "Cédric", Gender = CharacterGender.DarkLord)

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
    let command, parameters = CommandBuilder.Create(connection).GetDeleteCommand(character)
    command.Text |> Should.startWith @"DELETE FROM ""main"".""Characters"""
    command.Text |> Should.endWith @"WHERE ""ID"" = @ID"

    // It should also return the parameters used by the SQL command.
    let parameter = parameters |> Seq.exactlyOne
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

  // [<TestMethod>]
  // member _.GetFindCommand() =
  //   let builder = CommandBuilder.Create(connection)

  //   // It should return the SQL command to find an entity.
  //   let command, parameters = builder.GetFindCommand<Character>(character.Id)
  //   Assert.StartsWith(@"SELECT """, command.Text)
  //   Assert.DoesNotContain("*", command.Text)
  //   Assert.Contains(@"FROM ""main"".""Characters""", command.Text)
  //   Assert.EndsWith(@"WHERE ""ID"" = @ID", command.Text)

  //   // It should also return the parameters used by the SQL command.
  //   let parameter = parameters.Single()
  //   Assert.AreEqual("@ID", parameter.Name)
  //   Assert.AreEqual(1_000, parameter.Value)

  //   // It should allow selecting a specific set of columns.
  //   (command, _) = builder.GetFindCommand<Character>(character.Id, ["firstName"])
  //   Assert.StartsWith(@"SELECT ""firstName""", command.Text)
  //   Assert.DoesNotContain("gender", command.Text)
  //   Assert.DoesNotContain("lastName", command.Text)
  //   Assert.EndsWith(@"WHERE ""ID"" = @ID", command.Text)

  // [<TestMethod>]
  // member _.GetFindAllCommand() =
  //   let builder = CommandBuilder.Create(connection)

  //   // It should return the SQL command to find all entities.
  //   let command, parameters = builder.GetFindAllCommand<Character>()
  //   Assert.StartsWith(@"SELECT """, command.Text)
  //   Assert.DoesNotContain("*", command.Text)
  //   Assert.Contains(@"FROM ""main"".""Characters""", command.Text)
  //   Assert.EndsWith(@"ORDER BY ""ID"" ASC", command.Text)

  //   // It should also return an empty parameter collection.
  //   Should.beEmpty parameters

  //   // It should allow sorting the results by a specific set of columns.
  //   (command, _) = builder.GetFindAllCommand<Character>([("gender", SortOrder.Ascending), ("fullName", SortOrder.Descending)])
  //   Assert.StartsWith(@"SELECT """, command.Text)
  //   Assert.DoesNotContain("*", command.Text)
  //   Assert.Contains(@"FROM ""main"".""Characters""", command.Text)
  //   Assert.EndsWith(@"ORDER BY ""gender"" ASC, ""fullName"" DESC", command.Text)

  //   // It should allow selecting a specific set of columns.
  //   (command, _) = builder.GetFindAllCommand<Character>(columns: ["firstName"])
  //   Assert.StartsWith(@"SELECT ""firstName""", command.Text)
  //   Assert.DoesNotContain("gender", command.Text)
  //   Assert.DoesNotContain("lastName", command.Text)
  //   Assert.EndsWith(@"ORDER BY ""ID"" ASC", command.Text)

  // [<TestMethod>]
  // member _.GetInsertCommand() =
  //   // It should return the SQL command to insert an entity.
  //   let command, parameters = CommandBuilder.Create(connection).GetInsertCommand(character)
  //   Assert.StartsWith(@"INSERT INTO ""main"".""Characters"" (", command.Text)
  //   Assert.Contains("VALUES (", command.Text)

  //   // It should also return the parameters used by the SQL command.
  //   Assert.HasCount(3, parameters)
  //   Assert.AreEqual("Cédric", parameters["firstName"].Value)
  //   Assert.AreEqual(nameof(CharacterGender.DarkLord), parameters["gender"].Value)
  //   Assert.AreEqual("", parameters["lastName"].Value)

  // [<TestMethod>]
  // member _.GetUpdateCommand() =
  //   let builder = CommandBuilder.Create(connection)

  //   // It should return the SQL command to update an entity.
  //   let command, parameters = builder.GetUpdateCommand(character)
  //   Assert.StartsWith(@"UPDATE ""main"".""Characters""", command.Text)
  //   Assert.Contains(@"SET """, command.Text)
  //   Assert.EndsWith(@"WHERE ""ID"" = @ID", command.Text)

  //   // It should also return the parameters used by the SQL command.
  //   Assert.HasCount(4, parameters)
  //   Assert.AreEqual(1_000, parameters["ID"].Value)
  //   Assert.AreEqual("Cédric", parameters["firstName"].Value)
  //   Assert.AreEqual(nameof(CharacterGender.DarkLord), parameters["gender"].Value)
  //   Assert.AreEqual("", parameters["lastName"].Value)

  //   // It should allow updating a specific set of columns.
  //   (_, parameters) = builder.GetUpdateCommand(character, "firstName")
  //   Assert.HasCount(2, parameters)
  //   Assert.AreEqual(1_000, parameters["ID"].Value)
  //   Assert.AreEqual("Cédric", parameters["firstName"].Value)

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
