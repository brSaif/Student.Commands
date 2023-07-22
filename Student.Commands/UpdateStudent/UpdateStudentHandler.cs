using FluentValidation;
using Grpc.Core;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Domain;
using gRPCOnHttp3.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace gRPCOnHttp3.UpdateStudent;

/// <summary>
/// Represents the create student handler.
/// </summary>
public class UpdateStudentHandler : IRequestHandler<UpdateStudentRequest, Student>
{
    private readonly AppDbContext _context;

    public UpdateStudentHandler(AppDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<Student> Handle(UpdateStudentRequest request, CancellationToken cancellationToken)
    {

        var studentId = Guid.Parse(request.StudentId);
        
        if (await _context.UniqueReferences.AnyAsync(
                e => e.Name == request.Name && e.Id != studentId,
                cancellationToken: cancellationToken
            ))
        {
            throw new RpcException(
                new Status(StatusCode.AlreadyExists, Phrases.StudentNameAlreadyTaken));
        }
        
        var events = await _context.EventStore.Where(
                e => e.AggregateId == studentId)
            .OrderBy(e => e.Sequence)
            .ToListAsync(cancellationToken: cancellationToken);

        if (events.Count == 0)
            throw new RpcException(
                new Status(
                    StatusCode.NotFound, 
                    string.Format(Phrases.NoStudentWasFoundWithTheGivenId, request.StudentId))
                );

        var student = Student.LoadHistoryFromEvents(events);

        student.Update(request);
        await _context.CommitNewEventsAsync(student);

        return student;
    }
}