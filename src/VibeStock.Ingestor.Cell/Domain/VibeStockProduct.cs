using System.Collections.Generic;
using SaaSFoundry.SDK.ProductIntelligence.Models;

namespace VibeStock.Ingestor.Cell.Domain;

public class VibeStockProduct
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Inventory { get; set; }
    public List<string> Tags { get; set; } = new();

    public ProductIntelligenceReport? Intelligence { get; set; }
    public SeoFinding[]? SeoFindings { get; set; }
}
