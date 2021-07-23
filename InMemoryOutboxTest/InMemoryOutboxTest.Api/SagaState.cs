using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMemoryOutboxTest.Api
{
    public class SagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AppliedDelayBeforeSaveChanges { get; set; }
        public DateTime SecondEventPublishedAt { get; set; }
        public DateTime FinishedAt { get; set; }

        public byte[] RowVersion { get; set; }
    }

}
