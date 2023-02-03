using Microsoft.AspNetCore.Http;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RateLimiter.Middleware
{
    public class RateLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimiterModel _config;
        private readonly List<Attempt> _attempts;

        public RateLimiterMiddleware(RequestDelegate next, RateLimiterModel config)
        {
            _next = next;
            _config = config;
            _attempts = new List<Attempt>();
        }

        public async Task InvokeAsync(HttpContext context)
        {            
            if (_attempts.Count > 0)
            {
                int numberOfAttempts = 0;

                // Check if requests endpoint path exists in configuration so we can use configuration settings
                if (_config.EndpointLimits.Any(l => l.Endpoint == context.Request.Path))
                {
                    foreach (var endpointConfiguration in _config.EndpointLimits)
                    {
                        numberOfAttempts = _attempts.Where(a => 
                            a.Timestamp > PastTimestamp(endpointConfiguration.RequestLimitMs) 
                            && a.Endpoint == endpointConfiguration.Endpoint).Count();

                        if (numberOfAttempts >= endpointConfiguration.RequestLimitCount)
                        {
                            // Do not allow and return too many requests
                            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                            return;
                        }
                    }
                }
                // If configuration for requests endpoint doesn't exists we will use default values
                else
                {
                    numberOfAttempts = _attempts.Where(a => a.Timestamp > PastTimestamp(_config.DefaultRequestLimitMs)).Count();
                    
                    if (numberOfAttempts >= _config.DefaultRequestLimitCount)
                    {
                        // Do not allow and return to many requests
                        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                        return;
                    }
                }
                
                _attempts.Add(
                    new Attempt()
                    {
                        Timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds(),                        
                        IPAddress = context.Request.HttpContext.Connection.RemoteIpAddress, // IP Address can be null
                        Endpoint = context.Request.Path
                    }
                );
            }
            else
            {
                // If attempts are empty, it is first attempt and we will allow it and add attempt to list
                _attempts.Add(
                    new Attempt()
                    {
                        Timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds(),                        
                        IPAddress = context.Request.HttpContext.Connection.RemoteIpAddress, // IP Address can be null
                        Endpoint = context.Request.Path
                    }
                );
            }

            // I prefer to clean object that is storing attempts and I will clean all attempts that are older then maximum RequestLimitMs since they are out of date
            var maxRequestLimit = Math.Max(_config.EndpointLimits.Max(m => m.RequestLimitMs), _config.DefaultRequestLimitMs);
            _attempts.RemoveAll(a => a.Timestamp < ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds() - maxRequestLimit);

            await _next(context);
        }

        private long PastTimestamp(int howMuchInPast)
        {
            // For this task I found useful to everything transform into Unix time miliseconds
            return ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds() - howMuchInPast;
        }
    }
}
