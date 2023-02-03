using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class EndpointLimitModel
    {
        public string Endpoint { get; set; }    
        public int RequestLimitMs { get; set; }
        public int RequestLimitCount { get; set; }
    }
}
