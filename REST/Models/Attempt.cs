using System.Net;

namespace RateLimiter.Models
{
    /// <summary>
    /// Class that will contain one request attempt
    /// </summary>
    public class Attempt
    {
        /// <summary>
        /// IP Address from incoming request. It can be null.
        /// </summary>
        public IPAddress? IPAddress { get; set; }
        /// <summary>
        /// Endpoint name. We need to store it also since for different endpoints different rules can be applied.
        /// </summary>
        public string Endpoint { get; set; }
        /// <summary>
        /// Unix milisecond timestamp for easier calculation
        /// </summary>
        public long Timestamp { get; set; }
    }
}
