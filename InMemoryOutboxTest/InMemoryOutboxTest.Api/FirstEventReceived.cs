using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMemoryOutboxTest.Api
{
    public class FirstEventReceived
    {
        public Guid ExecutionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int DelayAfterPublish { get; set; }
    }
}
