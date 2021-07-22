using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMemoryOutboxTest.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnqueueController : ControllerBase
    {
        private readonly ILogger<EnqueueController> _logger;
        private readonly IBus _bus;

        public EnqueueController(ILogger<EnqueueController> logger, IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delayAfterPublish">Delay in ms applied after publish has been called. Used to check if the publish occours before or after this delay</param>
        /// <param name="asyncPublish">If true, will make a fire and forget thread with 1000ms of delay before execute the publish. Could be used to process the second event, deferring publish just like the InMemoryOutbox</param>        
        /// <returns></returns>
        [HttpPost("first-event")]
        public async Task<ActionResult> Get(int delayAfterPublish, bool asyncPublish)
        {
            await _bus.Publish(new FirstEventReceived()
            {
                ExecutionId = Guid.NewGuid(),
                DelayAfterPublish = delayAfterPublish,
                AsyncPublish  = asyncPublish
            });
            return Ok();
        }
    }
}
