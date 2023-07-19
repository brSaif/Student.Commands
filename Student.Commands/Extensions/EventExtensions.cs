using gPPCOnHttp3Server;
using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Domain;
using gRPCOnHttp3.Domain.Common;
using gRPCOnHttp3.UpdateStudent;
using UpdateStudentRequest = gRPCOnHttp3.UpdateStudent.UpdateStudentRequest;

namespace gRPCOnHttp3.Extensions;

public static class EventExtensions
{
    public static StudentUpdatedEvent ToEvent(this UpdateStudentRequest request, int sequence)
        => new(
            aggregateId: Guid.Parse(request.StudentId), 
            sequence: sequence,
            data: new UpdateStudentData(
                Name: request.Name,
                Email: request.Email,
                PhoneNumber: request.PhoneNumber
                )
        );
    
    public static StudentCreatedEvent ToEvent(this CreateStudentRequest request)
        => new(
            aggregateId: Guid.NewGuid(), 
            data: new StudentCreatedData(
                Name: request.Name,
                Email: request.Email,
                PhoneNumber: request.PhoneNumber
                )
        );

    public static IEnumerable<OutboxMessage> ToOutboxMessages(this IEnumerable<Event> events)
        => events.Select(e => new OutboxMessage(e));
}