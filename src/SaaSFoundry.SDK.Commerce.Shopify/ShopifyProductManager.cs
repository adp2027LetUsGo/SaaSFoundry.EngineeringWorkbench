using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.SDK.Commerce;
using SaaSFoundry.SDK.Commerce.Models;
using SaaSFoundry.SDK.Commerce.Shopify.Models;

namespace SaaSFoundry.SDK.Commerce.Shopify;

public sealed class ShopifyProductManager : ICommerceProductManager
{
    private readonly HttpClient _httpClient;

    public ShopifyProductManager(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CommerceResult<CommerceProduct>> CreateAsync(CommerceProduct product, CancellationToken cancellationToken = default)
    {
        var mutation = @"
            mutation productCreate($input: ProductInput!) {
                productCreate(input: $input) {
                    product { id }
                    userErrors { message }
                }
            }";
            
        var variables = new ProductCreateVariables { Input = new ProductCreateInput { Title = product.Title, Vendor = product.Vendor } };
        var reqObj = new GraphQLRequestProductCreate { Query = mutation, Variables = variables };
        
        var json = JsonSerializer.Serialize(reqObj, ShopifyJsonSerializerContext.Default.GraphQLRequestProductCreate);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("graphql.json", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errType = response.StatusCode == System.Net.HttpStatusCode.Unauthorized ? CommerceErrorType.AuthFailure :
                          response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ? CommerceErrorType.RateLimited :
                          CommerceErrorType.Transient;
                          
            return CommerceResult<CommerceProduct>.Failure(new CommerceError(errType, $"HTTP {response.StatusCode}"));
        }

        var respJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var graphResp = JsonSerializer.Deserialize(respJson, ShopifyJsonSerializerContext.Default.GraphQLResponseProductCreateData);

        if (graphResp?.Errors?.Any() == true)
        {
            return CommerceResult<CommerceProduct>.Failure(new CommerceError(CommerceErrorType.Validation, graphResp.Errors.First().Message));
        }

        if (graphResp?.Data?.ProductCreate?.UserErrors?.Any() == true)
        {
            return CommerceResult<CommerceProduct>.Failure(new CommerceError(CommerceErrorType.Validation, graphResp.Data.ProductCreate.UserErrors.First().Message));
        }

        if (graphResp?.Data?.ProductCreate?.Product == null)
        {
            return CommerceResult<CommerceProduct>.Failure(new CommerceError(CommerceErrorType.Permanent, "Missing product payload."));
        }

        product.ExternalId = graphResp.Data.ProductCreate.Product.Id;
        return CommerceResult<CommerceProduct>.Success(product);
    }

    public Task<CommerceResult<CommerceProduct>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CommerceResult<CommerceProduct>.Failure(new CommerceError(CommerceErrorType.NotFound, "Not implemented yet")));
    }
}
