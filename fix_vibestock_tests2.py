import os

test_path = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix 1: Create a mock mapper and register it
new_engine_init = """
            var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            serviceCollection.AddSingleton<SaaSFoundry.SDK.Import.Mapping.IImportMapper<VibeStockProduct>>(new MockImportMapper());
            ImportEngine = new DefaultImportEngine(serviceCollection.BuildServiceProvider());
"""
content = content.replace('            var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();\n            serviceCollection.AddSingleton<SaaSFoundry.SDK.Import.Mapping.IImportMapper<VibeStockProduct>>(new SaaSFoundry.SDK.Import.Mapping.DefaultImportMapper<VibeStockProduct>());\n            ImportEngine = new DefaultImportEngine(serviceCollection.BuildServiceProvider());', new_engine_init)

mock_mapper = """
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

content = content.replace('    private class EndToEndPipeline', mock_mapper + '\n    private class EndToEndPipeline')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
