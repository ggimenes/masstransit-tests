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
        private readonly ILogger<FirstActivity> _logger;

        public FirstActivity(ILogger<FirstActivity> logger)
        {
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

            if (context.Data.AsyncPublish)
                DoAsyncPublish(context, secondEvent);
            else
                await DoSyncPublish(context, secondEvent);

            context.Instance.AppliedDelayBeforeSaveChanges = context.Data.DelayAfterPublish;
            await Task.Delay(context.Data.DelayAfterPublish);

            _logger.LogInformation($"ExecutionId: {secondEvent.ExecutionId} - Delay Finished.. Saving Changes");

            await next.Execute(context).ConfigureAwait(false);
        }

        private void DoAsyncPublish(BehaviorContext<SagaState, FirstEventReceived> context, SecondEvent secondEvent)
        {
            context.Instance.SecondEventPublishedAt = DateTime.UtcNow.AddMilliseconds(1000);
            // Fire and forget.. trying to publish after save changes, if it happens before
            // the save changes, the saga still doesn't exists and the message will be descarted
            _ = Task.Run(() =>
            {
                Task.Delay(1000).Wait();
                context.Publish(secondEvent).Wait();
                _logger.LogInformation($"ExecutionId: {secondEvent.ExecutionId} - Async Publish called after 1000ms - SecondEvent");
            });
        }

        private async Task DoSyncPublish(BehaviorContext<SagaState, FirstEventReceived> context, SecondEvent secondEvent)
        {
            await context.Publish(secondEvent);
            context.Instance.SecondEventPublishedAt = DateTime.UtcNow;

            _logger.LogInformation($"ExecutionId: {secondEvent.ExecutionId} - Sync Publish called SecondEvent");
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
