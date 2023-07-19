using gRPCOnHttp3.Domain.Common;

namespace gRPCOnHttp3.Data;

public class OutboxMessage
{
    public long Id { get; private set; }
    public Event Event { get; private set; }
    
    public OutboxMessage(Event @event)
    {
        Event = @event;
    }
    
    private OutboxMessage(){}
}