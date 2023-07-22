using System.Globalization;
using System.Text;
using Calzolari.Grpc.AspNetCore.Validation;
using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Services;
using gRPCOnHttp3.UpdateStudent;

namespace gRPCOnHttp3.Extensions;

public static class GrpcExtensions
{
    public static void AddGrpcWithValidation(this IServiceCollection services)
    {
        // services.AddScoped<GrpcValidator>();
        
        // Step 1
        services.AddGrpc(opt =>
        {
            opt.EnableMessageValidation();
            // opt.Interceptors.Add<ThreadCultureInterceptor>();
            // opt.Interceptors.Add<ApplicationExceptionInterceptor>();
        });

        services.AddGrpcValidation();
        // Step 2
        services.AddValidator<CreateStudentRequestValidator>();
        services.AddValidator<UpdateStudentRequestValidator>();

        services.AddValidators();

        // Step 3
    }
    
    
    public static async Task ValidateRequestAndThrow<T>(
        this IValidator<T> validator,
        T request,
        CancellationToken cancellationToken
        )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
        {
        
            StringBuilder sb = new StringBuilder();
            sb.Append($"An exception is thrown due to the following {validationResult.Errors.Count} validation errors\n");
            validationResult
                .Errors
                .ForEach(x =>
                {
                    sb.Append($"Error code: '{x.ErrorCode}' ");
                    sb.Append($"Property name: '{x.PropertyName}' ");
                    sb.Append($"Error message: '{x.ErrorMessage}'\n ");
                });
            
            throw new RpcException(new Status(StatusCode.FailedPrecondition, sb.ToString()));
        }
    }
}

public class ThreadCultureInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var headerLanguage = context.RequestHeaders.FirstOrDefault(t => t.Key == "language");

        if (headerLanguage != null)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(headerLanguage.Value);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(headerLanguage.Value);
        }

        return await continuation(request, context);
    }


}

public class ApplicationExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception exception)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, exception.Message));
        }
    }
}