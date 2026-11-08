namespace Belin.Sql

open Belin.Should
open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `QueryFirstExtensions` module.
[<TestClass>]
type QueryFirstExtensionsTests() =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.QueryFirst() =
    // It should return the first record produced by the SQL query.
    let sql = "SELECT * FROM Characters WHERE fullName = @FullName"
    let parameters = ParameterCollection (Parameter("FullName", "Sauron"))
    let record = connection.QueryFirst<Character>(sql, parameters)

    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.FirstName |> Should.be "Sauron"
      character.Gender |> Should.be CharacterGender.DarkLord

    // It should return `None` if the query produces no results.
    let parameters = ParameterCollection (Parameter("FullName", "Cédric"))
    connection.QueryFirst<Character>(sql, parameters) |> Should.be None

  [<TestMethod>]
  member _.AsyncQueryFirst(): Task = task {
    // It should return the first record produced by the SQL query.
    let sql = "SELECT * FROM Characters WHERE fullName = @FullName"
    let parameters = ParameterCollection (Parameter("FullName", "Sauron"))
    let! record = connection.AsyncQueryFirst<Character>(sql, parameters)

    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.FirstName |> Should.be "Sauron"
      character.Gender |> Should.be CharacterGender.DarkLord

    // It should return `None` if the query produces no results.
    let parameters = ParameterCollection (Parameter("FullName", "Cédric"))
    do! connection.AsyncQueryFirst<Character>(sql, parameters) |> AsyncShould.be None
  }
