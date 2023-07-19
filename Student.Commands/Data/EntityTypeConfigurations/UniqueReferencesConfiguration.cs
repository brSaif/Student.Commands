using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gRPCOnHttp3.Data.EntityTypeConfigurations;

public class UniqueReferencesConfiguration : IEntityTypeConfiguration<UniqueReference>
{
    public void Configure(EntityTypeBuilder<UniqueReference> builder)
    {
        builder.Property(e => e.Id)
            .IsRequired()
            .HasConversion(i => i.ToString(), 
                n => Guid.Parse(n))
            .HasMaxLength(128);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(e => new { e.Id, e.Name }).IsUnique();
    }
}