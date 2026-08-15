using System;
using System.Net.Http;
using SaaSFoundry.SDK.Commerce;
using SaaSFoundry.SDK.Commerce.Models;
using SaaSFoundry.SDK.Commerce.Shopify;
using SaaSFoundry.SDK.ProductIntelligence;
using SaaSFoundry.SDK.ProductIntelligence.Models;

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("Commerce AOT Validator Running...");
        var client = new HttpClient();
        var manager = new ShopifyProductManager(client);
        // Do not actually call it without API keys, just verify linking and AOT compatibility
        Console.WriteLine(manager.GetType().Name);
        var _ = ProductIntelligenceJsonSerializerContext.Default.AIIntelligenceExtractionResult;
    }
}
