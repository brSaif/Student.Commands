using gRPCOnHttp3.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace gRPCOnHttp3.Data.EntityTypeConfigurations;

public class GenericEventConfiguration<TEntity, TData> : IEntityTypeConfiguration<TEntity>
    where TEntity : Event<TData>
    where TData : IEventData
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.Data).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<TData>(v)
        ).HasColumnName("Data");
    }
}