using System.Linq;
using System.Text.Json;
using FastEndpoints;
using FluentValidation;
using FluentValidation.Results;
using JetBrains.Annotations;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HackathonManager.Utilities.JsonPatch;

/// <summary>
///     Base type for any request that uses JsonPatch.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public abstract class PatchRequest<TModel>
    where TModel : class
{
    [FromBody]
    public required Operation<TModel>[] Operations
    {
        get;
        [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
        init;
    }

    /// <summary>
    ///     Applies the <see cref="Operations" /> to <paramref name="model" />.  If any errors occur,
    ///     uses <see cref="ValidationContext" /> to throw a validation error.
    /// </summary>
    /// <param name="model"></param>
    public void ApplyOperationsTo(TModel model)
    {
        try
        {
            // I think we need to serialize with STJ and re-deserialize with Newtonsoft because the operation's value
            // property is handled dynamically, so the STJ type used for that property isn't compatible with the
            // JsonPatchDocument, which depends on Newtonsoft.
            new JsonPatchDocument<TModel>(
                Operations
                    .Select(p =>
                        JsonConvert.DeserializeObject<Operation<TModel>>(
                            JsonSerializer.Serialize(p, JsonSerializerOptions.Web)
                        )
                    )
                    .ToList(),
                new DefaultContractResolver()
            ).ApplyTo(model);
        }
        catch (JsonPatchException ex)
        {
            var operation = ex.FailedOperation;

            var path = operation.path;
            if (path.StartsWith('/'))
            {
                path = path[1..].Replace(oldChar: '/', newChar: '.');
            }

            ValidationFailure failure;
            if (operation.OperationType is OperationType.Test)
            {
                failure = new ValidationFailure(path, ex.Message)
                {
                    ErrorCode = ErrorCodes.JsonPatchTestFailed,
                    Severity = Severity.Error,
                };
            }
            else
            {
                failure = new ValidationFailure(path, ex.Message, operation.value)
                {
                    ErrorCode = ErrorCodes.JsonPatchFailed,
                    Severity = Severity.Error,
                };
            }

            ValidationContext.Instance.ThrowError(failure);
        }
    }
}
