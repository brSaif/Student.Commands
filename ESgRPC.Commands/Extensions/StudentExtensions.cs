using gPPCOnHttp3Server;
using gRPCOnHttp3.CreateStudent;
using UpdateStudentRequest = gRPCOnHttp3.UpdateStudent.UpdateStudentRequest;

namespace gRPCOnHttp3.Extensions;

public static class StudentExtensions
{
    public static CreateStudentRequest Create(this CreateRequest grpcRequest)
        => new(
            grpcRequest.Name, 
            grpcRequest.Email, 
            grpcRequest.PhoneNumber
            );
    
    public static UpdateStudentRequest Create(this gPPCOnHttp3Server.UpdateStudentRequest grpcRequest)
        => new(
            grpcRequest.Id,
            grpcRequest.Name,
            grpcRequest.Email,
            grpcRequest.PhoneNumber
            );
}