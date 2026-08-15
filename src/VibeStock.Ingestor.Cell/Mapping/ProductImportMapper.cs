using System;
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
