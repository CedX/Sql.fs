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
[<Table("Characters", Schema = "main")>]
type Character() =

  /// The first name.
  [<Column("firstName")>]
  member val FirstName = "" with get, set

  /// The full name.
  [<Column("fullName"); DatabaseGenerated(DatabaseGeneratedOption.Computed)>]
  member val FullName: string option = None with get, set

  /// The character's gender.
  [<Column("gender", TypeName = nameof(DbType.AnsiString))>]
  member val Gender = CharacterGender.Human

  /// The character's identifier.
  [<Column("ID"); DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
  member val Id = 1 with get, set

  /// The last name.
  [<Column("lastName")>]
  member val LastName = "" with get, set
