using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SaaSFoundry.SDK.Commerce.Models;
using SaaSFoundry.SDK.Commerce.Shopify;
using SaaSFoundry.SDK.Commerce.Shopify.Http;

namespace SaaSFoundry.SDK.Commerce.Shopify.Tests;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
    public int CallCount { get; private set; }

    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(_handler(request));
    }
}

public class ShopifyProductManagerTests
{
    [Fact]
    public async Task CreateAsync_SuccessfulResponse_ReturnsProductWithId()
    {
        var mockResponse = @"
        {
            ""data"": {
                ""productCreate"": {
                    ""product"": { ""id"": ""gid://shopify/Product/123"" },
                    ""userErrors"": []
                }
            }
        }";
        
        var handler = new MockHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK) 
        { 
            Content = new StringContent(mockResponse) 
        });
        
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://test.myshopify.com") };
        var manager = new ShopifyProductManager(client);

        var product = new CommerceProduct { Title = "Test", Vendor = "Vendor" };
        var result = await manager.CreateAsync(product);

        Assert.True(result.IsSuccess);
        Assert.Equal("gid://shopify/Product/123", result.Data!.ExternalId);
    }

    [Fact]
    public async Task CreateAsync_UserErrors_ReturnsValidationFailure()
    {
        var mockResponse = @"
        {
            ""data"": {
                ""productCreate"": {
                    ""product"": null,
                    ""userErrors"": [ { ""message"": ""Title is required"" } ]
                }
            }
        }";
        
        var handler = new MockHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK) 
        { 
            Content = new StringContent(mockResponse) 
        });
        
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://test.myshopify.com") };
        var manager = new ShopifyProductManager(client);

        var product = new CommerceProduct { Title = "", Vendor = "Vendor" };
        var result = await manager.CreateAsync(product);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal(CommerceErrorType.Validation, result.Errors[0].Type);
        Assert.Equal("Title is required", result.Errors[0].Message);
    }

    [Fact]
    public async Task RateLimitHandler_RetriesOn429_AndSucceeds()
    {
        var mockSuccessResponse = @"
        {
            ""data"": {
                ""productCreate"": {
                    ""product"": { ""id"": ""gid://shopify/Product/123"" },
                    ""userErrors"": []
                }
            }
        }";

        int requestCount = 0;
        var handler = new MockHttpMessageHandler(req => 
        {
            requestCount++;
            if (requestCount < 3)
            {
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(mockSuccessResponse) };
        });

        // Use base delay of 1ms to speed up tests
        var rateLimitHandler = new ShopifyRateLimitHandler(maxRetries: 3, baseDelayMs: 1)
        {
            InnerHandler = handler
        };

        var client = new HttpClient(rateLimitHandler) { BaseAddress = new Uri("https://test.myshopify.com") };
        var manager = new ShopifyProductManager(client);

        var product = new CommerceProduct { Title = "Test", Vendor = "Vendor" };
        var result = await manager.CreateAsync(product);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, requestCount); // 2 failures + 1 success
    }
}
