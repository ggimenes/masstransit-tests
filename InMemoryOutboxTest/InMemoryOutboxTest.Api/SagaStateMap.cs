using InMemoryOutboxTest.Api;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InMemoryOutboxTest.Api
{
    public class SagaStateMap :
        SagaClassMap<SagaState>
    {
        protected override void Configure(EntityTypeBuilder<SagaState> entity, ModelBuilder model)
        {
            entity.HasKey(c => c.CorrelationId);

            entity.Property(c => c.CorrelationId)
                .ValueGeneratedNever();

            entity.Property(c => c.CurrentState)
                .HasMaxLength(50)
                .IsRequired();

            // If using Optimistic concurrency, otherwise remove this property
            entity.Property(x => x.RowVersion).IsRowVersion();
        }
    }

}

