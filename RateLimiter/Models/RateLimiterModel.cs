using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class RateLimiterModel
    {
        public bool RequestLimiterEnabled { get; set; }
        public int DefaultRequestLimitMs { get; set; }  
        public int DefaultRequestLimitCount { get; set; }
        public List<EndpointLimitModel> EndpointLimits { get; set; }
    }
}
