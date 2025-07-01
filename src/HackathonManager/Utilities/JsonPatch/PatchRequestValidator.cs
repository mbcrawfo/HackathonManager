using FastEndpoints;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace HackathonManager.Utilities.JsonPatch;

public abstract class PatchRequestValidator<TRequest, TModel> : Validator<TRequest>
    where TRequest : PatchRequest<TModel>
    where TModel : class
{
    /// <summary>
    ///     Creates a new instance of <see cref="PatchRequestValidator{TRequest, TModel}" />.
    /// </summary>
    /// <param name="requireTestBeforeMutate">
    ///     When true, adds validation rules requiring that any operations which mutate a value must be immediately
    ///     preceded by a test operation for the same property.
    /// </param>
    protected PatchRequestValidator(bool requireTestBeforeMutate = false)
    {
        RuleForEach(x => x.Operations)
            .Must(x => x.OperationType is not OperationType.Invalid)
            .WithErrorCode(ErrorCodes.JsonPatchInvalidOperation)
            .WithMessage((_, x) => $"Invalid operation type '{x.op}'.");

        if (requireTestBeforeMutate)
        {
            RuleFor(x => x.Operations).Custom(ValidateOperationSequence);
        }
    }

    private static void ValidateOperationSequence(
        Operation<TModel>[] operations,
        FluentValidation.ValidationContext<TRequest> context
    )
    {
        for (var i = 0; i < operations.Length; i++)
        {
            var twoBack = i < 2 ? null : operations[i - 2];
            var prev = i is 0 ? null : operations[i - 1];
            var current = operations[i];

            var validOrdering = (prev?.OperationType, current.OperationType) switch
            {
                // Ignore invalid operations.
                (_, OperationType.Invalid) => true,
                // Tests are always valid regardless of what comes before.
                (_, OperationType.Test) => true,
                // Remove followed by add (which is synonymous with a replace) is ok if working on the same path.
                (OperationType.Remove, OperationType.Add) when prev.path == current.path => true,
                // Move is a special case that mutates two properties (path is written, from is removed), meaning that
                // it requires two tests.
                (OperationType.Test, OperationType.Move)
                    when twoBack is { OperationType: OperationType.Test }
                        && (prev.path == current.path || twoBack.path == current.path)
                        && (prev.path == current.from || twoBack.path == current.from) => true,
                // A test followed by a mutation of the same path is ok.
                (OperationType.Test, not OperationType.Move) when prev.path == current.path => true,
                // All other permutations are disallowed.
                _ => false,
            };

            if (validOrdering)
            {
                continue;
            }

            var opString = current.OperationType.ToString().ToLowerInvariant();
            var testOp = nameof(OperationType.Test).ToLowerInvariant();
            var removeOp = nameof(OperationType.Remove).ToLowerInvariant();
            var messagePrefix = $"Invalid operation at index {i}: '{opString}'";
            var errorMessage = current.OperationType switch
            {
                OperationType.Add =>
                    $"{messagePrefix} must be preceded by a '{testOp}' or '{removeOp}' operation with the same 'path' value.",
                OperationType.Move =>
                    $"{messagePrefix} must be preceded by two '{testOp}' operations to validate both 'path' and 'from'.",
                _ => messagePrefix + $" must be preceded by a '{testOp}' operation with the same 'path' value.",
            };

            context.AddFailure(
                new ValidationFailure
                {
                    ErrorCode = ErrorCodes.JsonPatchInvalidSequence,
                    ErrorMessage = errorMessage,
                    Severity = Severity.Error,
                }
            );
        }
    }
}
