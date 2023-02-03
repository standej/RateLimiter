using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter;
using Microsoft.OpenApi.Models;
using RateLimiter.Models;
using RateLimiter.Middleware;

namespace REST
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Get data from SQL, SQLite, local JSON/XML ...
            RateLimiter.RateLimiter rateLimiter = new RateLimiter.RateLimiter
                (
                new RateLimiter.Models.RateLimiterModel()
                {
                    RequestLimiterEnabled = true,
                    DefaultRequestLimitMs = 1000,
                    DefaultRequestLimitCount = 10,
                    EndpointLimits = new List<RateLimiter.Models.EndpointLimitModel>()
                    {
                        new RateLimiter.Models.EndpointLimitModel(){Endpoint = "/api/products/books", RequestLimitCount= 1, RequestLimitMs=1000 },
                        new RateLimiter.Models.EndpointLimitModel(){Endpoint = "/api/products/pencils", RequestLimitCount= 2, RequestLimitMs=500 },
                    }
                });

            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = rateLimiter.rateLimiterModel.RequestLimiterEnabled;
                options.StackBlockedRequests = false;
                options.HttpStatusCode = 429;
                options.RealIpHeader = "X-Real-IP";
                options.ClientIdHeader = "X-ClientId";
                options.GeneralRules = new List<RateLimitRule>();
                foreach (var EndpointLimit in rateLimiter.rateLimiterModel.EndpointLimits)
                {
                    options.GeneralRules.Add(new RateLimitRule()
                    {
                        Endpoint = EndpointLimit.Endpoint,
                        Period = EndpointLimit.RequestLimitMs.ToString() + "s",
                        Limit = EndpointLimit.RequestLimitCount
                    });
                }
            });
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            services.AddInMemoryRateLimiting();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi", Version = "v1" });
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseIpRateLimiting();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            var rateLimiterConfig = Configuration.GetSection("RateLimiter").Get<RateLimiterModel>();

            if (rateLimiterConfig.RequestLimiterEnabled)
            {
                app.UseMiddleware<RateLimiterMiddleware>(rateLimiterConfig);
            }

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi v1"));
        }
    }
}
