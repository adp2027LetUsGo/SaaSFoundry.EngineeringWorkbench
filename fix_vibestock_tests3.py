import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

mock_mapper_old = """
    private class MockImportMapper : SaaSFoundry.SDK.Import.Mapping.IImportMapper<VibeStockProduct>
    {
        public VibeStockProduct Map(SaaSFoundry.SDK.Import.Models.ImportRow row, SaaSFoundry.SDK.Import.Models.ColumnMappingConfiguration? configuration)
        {
            return new VibeStockProduct 
            { 
                Name = row.Cells.FirstOrDefault(c => c.ColumnName == "Name" || c.ColumnName == "Nombre")?.Value?.ToString() ?? "", 
                Sku = row.Cells.FirstOrDefault(c => c.ColumnName == "Sku" || c.ColumnName == "Referencia")?.Value?.ToString() ?? "", 
                Price = decimal.TryParse(row.Cells.FirstOrDefault(c => c.ColumnName == "Price" || c.ColumnName == "Precio")?.Value?.ToString(), out var p) ? p : 0m,
                Inventory = int.TryParse(row.Cells.FirstOrDefault(c => c.ColumnName == "Inventory")?.Value?.ToString(), out var i) ? i : 0
            };
        }
    }
"""

mock_mapper_new = """
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
"""

content = content.replace(mock_mapper_old, mock_mapper_new)

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
