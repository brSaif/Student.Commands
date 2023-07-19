using System.Text;
using Azure.Messaging.ServiceBus;
using DemoEvents;
using Grpc.Core;
using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Domain.Common;
using gRPCOnHttp3.UpdateStudent;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UpdateStudentRequest = DemoEvents.UpdateStudentRequest;

namespace gRPCOnHttp3.Services;

public class DemoEvents : global::DemoEvents.DemoEvents.DemoEventsBase
{
    private readonly ILogger<DemoEvents> _logger;
    private readonly ServiceBusSender _sender;

    public DemoEvents(
        ServiceBusClient busClient,
        IOptions<ServiceBusPublisherOptions> serviceBusConfig,
        ILogger<DemoEvents> logger)
    {
        _logger = logger;
        _sender = busClient.CreateSender(serviceBusConfig.Value.TopicName);
    }

    public override async Task<Empty> Create(CreateRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Received request: '{request}'");
        var studentCreatedEvent = new StudentCreatedEvent(
                Guid.Parse(request.AggregateId),
                new StudentCreatedData(request.Name, request.Email, request.PhoneNumber)
            );

        await SendMessageAsync(studentCreatedEvent);
        _logger.LogInformation($"Student created event sent successfully: '{studentCreatedEvent}'");
        return new Empty { };
    }

    public override async Task<Empty> Update(UpdateStudentRequest request, ServerCallContext context)
    {        
        _logger.LogInformation($"Received request: '{request}'");
        var studentUpdatedEvent = new StudentUpdatedEvent(
            Guid.Parse(request.AggregateId),
            request.Sequence,
            new UpdateStudentData(request.Name, request.Email, request.PhoneNumber)
        );

        await SendMessageAsync(studentUpdatedEvent);
        _logger.LogInformation($"Student updated event sent successfully: '{studentUpdatedEvent}'");
        return new Empty { };
    }
    
    private async Task SendMessageAsync(Event @event)
    {
        var body = new MessageBody()
        {
            AggregateId = @event.AggregateId,
            DateTime = @event.DateTime,
            Sequence = @event.Sequence,
            Type = @event.Type.ToString(),
            Version = @event.Version,
            Data = ((dynamic)@event).Data
        };

        var json = JsonConvert.SerializeObject(body);

        await _sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(json))
        {
            CorrelationId = @event.Id.ToString(),
            MessageId = @event.Id.ToString(),
            PartitionKey = @event.AggregateId.ToString(),
            SessionId = @event.AggregateId.ToString(),
            Subject = @event.Type.ToString(),
            ApplicationProperties = {
                { nameof(@event.AggregateId), Guid.Parse(@event.AggregateId.ToString()) },
                { nameof(@event.Sequence), @event.Sequence },
                { nameof(@event.Version), @event.Version },
            }
        });
    }
}