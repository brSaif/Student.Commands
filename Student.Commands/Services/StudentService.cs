using gPPCOnHttp3Server;
using Grpc.Core;
using gRPCOnHttp3.Extensions;
using MediatR;

namespace gRPCOnHttp3.Services;

public class StudentService : Student.StudentBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IMediator mediator, ILogger<StudentService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    
    public override async Task<StudentResponse> Create(CreateRequest request, ServerCallContext context)
    {
        _logger.LogInformation("[Student grpc Service] Create student request received ...");
        var result = await _mediator.Send(request.Create());
        return new StudentResponse()
        {
            Id = result.Id.ToString(), 
            Name = result.Name, 
            Email = result.Email,
            PhoneNumber = result.PhoneNumber
        }; 
    }
    
    public override async Task<StudentResponse> Update(UpdateStudentRequest request, ServerCallContext context)
    {
        _logger.LogInformation("[Student grpc Service] Change email request received ...");
        var result = await _mediator.Send(request.Create());
        return new StudentResponse()
        {
            Id = result.Id.ToString(), 
            Name = result.Name, 
            Email = result.Email,
            PhoneNumber = result.PhoneNumber
        }; 
    }
}