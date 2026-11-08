namespace Belin.Sql

open Belin.Should
open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Data.Common
open System.Threading.Tasks

/// Tests the features of the `ObjectUpdateExtensions` module.
[<TestClass>]
type ObjectUpdateExtensionsTests() =

  /// The connection to the data source.
  let mutable connection = Unchecked.defaultof<DbConnection>
  [<TestInitialize>] member _.Initialize() = connection <- openConnection()
  [<TestCleanup>] member _.Cleanup() = connection.Dispose()

  [<TestMethod>]
  member _.Update() =
    // It should update the specified record.
    let sql = "SELECT * FROM Characters WHERE firstName = 'Sauron'"

    let sauron = connection.QuerySingle<Character> sql |> Option.get
    sauron.FullName |> Should.be (Some "Sauron")
    sauron.Gender |> Should.be CharacterGender.DarkLord

    sauron.LastName <- "The big bad guy"
    sauron.Gender <- CharacterGender.Istari
    connection.Update sauron |> Should.be 1

    let sauron = connection.QuerySingle<Character> sql |> Option.get
    sauron.FullName |> Should.be (Some "Sauron The big bad guy")
    sauron.Gender |> Should.be CharacterGender.Istari

    // It should allow updating a specific set of columns.
    let sql = "SELECT * FROM Characters WHERE firstName = 'Saruman'"

    let saruman = connection.QuerySingle<Character> sql |> Option.get
    saruman.FullName |> Should.be (Some "Saruman")
    saruman.Gender |> Should.be CharacterGender.Istari

    saruman.LastName <- "The traitor"
    saruman.Gender <- CharacterGender.DarkLord
    connection.Update(saruman, [| "gender" |]) |> Should.be 1

    let saruman = connection.QuerySingle<Character> sql |> Option.get
    saruman.FullName |> Should.be (Some "Saruman")
    saruman.Gender |> Should.be CharacterGender.DarkLord

  [<TestMethod>]
  member _.AsyncUpdate(): Task = task {
    // It should update the specified record.
    let sql = "SELECT * FROM Characters WHERE firstName = 'Sauron'"

    let! sauron = connection.AsyncQuerySingle<Character> sql
    let sauron = Option.get sauron
    sauron.FullName |> Should.be (Some "Sauron")
    sauron.Gender |> Should.be CharacterGender.DarkLord

    sauron.LastName <- "The big bad guy"
    sauron.Gender <- CharacterGender.Istari
    do! connection.AsyncUpdate sauron |> AsyncShould.be 1

    let! sauron = connection.AsyncQuerySingle<Character> sql
    let sauron = Option.get sauron
    sauron.FullName |> Should.be (Some "Sauron The big bad guy")
    sauron.Gender |> Should.be CharacterGender.Istari

    // It should allow updating a specific set of columns.
    let sql = "SELECT * FROM Characters WHERE firstName = 'Saruman'"

    let! saruman = connection.AsyncQuerySingle<Character> sql
    let saruman = Option.get saruman
    saruman.FullName |> Should.be (Some "Saruman")
    saruman.Gender |> Should.be CharacterGender.Istari

    saruman.LastName <- "The traitor"
    saruman.Gender <- CharacterGender.DarkLord
    do! connection.AsyncUpdate(saruman, [| "gender" |]) |> AsyncShould.be 1

    let! saruman = connection.AsyncQuerySingle<Character> sql
    let saruman = Option.get saruman
    saruman.FullName |> Should.be (Some "Saruman")
    saruman.Gender |> Should.be CharacterGender.DarkLord
  }
