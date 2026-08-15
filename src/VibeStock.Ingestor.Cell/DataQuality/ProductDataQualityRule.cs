using System.Collections.Generic;
using System.Threading.Tasks;
using SaaSFoundry.SDK.Import.Engine;
using SaaSFoundry.SDK.Import.Models;
using SaaSFoundry.SDK.Import.DataQuality;
using VibeStock.Ingestor.Cell.Domain;

namespace VibeStock.Ingestor.Cell.DataQuality;

public class ProductDataQualityRule : IDataQualityRule<VibeStockProduct>
{
    public ValueTask EvaluateAsync(ImportRecord<VibeStockProduct> record)
    {
        var product = record.Data;
        if (product == null) return ValueTask.CompletedTask;

        if (string.IsNullOrWhiteSpace(product.Sku))
        {
            record.Diagnostics.Add(new ImportDiagnostic { Category = ImportCategory.Invalid, Message = "SKU is required.", Field = "Sku" });
            record.Category = ImportCategory.Invalid;
        }

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            record.Diagnostics.Add(new ImportDiagnostic { Category = ImportCategory.Invalid, Message = "Name is required.", Field = "Name" });
            record.Category = ImportCategory.Invalid;
        }

        if (product.Price <= 0)
        {
            record.Diagnostics.Add(new ImportDiagnostic { Category = ImportCategory.Invalid, Message = $"Price must be greater than 0. Current: {product.Price}", Field = "Price" });
            record.Category = ImportCategory.Invalid;
        }

        if (product.Inventory < 0)
        {
            record.Diagnostics.Add(new ImportDiagnostic { Category = ImportCategory.Invalid, Message = $"Inventory cannot be negative. Current: {product.Inventory}", Field = "Inventory" });
            record.Category = ImportCategory.Invalid;
        }
        else if (product.Inventory == 0)
        {
            record.Diagnostics.Add(new ImportDiagnostic { Category = ImportCategory.Warning, Message = "Inventory is exactly 0. Product will be imported as out of stock.", Field = "Inventory" });
            // Do not invalidate for warning
        }

        return ValueTask.CompletedTask;
    }
}
