using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace ESgRPC.Tests.Live;

public class Listener
{

    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSessionProcessor _processor;

    public Listener(IConfiguration configuration)
    {
        _serviceBusClient = new ServiceBusClient(configuration["ServiceBus:ConnectionString"]);

        var options = new ServiceBusSessionProcessorOptions
        {
            AutoCompleteMessages = false,
            PrefetchCount = 1,
            MaxConcurrentCallsPerSession = 1,
            MaxConcurrentSessions = 100,
        };

        _processor = _serviceBusClient.CreateSessionProcessor("brseif", "esdemo", options);

        // configure the message and error handler to use .
        _processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
        _processor.ProcessErrorAsync += Processor_ProcessErrorAsync;

        _processor.StartProcessingAsync();
    }

    public List<ServiceBusReceivedMessage> Messages { get; set; } = new();

    private async Task Processor_ProcessMessageAsync(ProcessSessionMessageEventArgs arg)
    {
        Messages.Add(arg.Message);

        await arg.CompleteMessageAsync(arg.Message);
    }

    private Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        throw arg.Exception;
    }

    public Task CloseAsync() => _processor.CloseAsync();

}