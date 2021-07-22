using Automatonymous;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMemoryOutboxTest.Api
{
    public class SecondActivity : Activity<SagaState, SecondEvent>
    {
        private readonly ILogger<SecondActivity> _logger;

        public SecondActivity(ILogger<SecondActivity> logger)
        {
            _logger = logger;
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<SagaState, SecondEvent> context, Behavior<SagaState, SecondEvent> next)
        {
            context.Instance.SecondEventReceived = DateTime.UtcNow;
            int actualDelay = (int)(context.Instance.SecondEventReceived - context.Instance.SecondEventPublishedAt).TotalMilliseconds;
            context.Instance.ObservedInMemoryOutboxDelay = actualDelay;

            _logger.LogInformation($"ExecutionId: {context.Data.ExecutionId} - SecondEvent Received: {context.Instance.SecondEventReceived.ToString("mm:ss.fff")}");

            context.Instance.FinishedAt = DateTime.UtcNow;

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<SagaState, SecondEvent, TException> context, Behavior<SagaState, SecondEvent> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("second-activity");
        }
    }
}
