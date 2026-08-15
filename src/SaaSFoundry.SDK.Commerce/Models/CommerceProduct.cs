using System.Collections.Generic;

namespace SaaSFoundry.SDK.Commerce.Models;

public sealed class CommerceProduct
{
    public string? ExternalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public IReadOnlyList<CommerceVariant> Variants { get; set; } = new List<CommerceVariant>();
}

public sealed class CommerceVariant
{
    public string? ExternalId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int InventoryQuantity { get; set; }
}
