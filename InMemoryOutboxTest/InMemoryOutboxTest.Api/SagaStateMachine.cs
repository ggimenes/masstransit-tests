using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMemoryOutboxTest.Api
{
    public class SagaStateMachine :
        MassTransitStateMachine<SagaState>
    {
        public SagaStateMachine()
        {
            Event(() => FirstEventReceived, x => x.CorrelateById(m => m.Message.ExecutionId));
            Event(() => SecondEvent, x => x.CorrelateById(m => m.Message.ExecutionId));

            InstanceState(x => x.CurrentState);

            Initially(
               When(FirstEventReceived)
                   .Activity(x => x.OfType<FirstActivity>())
                   .TransitionTo(Started));

            DuringAny(
                When(SecondEvent)
                   .Activity(x => x.OfType<SecondActivity>())
                   .Finalize());
        }

        public State Started { get; private set; }

        public Event<FirstEventReceived> FirstEventReceived { get; private set; }
        public Event<SecondEvent> SecondEvent { get; private set; }
    }
}

