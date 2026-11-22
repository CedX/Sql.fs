# Changelog

## Version [2027.1.0](https://github.com/CedX/Sql.fs/compare/v2026.12.1...v2027.1.0)
- Breaking change: dropped support for properties of type `'T option`.
- Breaking change: raised the required target framework.

## Version [2026.12.1](https://github.com/CedX/Sql.fs/compare/v2026.12.0...v2026.12.1)
- Fixed the handling of properties of type `'T option` in the `CommandBuilder.GetParameterValue` method.

## Version [2026.12.0](https://github.com/CedX/Sql.fs/compare/v2026.11.0...v2026.12.0)
- Improved the signature of generic functions.

## Version [2026.11.0](https://github.com/CedX/Sql.fs/compare/v2026.10.0...v2026.11.0)
- Added extension methods that cover basic querying needs.
- Added the `CommandBuilder`, `OrderHint` and `OrderHintCollection` classes.
- Renamed the `ParameterCollection.AddWithValue` method to `AddParameter`.

## Version 2026.10.0
- Initial release.
