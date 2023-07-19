using System.Net;
using System.Text;
using FluentValidation;
using Grpc.Core;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Domain;
using gRPCOnHttp3.Domain.Common;
using gRPCOnHttp3.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace gRPCOnHttp3.CreateStudent;

/// <summary>
/// Represents the create student handler.
/// </summary>
public class CreateStudentHandler : IRequestHandler<CreateStudentRequest, Student>
{
    private readonly AppDbContext _context;

    public CreateStudentHandler(
        AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Student> Handle(
        CreateStudentRequest request, 
        CancellationToken cancellationToken
        )
    {
        if (await _context.UniqueReferences.AnyAsync(
                        e => e.Name == request.Name,
                        cancellationToken: cancellationToken
                    ))
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, ""));
                }
                        
        var student = Student.Create(request);
        
        foreach (var @event in student.GetUncommittedEvents())
        {
            if (@event.Type == EventType.StudentCreated)
                await _context.UniqueReferences.AddAsync(
                    new UniqueReference(student),
                    cancellationToken
                    );
        }

        await _context.CommitNewEventsAsync(student);

        return student;
    }
}
