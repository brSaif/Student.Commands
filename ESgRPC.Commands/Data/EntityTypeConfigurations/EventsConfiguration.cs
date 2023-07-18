using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Domain.Common;
using gRPCOnHttp3.UpdateStudent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gRPCOnHttp3.Data.EntityTypeConfigurations;

public class EventsConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasIndex(e => new { e.AggregateId, e.Sequence }).IsUnique();

        builder.Property(e => e.AggregateId)
            .HasConversion(i => i.ToString(),
                n => Guid.Parse(n));
        
        builder.Property(e => e.Type)
            .HasMaxLength(128)
            .HasConversion<string>();

        builder.HasDiscriminator(e => e.Type)
            .HasValue<StudentCreatedEvent>(EventType.StudentCreated)
            .HasValue<StudentUpdatedEvent>(EventType.StudentUpdated);
    }
}