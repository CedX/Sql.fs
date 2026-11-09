namespace Belin.Sql

open System.ComponentModel.DataAnnotations.Schema
open System.Data

/// Defines the gender of a character.
type CharacterGender =
  | Balrog = 1
  | DarkLord = 2
  | Dwarf = 3
  | Elf = 4
  | Hobbit = 5
  | Human = 6
  | Istari = 7

/// Represents a fictional character from a well-known saga.
[<Table("Characters", Schema = "main"); CLIMutable>]
type Character =
  {
    /// The first name.
    [<Column "firstName">]
    FirstName: string

    /// The full name.
    [<Column "fullName"; DatabaseGenerated(DatabaseGeneratedOption.Computed)>]
    FullName: string option

    /// The character's gender.
    [<Column("gender", TypeName = nameof(DbType.AnsiString))>]
    mutable Gender: CharacterGender

    /// The character's identifier.
    [<Column("ID"); DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    Id: int

    /// The last name.
    [<Column "lastName">]
    mutable LastName: string
  }

  /// Creates a new instance with default values.
  static member Default = {
    FirstName = ""
    FullName = None
    Gender = CharacterGender.Human
    Id = 0
    LastName = ""
  }
