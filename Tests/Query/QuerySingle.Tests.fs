namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `QuerySingleExtensions` module.
[<TestClass>]
type QuerySingleExtensionsTests() =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.QuerySingle() =
    // It should return the single record produced by the SQL query.
    let sql = "SELECT * FROM Characters WHERE fullName = @FullName"
    let parameters = ParameterCollection (Parameter("FullName", "Saruman"))
    let record = connection.QuerySingle<Character>(sql, parameters)

    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.FirstName |> Should.be "Saruman"
      character.Gender |> Should.be CharacterGender.Istari

    // It should return `None` if the query produces no results.
    let parameters = ParameterCollection (Parameter("FullName", "Cédric"))
    connection.QuerySingle<Character>(sql, parameters) |> Should.be None

    // It should return `None` if the query produces more than one result.
    let sql = "SELECT * FROM Characters WHERE gender = @Gender"
    let parameters = ParameterCollection (Parameter("Gender", CharacterGender.Human))
    connection.QuerySingle<Character>(sql, parameters) |> Should.be None

  [<TestMethod>]
  member _.AsyncQuerySingle(): Task = task {
    // It should return the single record produced by the SQL query.
    let sql = "SELECT * FROM Characters WHERE fullName = @FullName"
    let parameters = ParameterCollection (Parameter("FullName", "Saruman"))
    let! record = connection.AsyncQuerySingle<Character>(sql, parameters)

    match record with
    | None -> Should.beFalse record.IsNone
    | Some character ->
      character.FirstName |> Should.be "Saruman"
      character.Gender |> Should.be CharacterGender.Istari

    // It should return `None` if the query produces no results.
    let parameters = ParameterCollection (Parameter("FullName", "Cédric"))
    do! connection.AsyncQuerySingle<Character>(sql, parameters) |> ShouldAsync.be None

    // It should return `None` if the query produces more than one result.
    let sql = "SELECT * FROM Characters WHERE gender = @Gender"
    let parameters = ParameterCollection (Parameter("Gender", CharacterGender.Human))
    do! connection.AsyncQuerySingle<Character>(sql, parameters) |> ShouldAsync.be None
  }
