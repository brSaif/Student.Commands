using gRPCOnHttp3.Domain;
using gRPCOnHttp3.Extensions;
using gRPCOnHttp3.Services;

namespace gRPCOnHttp3.Data;

public interface IUnitOfWork
{
    Task CommitNewEventsAsync(Student student);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IServiceBusPublisher _publisher;

    public UnitOfWork(
        AppDbContext context, 
        IServiceBusPublisher serviceBusPublisher)
    {
        _context = context;
        _publisher = serviceBusPublisher;
    }
    
    public async Task CommitNewEventsAsync(Student student)
    {
        var newEvents = student.GetUncommittedEvents();

        await _context.EventStore.AddRangeAsync(newEvents);

        var messages = newEvents.ToOutboxMessages();

        await _context.OutboxMessages.AddRangeAsync(messages);

        await _context.SaveChangesAsync();

        student.MarkChangesAsCommitted();
        
        await _publisher.PublishAsync(messages);
    }
}