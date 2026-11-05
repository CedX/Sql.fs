namespace Belin.Sql

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Text.RegularExpressions

/// Contains assertions for working with unit tests.
[<RequireQualifiedAccess>]
module internal Should =

  /// Tests whether the specified values are equal.
  let inline be (expected: 'T) (actual: 'T) =
    Assert.AreEqual<'T>(expected, actual)

  /// Tests whether the specified object is an instance of the expected type.
  let inline beAssignableTo (expectedType: Type) (value: 'T) =
    Assert.IsInstanceOfType(value, expectedType)

  /// Tests whether the specified sequence is empty.
  let inline beEmpty (sequence: 'T seq) =
    Assert.IsEmpty sequence

  /// Tests whether the specified condition is false.
  let inline beFalse (condition: bool) =
    Assert.IsFalse condition

  /// Tests whether the specified object is `None`.
  let inline beNone (option: 'T option) =
    option |> Option.isNone |> Assert.IsTrue

  /// Tests whether the specified object is `null`.
  let inline beNull (value: 'T) =
    Assert.IsNull value

  /// Tests whether the specified object is exactly an instance of the expected type.
  let inline beOfType (expectedType: Type) (value: 'T) =
    Assert.IsExactInstanceOfType(value, expectedType)

  /// Tests whether the specified value is contained in the given sequence.
  let inline beOneOf (sequence: 'T seq) (value: 'T) =
    Assert.Contains<'T>(value, sequence)

  /// Tests whether two sequences contain equal elements in the same order.
  let inline beSequence (expected: 'T seq) (actual: 'T seq) =
    Assert.AreSequenceEqual(expected, actual)

  /// Tests whether the specified object is `Some expected` value.
  let inline beSome (expected: 'T) (option: 'T option) =
    match option with
    | None -> Assert.Fail "The specified option is None."
    | Some value -> Assert.AreEqual<'T>(expected, value)

  /// Tests whether the specified condition is true.
  let inline beTrue (condition: bool) =
    Assert.IsTrue condition

  /// Tests whether the specified string ends with the given suffix.
  let inline endWith (expectedSuffix: string) (value: string) =
    Assert.EndsWith(expectedSuffix, value)

  /// Tests whether the specified string matches the given regular expression.
  let inline matchRegex (pattern: Regex) (value: string) =
    Assert.MatchesRegex(pattern, value)

  /// Tests whether the specified values are unequal.
  let inline notBe (notExpected: 'T) (actual: 'T) =
    Assert.AreNotEqual<'T>(notExpected, actual)

  /// Tests whether the specified object is not `null`.
  let inline notBeNull (value: 'T) =
    Assert.IsNotNull value

  /// Tests whether the specified string does not match the given regular expression.
  let inline notMatchRegex (pattern: Regex) (value: string) =
    Assert.DoesNotMatchRegex(pattern, value)

  /// Tests whether the specified action does not throw an exception of the expected type.
  let notThrow (exceptionType: Type) (action: unit -> unit) =
    try
      action()
      Assert.IsTrue true
    with ex ->
      Assert.IsNotInstanceOfType(ex, exceptionType)

  /// Tests whether the specified string begins with the given prefix.
  let inline startWith (expectedPrefix: string) (value: string) =
    Assert.StartsWith(expectedPrefix, value)

  /// Tests whether the specified action throws an exception of the expected type.
  let throw (exceptionType: Type) (action: unit -> unit) =
    try
      action()
      Assert.Fail "The specified action did not throw any exception."
    with ex ->
      Assert.IsInstanceOfType(ex, exceptionType)

/// Contains asynchronous assertions for working with unit tests.
[<RequireQualifiedAccess>]
module internal ShouldAsync =

  /// Tests whether the specified values are equal.
  let be (expected: 'T) (actual: Async<'T>) = task {
    let! actual = actual |> Async.StartAsTask
    return actual |> Should.be expected
  }

  /// Tests whether the specified object is an instance of the expected type.
  let beAssignableTo (expectedType: Type) (value: Async<'T>) = task {
    let! value = value |> Async.StartAsTask
    return value |> Should.beAssignableTo expectedType
  }

  /// Tests whether the specified sequence is empty.
  let beEmpty (sequence: Async<'T seq>) = task {
    let! sequence = sequence |> Async.StartAsTask
    return sequence |> Should.beEmpty
  }

  /// Tests whether the specified condition is false.
  let beFalse (condition: Async<bool>) = task {
    let! condition = condition |> Async.StartAsTask
    return condition |> Should.beFalse
  }

  /// Tests whether the specified object is `None`.
  let beNone (option: Async<'T option>) = task {
    let! option = option |> Async.StartAsTask
    return option |> Should.beNone
  }

  /// Tests whether the specified object is `null`.
  let beNull (value: Async<'T>) = task {
    let! value = value |> Async.StartAsTask
    return value |> Should.beNull
  }

  /// Tests whether the specified object is exactly an instance of the expected type.
  let beOfType (expectedType: Type) (value: Async<'T>) = task {
    let! value = value |> Async.StartAsTask
    return value |> Should.beOfType expectedType
  }

  /// Tests whether the specified value is contained in the given sequence.
  let beOneOf (sequence: 'T seq) (value: Async<'T>) = task {
    let! value = value |> Async.StartAsTask
    return value |> Should.beOneOf sequence
  }

  /// Tests whether two sequences contain equal elements in the same order.
  let beSequence (expected: 'T seq) (actual: Async<'T seq>) = task {
    let! actual = actual |> Async.StartAsTask
    return actual |> Should.beSequence expected
  }

  /// Tests whether the specified object is `Some expected` value.
  let beSome (expected: 'T) (option: Async<'T option>) = task {
    let! option = option |> Async.StartAsTask
    return option |> Should.beSome expected
  }

  /// Tests whether the specified condition is true.
  let beTrue (condition: Async<bool>) = task {
    let! condition = condition |> Async.StartAsTask
    return condition |> Should.beTrue
  }

  /// Tests whether the specified string ends with the given suffix.
  let endWith (expectedSuffix: string) (value: Async<string>) = task {
    let! value = value |> Async.StartAsTask
    return value |> Should.endWith expectedSuffix
  }

  /// Tests whether the specified string matches the given regular expression.
  let matchRegex (pattern: Regex) (value: Async<string>) = task {
    let! value = value |> Async.StartAsTask
    return value |> Should.matchRegex pattern
  }

  /// Tests whether the specified values are unequal.
  let notBe (notExpected: 'T) (actual: Async<'T>) = task {
    let! actual = actual |> Async.StartAsTask
    return actual |> Should.notBe notExpected
  }

  /// Tests whether the specified object is not `null`.
  let notBeNull (value: Async<'T>) = task {
    let! value = value |> Async.StartAsTask
    return value |> Should.notBeNull
  }

  /// Tests whether the specified string does not match the given regular expression.
  let notMatchRegex (pattern: Regex) (value: Async<string>) = task {
    let! value = value |> Async.StartAsTask
    return value |> Should.notMatchRegex pattern
  }

  /// Tests whether the specified string begins with the given prefix.
  let startWith (expectedPrefix: string) (value: Async<string>) = task {
    let! value = value |> Async.StartAsTask
    return value |> Should.startWith expectedPrefix
  }
