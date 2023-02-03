using RateLimiter.Models;
using System;
using System.Collections.Generic;

namespace RateLimiter
{
    public class RateLimiter
    {
        public RateLimiterModel rateLimiterModel;
        public RateLimiter(RateLimiterModel rateLimiterModel)
        {
            this.rateLimiterModel = rateLimiterModel;
            //rateLimiterModel = new RateLimiterModel();
            //rateLimiterModel.RequestLimiterEnabled = true;
            //rateLimiterModel.DefaultRequestLimitMs = 1000;
            //rateLimiterModel.DefaultRequestLimitCount = 10;

            //rateLimiterModel.EndpointLimits = new List<EndpointLimitModel> { new EndpointLimitModel() };

            //rateLimiterModel.EndpointLimits.Add(new EndpointLimitModel() {Endpoint = "/api/products/books", RequestLimitMs = 1000, RequestLimitCount = 1});
            //rateLimiterModel.EndpointLimits.Add(new EndpointLimitModel() {Endpoint = "/api/products/pencils", RequestLimitMs = 500, RequestLimitCount = 2});
        }
    }
}