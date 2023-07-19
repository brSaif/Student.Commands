using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Data.EntityTypeConfigurations;
using gRPCOnHttp3.Domain;
using gRPCOnHttp3.Domain.Common;
using gRPCOnHttp3.Extensions;
using gRPCOnHttp3.Services;
using gRPCOnHttp3.UpdateStudent;
using Microsoft.EntityFrameworkCore;

namespace gRPCOnHttp3.Data;

public sealed class AppDbContext : DbContext
{
    private readonly IServiceBusPublisher _publisher;
    public DbSet<UniqueReference> UniqueReferences { get; set; }
    public DbSet<Event> EventStore { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> opt, IServiceBusPublisher publisher): base(opt)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventsConfiguration());
        modelBuilder.ApplyConfiguration(new UniqueReferencesConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessagesConfiguration());
        modelBuilder.ApplyConfiguration(new GenericEventConfiguration<StudentCreatedEvent, StudentCreatedData>());
        modelBuilder.ApplyConfiguration(new GenericEventConfiguration<StudentUpdatedEvent, UpdateStudentData>());
        base.OnModelCreating(modelBuilder);
    }
    
    public async Task CommitNewEventsAsync(Student student)
    {
        var newEvents = student.GetUncommittedEvents();

        await EventStore.AddRangeAsync(newEvents);

        var messages = newEvents.ToOutboxMessages();

        await OutboxMessages.AddRangeAsync(messages);

        await SaveChangesAsync();

        student.MarkChangesAsCommitted();
        
        await _publisher.PublishAsync(messages);
    }
}