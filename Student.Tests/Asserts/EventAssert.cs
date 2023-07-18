using ES_gRPC.UnitTests.Grpc.Protos;
using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Domain.Common;
using gRPCOnHttp3.UpdateStudent;
using UpdateStudentRequest = ES_gRPC.UnitTests.Grpc.Protos.UpdateStudentRequest;

namespace ES_gRPC.UnitTests.Asserts;

public static class EventAssert
{
    #region #CreateStudentAssertions

    public static void AssertEquality(
        CreateRequest createRequest,
        StudentResponse studentResponse,
        Event studentEvent)
    {
        Assert.NotNull(createRequest);
        Assert.NotNull(studentResponse);
        Assert.NotNull(studentEvent);

        // Request vs Output
        Assert.Equal(createRequest.Name, studentResponse.Name);
        Assert.Equal(createRequest.Email, studentResponse.Email);
        Assert.Equal(createRequest.PhoneNumber, studentResponse.PhoneNumber);

        // Output vs Event
        Assert.Equal(studentResponse.Id, studentEvent.AggregateId.ToString());
        Assert.Equal(1, studentEvent.Sequence);
        Assert.Equal(1, studentEvent.Version);
        Assert.Equal(EventType.StudentCreated, studentEvent.Type);
        Assert.Equal(DateTime.UtcNow, studentEvent.DateTime, TimeSpan.FromMinutes(1));

        var @event = (StudentCreatedEvent)studentEvent;

        var data = @event.Data;

        // Output vs Event data 
        Assert.Equal(studentResponse.Name, data.Name);
        Assert.Equal(studentResponse.Email, data.Email);
        Assert.Equal(studentResponse.PhoneNumber, data.PhoneNumber);
    }


    public static void AssertEquality(
        Event studentEvent,
        OutboxMessage message
    )
    {
        Assert.NotNull(studentEvent);
        Assert.NotNull(message);

        Assert.Equal(studentEvent.Sequence, message.Event.Sequence);
        Assert.Equal(1, message.Event.Version);
        Assert.Equal(studentEvent.Type, message.Event.Type);
        Assert.Equal(studentEvent.DateTime, message.Event.DateTime, TimeSpan.FromMinutes(1));

        Assert.Equal(((StudentCreatedEvent)studentEvent).Data, ((StudentCreatedEvent)message.Event).Data);
        Assert.Equal(studentEvent.Id, message.Event.Id);
    }

    #endregion

    #region #UpdateStudentAssertions

    // Update student
    public static void AssertEquality(
        UpdateStudentRequest updateStudentRequest,
        StudentResponse studentResponse,
        Event studentEvent,
        int sequence)
    {
        Assert.NotNull(updateStudentRequest);
        Assert.NotNull(studentResponse);
        Assert.NotNull(studentEvent);

        // Request vs Output
        Assert.Equal(updateStudentRequest.Name, studentResponse.Name);
        Assert.Equal(updateStudentRequest.Email, studentResponse.Email);
        Assert.Equal(updateStudentRequest.PhoneNumber, studentResponse.PhoneNumber);

        // Output vs Event
        Assert.Equal(studentResponse.Id, studentEvent.AggregateId.ToString());
        Assert.Equal(sequence, studentEvent.Sequence);
        Assert.Equal(1, studentEvent.Version);
        Assert.Equal(EventType.StudentUpdated, studentEvent.Type);
        Assert.Equal(DateTime.UtcNow, studentEvent.DateTime, TimeSpan.FromMinutes(1));

        var @event = (StudentUpdatedEvent)studentEvent;

        var data = @event.Data;

        // Output vs Event data 
        Assert.Equal(studentResponse.Name, data.Name);
        Assert.Equal(studentResponse.Email, data.Email);
        Assert.Equal(studentResponse.PhoneNumber, data.PhoneNumber);
    }

    public static void AssertEquality(
        OutboxMessage message,
        Event updatedStudentEvent
    )
    {
        Assert.NotNull(updatedStudentEvent);
        Assert.NotNull(message);

        Assert.Equal(updatedStudentEvent.Sequence, message.Event.Sequence);
        Assert.Equal(1, message.Event.Version);
        Assert.Equal(updatedStudentEvent.Type, message.Event.Type);
        Assert.Equal(updatedStudentEvent.DateTime, message.Event.DateTime, TimeSpan.FromMinutes(1));

        Assert.Equal(((StudentUpdatedEvent)updatedStudentEvent).Data, ((StudentUpdatedEvent)message.Event).Data);
        Assert.Equal(updatedStudentEvent.Id, message.Event.Id);
    }

    #endregion
}