using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace InMemoryOutboxTest.Api
{
    public class TestSagaDbContext : SagaDbContext
    {
        public DbSet<SagaState> SagaStates { get; set; }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new SagaStateMap(); }
        }

        public TestSagaDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
