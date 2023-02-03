using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using RateLimiter.Models;
using RateLimiter.Middleware;

namespace RateLimiterTests
{
    public class RateLimiterMiddlewareTests
    {
        private TestServer _server;
        private HttpClient _client;
        [SetUp]
        public void Setup()
        {
            // I will prepare some default config for testing purposes
            RateLimiterModel rateLimiterConfiguration = new RateLimiterModel()
            {
                RequestLimiterEnabled = true,
                DefaultRequestLimitCount = 2,
                DefaultRequestLimitMs = 1000,
                EndpointLimits = new List<EndpointLimitModel>()
                {
                    new EndpointLimitModel()
                    {
                        Endpoint = "/api/products/books/",
                        RequestLimitCount = 3,
                        RequestLimitMs = 2000
                    },
                    new EndpointLimitModel()
                    {
                        Endpoint = "/api/products/pencils/",
                        RequestLimitCount = 5,
                        RequestLimitMs = 5000
                    }
                }
            };
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseMiddleware<RateLimiterMiddleware>(rateLimiterConfiguration);
                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync("RateLimiterTests!");
                    });
                }));
            _client = _server.CreateClient();
        }

        [Test]
        public async Task DefaultValueTest()
        {
            // Act
            var response1 = await _client.GetAsync("/");
            var response2 = await _client.GetAsync("/");
            var response3 = await _client.GetAsync("/");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response3.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            });

            // We will wait for cooldown and try to violete rules 2 times next time
            Thread.Sleep(2000);

            // Act
            var response4 = await _client.GetAsync("/");
            var response5 = await _client.GetAsync("/");
            var response6 = await _client.GetAsync("/");
            var response7 = await _client.GetAsync("/");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response4.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response5.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response6.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
                Assert.That(response7.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            });
        }
        [Test]
        public async Task EndpointValueTest()
        {
            // Act
            var response1 = await _client.GetAsync("/api/products/books/");
            var response2 = await _client.GetAsync("/api/products/books/");
            var response3 = await _client.GetAsync("/api/products/books/");
            var response4 = await _client.GetAsync("/api/products/books/");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response3.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response4.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            });

            // We will wait for cooldown and try to violete rules 2 times next time
            Thread.Sleep(3000);

            // Act
            var response5 = await _client.GetAsync("/api/products/books/");
            var response6 = await _client.GetAsync("/api/products/books/");
            var response7 = await _client.GetAsync("/api/products/books/");
            var response8 = await _client.GetAsync("/api/products/books/");
            var response9 = await _client.GetAsync("/api/products/books/");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response5.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response6.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response7.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response8.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
                Assert.That(response9.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            });
        }
        [Test]
        public async Task LoopingValueTest()
        {
            var responses = new Dictionary<int, HttpResponseMessage>();
            // Act
            for (int i = 0; i < 100; i++)
            {
                var response = await _client.GetAsync("/api/products/pencils/");
                responses.Add(i, response);
            }

            // Assert
            foreach(var response in responses)
            {
                if(response.Key < 5)
                    Assert.That(response.Value.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                else
                    Assert.That(response.Value.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            }

            // Cooling down and try again
            Thread.Sleep(6000);

            // Call again endpoint
            var responseAgain = await _client.GetAsync("/api/products/pencils");

            // It should get OK
            Assert.That(responseAgain.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}