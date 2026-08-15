import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix DefaultImportEngine
content = content.replace('new DefaultImportEngine()', 'new DefaultImportEngine(null!)')

# Fix malformed result 1 (Rate Limiting)
content = content.replace("""        var result = new ImportResult<VibeStockProduct>(),
            0
        );""", """        var result = new ImportResult<VibeStockProduct>();
        result.Rows.Add(new ImportRecord<VibeStockProduct> { Data = new VibeStockProduct { Title = "A", Price = 10, Sku = "SKU-A" }, Category = ImportCategory.Valid });
        result.Rows.Add(new ImportRecord<VibeStockProduct> { Data = new VibeStockProduct { Title = "B", Price = 20, Sku = "SKU-B" }, Category = ImportCategory.Valid });""")

# Fix malformed result 2 (Idempotency)
content = content.replace("""        var result = new ImportResult<VibeStockProduct>(),
            0
        );""", """        var result = new ImportResult<VibeStockProduct>();
        result.Rows.Add(new ImportRecord<VibeStockProduct> { Data = new VibeStockProduct { Title = "A", Price = 10, Sku = "SKU-A" }, Category = ImportCategory.Valid });
        result.Rows.Add(new ImportRecord<VibeStockProduct> { Data = new VibeStockProduct { Title = "B", Price = 20, Sku = "SKU-B" }, Category = ImportCategory.Valid });""")


with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
