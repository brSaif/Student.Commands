using System.Text;
using Azure.Messaging.ServiceBus;
using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Domain.Common;
using gRPCOnHttp3.Services;
using gRPCOnHttp3.UpdateStudent;
using Newtonsoft.Json;

namespace ESgRPC.Tests.Live.Asserts;

public class MessageAsserts
{
    public static void AssertEquality(
        StudentCreatedEvent createdEvent,
        ServiceBusReceivedMessage message
    )
    {
        var body = JsonConvert.DeserializeObject<MessageBody>(Encoding.UTF8.GetString(message.Body));

        BaseAssert(createdEvent, message, body);

        var eventData = createdEvent.Data;

        var messageData = JsonConvert.DeserializeObject<StudentCreatedData>(body.Data.ToString());

        Assert.Equal(eventData.Name, messageData.Name);
        Assert.Equal(eventData.Email, messageData.Email);
        Assert.Equal(eventData.PhoneNumber, messageData.PhoneNumber);
    }

    #region StudentUpdatedAssert

    public static void AssertEquality(
        StudentUpdatedEvent updatedEvent,
        ServiceBusReceivedMessage message
    )
    {
        var body = JsonConvert.DeserializeObject<MessageBody>(Encoding.UTF8.GetString(message.Body));

        BaseAssert(updatedEvent, message, body);

        var eventData = updatedEvent.Data;

        var messageData = JsonConvert.DeserializeObject<UpdateStudentData>(body.Data.ToString());

        Assert.Equal(eventData.Name, messageData.Name);
        Assert.Equal(eventData.Email, messageData.Email);
        Assert.Equal(eventData.PhoneNumber, messageData.PhoneNumber);
    }

    #endregion


    #region BaseAssert

    private static void BaseAssert(Event cardComplaintEvent, ServiceBusReceivedMessage message, MessageBody body)
    {
        Assert.NotNull(cardComplaintEvent);
        Assert.NotNull(message);
        Assert.Equal(cardComplaintEvent.Id.ToString(), message.CorrelationId);

        Assert.NotNull(body);
        Assert.NotNull(body.Data);

        Assert.Equal(cardComplaintEvent.Sequence, body.Sequence);
        Assert.Equal(cardComplaintEvent.Version, body.Version);
        Assert.Equal(cardComplaintEvent.Type.ToString(), body.Type);
        Assert.Equal(cardComplaintEvent.DateTime, body.DateTime, TimeSpan.FromMinutes(1));
    }

    #endregion
}