using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SaaSFoundry.SDK.Commerce.Shopify.Models;

public class GraphQLRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;
    
    [JsonPropertyName("variables")]
    public object? Variables { get; set; }
}

public class GraphQLResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<GraphQLError>? Errors { get; set; }

    [JsonPropertyName("extensions")]
    public GraphQLExtensions? Extensions { get; set; }
}

public class GraphQLError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class GraphQLExtensions
{
    [JsonPropertyName("cost")]
    public GraphQLCost? Cost { get; set; }
}

public class GraphQLCost
{
    [JsonPropertyName("throttleStatus")]
    public GraphQLThrottleStatus? ThrottleStatus { get; set; }
}

public class GraphQLThrottleStatus
{
    [JsonPropertyName("maximumAvailable")]
    public double MaximumAvailable { get; set; }
    
    [JsonPropertyName("currentlyAvailable")]
    public double CurrentlyAvailable { get; set; }
    
    [JsonPropertyName("restoreRate")]
    public double RestoreRate { get; set; }
}

public class ProductCreateData
{
    [JsonPropertyName("productCreate")]
    public ProductCreatePayload? ProductCreate { get; set; }
}

public class ProductCreatePayload
{
    [JsonPropertyName("product")]
    public ShopifyProductNode? Product { get; set; }

    [JsonPropertyName("userErrors")]
    public List<ShopifyUserError>? UserErrors { get; set; }
}

public class ShopifyProductNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class ShopifyUserError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class ProductCreateVariables
{
    [JsonPropertyName("input")]
    public ProductCreateInput? Input { get; set; }
}

public class ProductCreateInput
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("vendor")]
    public string Vendor { get; set; } = string.Empty;
}

public class GraphQLRequestProductCreate
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;
    
    [JsonPropertyName("variables")]
    public ProductCreateVariables? Variables { get; set; }
}

[JsonSerializable(typeof(GraphQLRequestProductCreate))]
[JsonSerializable(typeof(GraphQLRequest))]
[JsonSerializable(typeof(GraphQLResponse<ProductCreateData>))]
[JsonSerializable(typeof(object))]
public partial class ShopifyJsonSerializerContext : JsonSerializerContext { }
