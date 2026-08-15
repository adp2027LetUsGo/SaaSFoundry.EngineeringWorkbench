extern alias Ingestor;
using SaaSFoundry.SDK.AI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SaaSFoundry.SDK.Import;
using SaaSFoundry.SDK.Import.Mapping;

using SaaSFoundry.SDK.Import.Engine;
using SaaSFoundry.SDK.Import.AI.Engine;
using SaaSFoundry.SDK.Import.AI.Models.AI;

using SaaSFoundry.SDK.Import.Models;
using SaaSFoundry.SDK.Import.Parsers;
using SaaSFoundry.SDK.Import.AI;
using SaaSFoundry.SDK.Import.AI.Models;
using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.SDK.AI;
using SaaSFoundry.SDK.Commerce;
using SaaSFoundry.SDK.Commerce.Models;
using SaaSFoundry.SDK.Commerce.Shopify;
using SaaSFoundry.SDK.Commerce.Shopify.Http;
using Ingestor::VibeStock.Ingestor.Cell.Domain;
using Ingestor::VibeStock.Ingestor.Cell.Mapping;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;

namespace VibeStock.System.Cell.IntegrationTests;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
public class VibeStockEndToEndCommerceFlowTests
{
    // =========================================================================
    // MOCKS & STUBS (Simulating Boundaries & external systems)
    // =========================================================================

    public class MockAIEngine : IAIEngine
    {
        public bool SimulateFailure { get; set; }

