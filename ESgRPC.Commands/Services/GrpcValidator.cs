using Calzolari.Grpc.AspNetCore.Validation;
using Calzolari.Grpc.AspNetCore.Validation.Internal;
using Calzolari.Grpc.Domain;
using FluentValidation.Results;
using Grpc.Core;

namespace gRPCOnHttp3.Services;

public class GrpcValidator
{
    private readonly IValidatorErrorMessageHandler _handler;
    private readonly List<ValidationFailure> _failures = new();

    public GrpcValidator(IValidatorErrorMessageHandler handler)
    {
        _handler = handler;
    }

    public bool IsValid => _failures.Count == 0;

    public void AddError(string propertyName, string errorMessage)
    {
        _failures.Add(new ValidationFailure(propertyName, errorMessage));
    }

    public async Task<RpcException> RpcExceptionAsync(StatusCode statusCode = StatusCode.InvalidArgument)
    {
        var message = await _handler.HandleAsync(_failures);

        var trailers = _failures.Select(x => new ValidationTrailers
        {
            PropertyName = x.PropertyName,
            AttemptedValue = x.AttemptedValue?.ToString(),
            ErrorMessage = x.ErrorMessage
        }).ToList();

        var metadata = new Metadata
        {
            new Metadata.Entry(
                "validation-errors-text",
                trailers.ToBase64()
            )
        };

        _failures.Clear();

        return new RpcException(new Status(statusCode, message), metadata);
    }

}