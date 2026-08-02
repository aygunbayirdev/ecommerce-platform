using ECommercePlatform.SharedKernel;
using FluentValidation;
using MediatR;

namespace ECommercePlatform.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        var error = Error.Validation(
            "Validation.Failed",
            string.Join(" | ", failures.Select(failure => failure.ErrorMessage)));

        return CreateValidationResult<TResponse>(error);
    }

    private static TResult CreateValidationResult<TResult>(Error error)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Failure(error) as TResult)!;
        }

        var resultValueType = typeof(TResult).GetGenericArguments()[0];

        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(resultValueType);

        return (TResult)failureMethod.Invoke(null, [error])!;
    }
}