        public Task<AIResult<TOutput>> ExecuteAsync<TInput, TOutput>(AIRequest<TInput> request, CancellationToken cancellationToken = default)
        {
            if (SimulateFailure)
                return Task.FromResult(AIResult<TOutput>.Failure(AIResultStatus.Unavailable, "AI Offline"));

            // Mocking semantic understanding for column aliases
            if (typeof(TOutput) == typeof(SemanticMappingProviderResponse))
            {
                var suggestions = new List<ProviderSuggestedMapping>
                {
                    new ProviderSuggestedMapping { SourceColumn = "Coste", TargetField = "Price", Confidence = "High" },
                    new ProviderSuggestedMapping { SourceColumn = "Referencia", TargetField = "Sku", Confidence = "High" },
                    new ProviderSuggestedMapping { SourceColumn = "Cant", TargetField = "Inventory", Confidence = "High" }
                };
                return Task.FromResult(AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)new SemanticMappingProviderResponse { Suggestions = suggestions }, AIConfidence.High, "mocked")));
            }
                          if (typeof(TOutput) == typeof(string))
              {
                  var json = @"{
                      ""ExtractedFeatures"": [""mock""],
                      ""TargetAudience"": ""General"",
                      ""Tone"": ""Friendly"",
                      ""SemanticTags"": [""mock""],
                      ""ContentGaps"": [""Missing dimensions""],
                      ""Recommendations"": [""mock""]
                  }";
                  return Task.FromResult(AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)json, AIConfidence.High, "mocked")));
              }
              throw new NotImplementedException();
        }
    }

    public class MockIdempotencyEnforcer : IIdempotencyEnforcer
    {
        public HashSet<string> Keys = new();

        public Task<IdempotencyAcquisitionStatus> TryAcquireAsync(string tenantId, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            if (Keys.Contains(idempotencyKey)) return Task.FromResult(IdempotencyAcquisitionStatus.AlreadyProcessed);
            return Task.FromResult(IdempotencyAcquisitionStatus.Acquired);
        }

        public Task CompleteAsync(string tenantId, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            Keys.Add(idempotencyKey);
            return Task.CompletedTask;
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public bool SimulateRateLimit { get; set; }
        public bool Simulate500 { get; set; }
        public string? CapturePayload { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            if (request.Content != null)
                CapturePayload = await request.Content.ReadAsStringAsync(cancellationToken);

            if (SimulateRateLimit && CallCount < 3)
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests);

            if (Simulate500 && CallCount < 3)
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);

            var mockResponse = @"
            {
                ""data"": {
                    ""productCreate"": {
                        ""product"": { ""id"": ""gid://shopify/Product/999"" },
                        ""userErrors"": []
                    }
                }
            }";
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(mockResponse) };
        }
    }

    // =========================================================================
    // PIPELINE ORCHESTRATOR 
    // =========================================================================


    private class MockImportMapper : SaaSFoundry.SDK.Import.Mapping.IImportMapper<VibeStockProduct>
    {
        public VibeStockProduct Map(IReadOnlyDictionary<string, string> row)
        {
            row.TryGetValue("Name", out var name);
            if (name == null) row.TryGetValue("Nombre", out name);
            
            row.TryGetValue("Sku", out var sku);
            if (sku == null) row.TryGetValue("Referencia", out sku);
            
            row.TryGetValue("Price", out var priceStr);
            if (priceStr == null) row.TryGetValue("Precio", out priceStr);
            
            row.TryGetValue("Inventory", out var invStr);
            
            return new VibeStockProduct 
            { 
                Name = name ?? "", 
                Sku = sku ?? "", 
                Price = decimal.TryParse(priceStr, out var p) ? p : 0m,
                Inventory = int.TryParse(invStr, out var i) ? i : 0
            };
        }
    }

    private class EndToEndPipeline
    {
        public IImportEngine ImportEngine;
        public IAISemanticMapper SemanticMapper;
        public ICommerceProductManager CommerceManager;
        public SaaSFoundry.SDK.ProductIntelligence.IProductIntelligenceEngine ProductIntelligence;
        public IIdempotencyEnforcer IdempotencyEnforcer;
        public MockAIEngine AIEngine;
        public MockHttpMessageHandler HttpHandler;

        public EndToEndPipeline()
        {


            var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            serviceCollection.AddSingleton<SaaSFoundry.SDK.Import.Mapping.IImportMapper<VibeStockProduct>>(new MockImportMapper());
            ImportEngine = new DefaultImportEngine(serviceCollection.BuildServiceProvider());


            AIEngine = new MockAIEngine();
            SemanticMapper = new DefaultAISemanticMapper(new Microsoft.Extensions.DependencyInjection.ServiceCollection().AddSingleton<SaaSFoundry.SDK.AI.IAIEngine>(AIEngine).BuildServiceProvider()); 
            
            HttpHandler = new MockHttpMessageHandler();
            var rateLimitHandler = new ShopifyRateLimitHandler(maxRetries: 3, baseDelayMs: 1) { InnerHandler = HttpHandler };
            var httpClient = new HttpClient(rateLimitHandler) { BaseAddress = new Uri("https://test.myshopify.com") };
            
            CommerceManager = new ShopifyProductManager(httpClient);
            IdempotencyEnforcer = new MockIdempotencyEnforcer();

            ProductIntelligence = new SaaSFoundry.SDK.ProductIntelligence.DefaultProductIntelligenceEngine(AIEngine);

        }

        // Simulates Bridge.Cell consuming VibeStockProduct and pushing to Commerce SDK
        public async Task<List<CommerceResult<CommerceProduct>>> SyncToShopifyAsync(ImportResult<VibeStockProduct> importResult)
        {
            var results = new List<CommerceResult<CommerceProduct>>();
            foreach (var record in importResult.Rows.Where(r => r.Category == ImportCategory.Valid).Select(r => r.Data))
            {
                var idempotencyKey = "sync_" + record.Sku;
                
                // Idempotency Boundary
                var lockStatus = await IdempotencyEnforcer.TryAcquireAsync("tenant", idempotencyKey);
                if (lockStatus == IdempotencyAcquisitionStatus.AlreadyProcessed)
                {
                    results.Add(CommerceResult<CommerceProduct>.Failure(new CommerceError(CommerceErrorType.Conflict, "Duplicate")));
                    continue;
                }


                // Product Intelligence
                var piRequest = new SaaSFoundry.SDK.ProductIntelligence.Models.ProductIntelligenceRequest
                {
                    Sku = record.Sku,
                    Title = record.Name,
                    Description = record.Description,
                    Tags = record.Tags
                };
                var piReport = await ProductIntelligence.AnalyzeAsync(piRequest, default);
                record.Intelligence = piReport;
                
                // SEO is part of PI Report
                record.SeoFindings = record.Intelligence?.SeoFindings.ToArray();

                // Domain Map: VibeStock -> Commerce
                var cProduct = new CommerceProduct 
                {
                    Title = record.Name,
                    Description = record.Intelligence.ContentGaps.Count == 0 ? record.Description : record.Description + " [Enhanced]",
                    Vendor = "VibeStock",
                    Variants = new List<CommerceVariant> 
                    { 
                        new CommerceVariant { Sku = record.Sku, Price = record.Price, InventoryQuantity = record.Inventory } 
                    }
                };

                // Shopify Push
                var result = await CommerceManager.CreateAsync(cProduct);
                if (result.IsSuccess)
                {
                    await IdempotencyEnforcer.CompleteAsync("tenant", idempotencyKey);
                }
                
                results.Add(result);
            }
            return results;
        }
    }

    // =========================================================================
    // THE MATRIX TESTS
    // =========================================================================

    [Fact]
    public async Task A_CsvHappyPath_SuccessfullySynchronizes()
    {
        var pipeline = new EndToEndPipeline();
        
        // 1. Raw Input
        string csv = "Referencia,Name,Coste,Cant\nSKU-100,Test Product,19.99,150";
        using var stream = new MemoryStream(global::System.Text.Encoding.UTF8.GetBytes(csv));
        
        // 2. Import Inspect (Phase 1)
        var schema = await pipeline.ImportEngine.InspectAsync(stream, ImportFormat.Csv);
        Assert.Equal(4, schema.Columns.Count); // Referencia, Name, Coste, Cant
        
        // 3. AI Mapping (Phase 2 - Test D & C)
        var semanticResult = await pipeline.SemanticMapper.SuggestMappingsAsync(schema, new[] { "Name", "Price", "Sku", "Inventory" }, null);
        
        // 'Name' is deterministic. Referencia, Coste, Cant are AI resolved.
        var mappings = semanticResult.Mappings.ToList();
        var nameMap = mappings.First(m => m.TargetField == "Name");
        var priceMap = mappings.First(m => m.TargetField == "Price");
        
        Assert.Equal(MappingStatus.Confirmed, nameMap.Status); // Deterministic bypass
        Assert.Equal(MappingStatus.Suggested, priceMap.Status); // AI Suggested
        
        // 4. Human Approval (Test E)
        foreach(var map in semanticResult.Mappings) map.Status = MappingStatus.Confirmed;
        var config = semanticResult.ToConfiguration();
        
        // 5. Data Quality (Phase 3 - Test G)
        stream.Position = 0; // reset
        var result = await pipeline.ImportEngine.ProcessAsync<VibeStockProduct>(stream, ImportFormat.Csv, config);
        
        Assert.Single(result.Rows.Where(r => r.Category == SaaSFoundry.SDK.Import.Models.ImportCategory.Valid).Select(r => r.Data).ToList());
        Assert.Equal("SKU-100", result.Rows.Where(r => r.Category == SaaSFoundry.SDK.Import.Models.ImportCategory.Valid).Select(r => r.Data).ToList()[0].Sku);
        
        // 6. Commerce/Shopify (Phase 4 & 5)
        var syncResults = await pipeline.SyncToShopifyAsync(result);
        
        Assert.Single(syncResults);
        Assert.True(syncResults[0].IsSuccess);
        Assert.Equal("gid://shopify/Product/999", syncResults[0].Data!.ExternalId);
        
        // 7. Security (Test K)
        Assert.DoesNotContain("AccessToken", pipeline.HttpHandler.CapturePayload ?? "");
    }

    [Fact]
    public async Task F_AiFailure_FallsBackToManual()
    {
        var pipeline = new EndToEndPipeline();
        pipeline.AIEngine.SimulateFailure = true;
        
        string csv = "Referencia,Name\nSKU-100,Test Product";
        using var stream = new MemoryStream(global::System.Text.Encoding.UTF8.GetBytes(csv));
        var schema = await pipeline.ImportEngine.InspectAsync(stream, ImportFormat.Csv);
        
        var semanticResult = await pipeline.SemanticMapper.SuggestMappingsAsync(schema, new[] { "Name", "Price", "Sku", "Inventory" }, null);
        
        var refMap = semanticResult.Mappings.First(m => m.SourceColumn == "Referencia");
        Assert.Equal(MappingStatus.Unresolved, refMap.Status); // Failed AI -> manual fallback
    }

    [Fact]
    public async Task H_ShopifyRateLimiting_AppliesBoundedRetry()
    {
        var pipeline = new EndToEndPipeline();
        pipeline.HttpHandler.SimulateRateLimit = true;

        var result = new ImportResult<VibeStockProduct>();
        result.Rows.Add(new ImportRecord<VibeStockProduct> { Data = new VibeStockProduct { Name = "A", Price = 10, Sku = "SKU-A" }, Category = ImportCategory.Valid });
        result.Rows.Add(new ImportRecord<VibeStockProduct> { Data = new VibeStockProduct { Name = "B", Price = 20, Sku = "SKU-B" }, Category = ImportCategory.Valid });

        var syncResults = await pipeline.SyncToShopifyAsync(result);

        Assert.True(syncResults[0].IsSuccess);
        Assert.Equal(4, pipeline.HttpHandler.CallCount); // 2 fails (429) + 1 success for A + 1 success for B
    }

    [Fact]
    public async Task J_Idempotency_PreventsDuplicateGraphQLMutations()
    {
        var pipeline = new EndToEndPipeline();

        var result = new ImportResult<VibeStockProduct>();
        result.Rows.Add(new ImportRecord<VibeStockProduct> { Data = new VibeStockProduct { Name = "A", Price = 10, Sku = "SKU-A" }, Category = ImportCategory.Valid });
        result.Rows.Add(new ImportRecord<VibeStockProduct> { Data = new VibeStockProduct { Name = "B", Price = 20, Sku = "SKU-B" }, Category = ImportCategory.Valid });

        var sync1 = await pipeline.SyncToShopifyAsync(result);
        Assert.True(sync1[0].IsSuccess);
        Assert.Equal(2, pipeline.HttpHandler.CallCount);

        // Run same records again!
        var sync2 = await pipeline.SyncToShopifyAsync(result);
        Assert.False(sync2[0].IsSuccess); // Duplicate
        Assert.Equal(CommerceErrorType.Conflict, sync2[0].Errors[0].Type);
        
        // Ensure handler was NEVER called again
        Assert.Equal(2, pipeline.HttpHandler.CallCount);
    }
}
#pragma warning restore CS1998
