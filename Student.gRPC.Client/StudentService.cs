using gPPCOnHttp3Client;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace gRPCOnHttp3Client;

/// <summary>
/// A greeting service that uses a constructor DI gRPC client.
/// </summary>
public class StudentService : Student.StudentClient
{
    private readonly Student.StudentClient _client;

    public StudentService(IServiceProvider sp)
    {
        
        this._client = sp.GetRequiredService<Student.StudentClient>();
    }

    public override StudentResponse Create(CreateRequest request, CallOptions options)
    {
        Console.WriteLine("[gRPC Client] Logging the create student request ... \n" + request);
        var response = _client.Create(request);
        Console.WriteLine("[gRPC Client] Logging the create student response ... \n" + response);
        return response;
    }

    public override StudentResponse Update(UpdateStudentRequest request, CallOptions options)
    {
        Console.WriteLine("[gRPC Client][Student Service] Logging request ... \n Request :" +request);
        var response = _client.Update(request);
        Console.WriteLine("[gRPC Client][Student Service] Logging response ... \n Response :" + response);
        return response;
    }

}