# HackathonManager REST API

## Folder Structure

```
/HackathonManager
    /Exceptions - custom exception classes
    /Extensions - general extension methods that are used across the application
    /Features - contains the API endpoints and their supporting classes, grouped by feature
    /Interfaces - interfaces that are used across the application
    /Persistence - code for interacting with the database
        /Configuration - contains EF entity type configuration classes
        /Entities - contains the EF model classes that mirror database tables
        /Enums - contains enums that map to tables in the `enums` schema of the database
    /Services - service classes providing logic that is used across multiple features
    /Settings - classes that are used to access groups of configuration values from `IConfiguration`
    /Utilities - helper code that doesn't fit anywhere else
        /JsonPatch - helpers for working with JsonPatch requests
        /Results - Implementations of `Microsoft.AspNetCore.Http.IResult` for status codes that aren't defined by default
```

## API Patterns

All requests must include input validation using FluentValidation.
- Validation errors MUST use custom error codes defined `ErrorCodes.cs`.
- Wrappers for built-in FluentValidation rules, as well as custom rules, are defined in `Extensions/RuleBuilderExtensions.cs`.
- Always use the `...WithCode()` extension methods instead of built-in rules (add new wrappers as needed).

The API uses TypeId's for resource id values.  A TypeId consists of a type identifier prefix and base32 suffix that encodes a uuid.  Example: `user_2x4y6z8a0b1c2d3e4f5g6h7j8k`.
- Type identifiers are defined in `ResourceTypes.cs`, along with value converters for the id types.
- API request and response models use the `FastIDs.TypeId.TypeId` type for id properties.
- Database entities use the `FastIDs.TypeId.TypeIdDecoded` type for id properties.  The underlying database column is always a uuid, so the property's EF type configuration must use a value converter from `ResourceTypes`.

ETags are used for concurrency control and request optimization.
- Endpoints that work with a database resource with a `Version` should return an ETag header.
- The ETag is the entity's `Version` property encoded as a sqid (using `SqidsEncoder<uint>`).
- When an endpoint returns a collection, generate the uint value for its ETag by using `HackathonManager.Extensions.EnumerableExtensions.GenerateETag()`, and encode the value as a sqid.
- When a resource is versioned, GET endpoints must include an optional `If-None-Match` header.  If the etag in the header matches the etag of the resource (or collection), return a 304 not modified result.  See `Features/Users/GetUserById/GetUserByIdEndpoint.cs` as an example.
- When a resource is versioned, endpoints that modify the resource must include an `If-Match` header.  If the etag in the header does not match the etag of the resource, return a 412 precondition failed result.


Collection endpoints should implement pagination using cursors.
- Always use enums for search and sort options.
- The cursor should be a `record` containing all of the search and sort parameters needed to retrieve the next page of results.  See `Features/Users/GetUsers/GetUsersCursor.cs` for an example.
- Set explicit, single letter json property names on the cursor's properties.
- Encode the cursor for the response using `HackathonManager.Utilities.PaginationCursor.Encode(object cursor)`.
- Use `HackathonManager.Utilities.PaginatedEndpoint<TRequest, TResponse>` as the base class for the endpoint that uses cursors.
- The request object for the paginated endpoint should implement `HackathonManager.Interfaces.ICursorRequest`.
- The validator for the request should use `HackathonManager.Interfaces.CursorRequestValidatorExtensions.AddCursorRules<T>(this AbstractValidator<T> validator)` to apply validation to the cursor properties.
- Use the `TryDecodeCursor<TCursor>(string? value, [NotNullWhen(true)] out TCursor? cursor)` method from `PaginatedEndpoint` to decode cursors, and its `ThrowInvalidCursor()` method to indicate a cursor error.
- When provided, the cursor should override any search/sort parameters in the request.
- See `Features/Users/GetUsers/GetUsersEndpoint.cs` for an example cursor implementation.

## Coding standards

- Collection type guidelines:
  - Only use `IEnumerable<T>` parameters in methods that perform a simple single iteration of the data.
  - Only use `IEnumerable<T>` properties on response DTOs where the data is not read by our code.
  - Otherwise, prefer `IReadOnlyCollection<T>` to avoid multiple enumeration.
  - Use `HackathonManager.Extensions.EnumerableExtensions.ToReadOnlyCollection(this IEnumerable<T> enumerable)` to convert collections to `IReadOnlyCollection<T>`.
  - When working with temporary collections, such as the results of database queries, prefer using arrays instead of lists.
- Always implement `Endpoint` classes using the typed result pattern and overriding the `ExecuteAsync()` method.
- Use `ProblemDetails` to return error information when available.
- For simple responses that only require a status code (e.g. 304 not modified), when a `TypedResult` does not exist for that status, look for a custom result in `Utilities/Results`.  Suggest creating a new result class if one does not exist.
- When a DTO is used in multiple places, create a mapping method by adding a `DtoNameMapper` static class in the same file as the DTO, with a `ToDto()` extension method.  See `Features/Users/UserDto.cs` for an example.
- Constant values that don't fit elsewhere and are used in multiple places should be placed in `Constants.cs`.
- Switch statements or switch expressions that operator on enums should include a default case that throws `HackathonManager.Exceptions.UnexpectedEnumValueException`.  If the switch is not meant to handle all values of the enum, include a comment stating that the default case is intentionally omitted.
