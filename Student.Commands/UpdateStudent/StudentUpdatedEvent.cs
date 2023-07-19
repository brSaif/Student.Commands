using gRPCOnHttp3.Domain.Common;

namespace gRPCOnHttp3.UpdateStudent;

/// <summary>
/// Represents the <see cref="StudentUpdatedEvent"/> class.
/// </summary>
public class StudentUpdatedEvent : Event<UpdateStudentData>
{
    private StudentUpdatedEvent() { }

    /// <summary>
    /// Initializes anew instance of the <see cref="StudentUpdatedEvent"/>
    /// </summary>
    /// <param name="aggregateId">The aggregate event id.</param>
    /// <param name="sequence">The sequence event id.</param>
    /// <param name="data">The event data.</param>
    /// <param name="version">The event version.</param>
    public StudentUpdatedEvent(
            Guid aggregateId, 
            int sequence,
            UpdateStudentData data, 
            int version = 1
        ) : base(aggregateId, sequence: sequence, data, version)
    { }
}