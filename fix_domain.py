import os

ingestor_dir = r"C:\Users\armando\Documents\_AHS\projects\Utilitarios\SaaSFoundry.EngineeringWorkbench\src\VibeStock.Ingestor.Cell"

def create_file(path, content):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w") as f:
        f.write(content)

vibestock_product = """using System.Collections.Generic;
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
"""
create_file(os.path.join(ingestor_dir, "Domain", "VibeStockProduct.cs"), vibestock_product)

product_mapper = """using System;
using System.Collections.Generic;
using SaaSFoundry.SDK.Import.Mapping;
using VibeStock.Ingestor.Cell.Domain;

namespace VibeStock.Ingestor.Cell.Mapping;

public class ProductImportMapper : IImportMapper<VibeStockProduct>
{
    public VibeStockProduct Map(IReadOnlyDictionary<string, string> row)
    {
        var product = new VibeStockProduct();

        if (row.TryGetValue("SKU", out var sku) || row.TryGetValue("sku", out sku))
            product.Sku = sku ?? string.Empty;

        if (row.TryGetValue("Name", out var name) || row.TryGetValue("name", out name))
            product.Name = name ?? string.Empty;

        if (row.TryGetValue("Description", out var desc) || row.TryGetValue("description", out desc))
            product.Description = desc ?? string.Empty;

        if (row.TryGetValue("Tags", out var tags) || row.TryGetValue("tags", out tags))
        {
            if (!string.IsNullOrWhiteSpace(tags))
                product.Tags.AddRange(tags.Split(','));
        }

        if (row.TryGetValue("Price", out var priceStr) || row.TryGetValue("price", out priceStr))
        {
            if (decimal.TryParse(priceStr, out var price))
                product.Price = price;
        }

        if (row.TryGetValue("Inventory", out var invStr) || row.TryGetValue("inventory", out invStr) || row.TryGetValue("Qty", out invStr))
        {
            if (int.TryParse(invStr, out var inv))
                product.Inventory = inv;
        }

        return product;
    }
}
"""
create_file(os.path.join(ingestor_dir, "Mapping", "ProductImportMapper.cs"), product_mapper)

print("Domain fixed.")
