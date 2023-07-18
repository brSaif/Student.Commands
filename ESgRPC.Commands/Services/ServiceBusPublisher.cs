using System.Text;
using Azure.Messaging.ServiceBus;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gRPCOnHttp3.Services;

public interface IServiceBusPublisher
{
    Task PublishAsync(IEnumerable<OutboxMessage> messages);
}

public class ServiceBusPublisher : IServiceBusPublisher
{
    private readonly IServiceProvider _provider;
    private readonly ServiceBusSender _sender;

        private static readonly object _lockObject = new();

        public ServiceBusPublisher(
            IServiceProvider _provider,
            ServiceBusClient busClient,
            IOptions<ServiceBusPublisherOptions> serviceBusConfig
        )
        {
            this._provider = _provider;
            _sender = busClient.CreateSender(serviceBusConfig.Value.TopicName);
        }


        public async Task PublishAsync(IEnumerable<OutboxMessage> messages)
        {

            using var scope = _provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            context.AttachRange(messages);

            // Don't wait
            Task.Run(PublishNonPublishedMessages).GetAwaiter();
        }

        private void PublishNonPublishedMessages()
        {
            lock (_lockObject)
            {
                using var scope = _provider.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var messages = context.OutboxMessages
                    .Include(e => e.Event)
                    .ToList();// add include()

                PublishAndRemoveMessages(messages, context).GetAwaiter().GetResult();
            }
        }


        private async Task PublishAndRemoveMessages(IEnumerable<OutboxMessage> messages, AppDbContext context)
        {
            foreach (var message in messages)
            {
                await SendMessageAsync(message.Event);

                context.OutboxMessages.Remove(message); 

                await context.SaveChangesAsync();
            }
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

public class ServiceBusPublisherOptions
{
    public string ConnectionString { get; set; }
    public string TopicName { get; set; }
}