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
    public class FirstActivity : Activity<SagaState, FirstEventReceived>
    {
        private readonly IBus _bus;
        private readonly ILogger<FirstActivity> _logger;

        public FirstActivity(IBus bus, ILogger<FirstActivity> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<SagaState, FirstEventReceived> context, Behavior<SagaState, FirstEventReceived> next)
        {
            context.Instance.CreatedAt = DateTime.UtcNow;
            var secondEvent = new SecondEvent()
            {
                ExecutionId = context.Data.ExecutionId
            };

            await _bus.Publish(secondEvent);
            context.Instance.SecondEventPublishedAt = DateTime.UtcNow;

            _logger.LogInformation($"ExecutionId: {secondEvent.ExecutionId} - Publish called SecondEvent");

            context.Instance.AppliedDelayBeforeSaveChanges = context.Data.DelayAfterPublish;
            await Task.Delay(context.Data.DelayAfterPublish);

            _logger.LogInformation($"ExecutionId: {secondEvent.ExecutionId} - Delay Finished.. Saving Changes");

            context.Instance.FinishedAt = DateTime.UtcNow;

           await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<SagaState, FirstEventReceived, TException> context, Behavior<SagaState, FirstEventReceived> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("first-activity");
        }
    }
}
