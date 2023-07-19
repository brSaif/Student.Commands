using gRPCOnHttp3.Domain.Common;

namespace gRPCOnHttp3.CreateStudent;

public class StudentCreatedEvent : Event<StudentCreatedData>
{
    private StudentCreatedEvent() { }

    public StudentCreatedEvent(
        Guid aggregateId,
        StudentCreatedData data,
        int version = 1
        ) : base(aggregateId, sequence: 1, data, version)
    { }
}