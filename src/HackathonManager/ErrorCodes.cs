namespace HackathonManager;

public static class ErrorCodes
{
    public const string EmailAddress = "validation_email_address";
    public const string InvalidETag = "validation_invalid_etag";
    public const string InvalidLength = "validation_invalid_length";
    public const string JsonPatchFailed = "json_patch_failed";
    public const string JsonPatchInvalidOperation = "validation_json_patch_operation";
    public const string JsonPatchInvalidSequence = "validation_json_patch_sequence";
    public const string JsonPatchTestFailed = "json_patch_test_failed";
    public const string MaxLength = "validation_max_length";
    public const string NotEmpty = "validation_not_empty";
    public const string NotNull = "validation_not_null";
    public const string TypeIdInvalid = "validation_typeid";
    public const string UniqueValueRequired = "unique_value_required";
}
