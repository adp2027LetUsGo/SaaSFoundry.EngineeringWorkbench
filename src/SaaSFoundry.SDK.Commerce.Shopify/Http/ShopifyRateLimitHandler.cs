using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.SDK.Commerce.Shopify.Http;

public sealed class ShopifyRateLimitHandler : DelegatingHandler
{
    private static readonly ActivitySource ActivitySource = new ActivitySource("SaaSFoundry.SDK.Commerce.Shopify");
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;

    public ShopifyRateLimitHandler(int maxRetries = 3, int baseDelayMs = 1000)
    {
        _maxRetries = maxRetries;
        _baseDelay = TimeSpan.FromMilliseconds(baseDelayMs);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        int retries = 0;

        while (true)
        {
            HttpResponseMessage? response = null;
            Exception? caughtException = null;

            try
            {
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                caughtException = ex;
            }

            bool shouldRetry = false;
            TimeSpan delay = _baseDelay * Math.Pow(2, retries);

            if (response != null)
            {
                if ((int)response.StatusCode == 429)
                {
                    shouldRetry = true;
                }
                else if ((int)response.StatusCode >= 500)
                {
                    shouldRetry = true;
                }
                else
                {
                    return response; // Success or 4xx
                }
            }
            else if (caughtException != null)
            {
                shouldRetry = true; // Network transient error
            }

            if (!shouldRetry || retries >= _maxRetries)
            {
                if (caughtException != null) throw caughtException;
                return response!;
            }

            using var activity = ActivitySource.StartActivity("ShopifyRateLimitRetry");
            activity?.SetTag("retry.count", retries + 1);
            
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            retries++;
        }
    }
}
