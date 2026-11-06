namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `DbConnectionExtensions` module.
[<TestClass>]
type DbConnectionExtensionsTests(testContext: TestContext) =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.Query() =
    // It should return the records produced by the SQL query.
    let sql = "SELECT * FROM Characters WHERE gender = @Gender ORDER BY fullName"
    let parameters = ParameterCollection [Parameter("Gender", nameof CharacterGender.Elf)]
    let records = connection.Query<Character>(sql, parameters)
    records.Length |> Should.be 3

    let elrond = records[0]
    elrond.FullName |> Should.beSome "Elrond"
    elrond.Gender |> Should.be CharacterGender.Elf

    let galadriel = records[1]
    galadriel.FullName |> Should.beSome "Galadriel"
    galadriel.Gender |> Should.be CharacterGender.Elf

    // It should allow the data rows to be split into distinct objects.
    let sql = "SELECT ID, firstName, lastName, ID, fullName, gender FROM Characters WHERE firstName = @FirstName"
    let parameters = ParameterCollection [Parameter("FirstName", "Frodo")]
    let objects = connection.Query<Map<string, objnull>, Map<string, objnull>>(sql, parameters)
    objects.Length |> Should.be 1

    let left, right = objects[0]
    left["ID"] |> Should.be 6L
    left["firstName"] |> Should.be "Frodo"
    left["lastName"] |> Should.be "Baggins"
    left.ContainsKey "fullName" |> Should.beFalse
    right["ID"] |> Should.be 6L
    right["fullName"] |> Should.be "Frodo Baggins"
    right["gender"] |> Should.be "Hobbit"
    right.ContainsKey "firstName" |> Should.beFalse

  [<TestMethod>]
  member _.AsyncQuery(): Task = task {
    // It should return the records produced by the SQL query.
    let sql = "SELECT * FROM Characters WHERE gender = @Gender ORDER BY fullName"
    let parameters = ParameterCollection [Parameter("Gender", nameof CharacterGender.Elf)]
    let! records = connection.AsyncQuery<Character>(sql, parameters, testContext.CancellationToken)
    records.Length |> Should.be 3

    let elrond = records[0]
    elrond.FullName |> Should.beSome "Elrond"
    elrond.Gender |> Should.be CharacterGender.Elf

    let galadriel = records[1]
    galadriel.FullName |> Should.beSome "Galadriel"
    galadriel.Gender |> Should.be CharacterGender.Elf

    // It should allow the data rows to be split into distinct objects.
    let sql = "SELECT ID, firstName, lastName, ID, fullName, gender FROM Characters WHERE firstName = @FirstName"
    let parameters = ParameterCollection [Parameter("FirstName", "Frodo")]
    let! objects = connection.AsyncQuery<Map<string, objnull>, Map<string, objnull>>(sql, parameters, cancellationToken = testContext.CancellationToken)
    objects.Length |> Should.be 1

    let left, right = objects[0]
    left["ID"] |> Should.be 6L
    left["firstName"] |> Should.be "Frodo"
    left["lastName"] |> Should.be "Baggins"
    left.ContainsKey "fullName" |> Should.beFalse
    right["ID"] |> Should.be 6L
    right["fullName"] |> Should.be "Frodo Baggins"
    right["gender"] |> Should.be "Hobbit"
    right.ContainsKey "firstName" |> Should.beFalse
  }
