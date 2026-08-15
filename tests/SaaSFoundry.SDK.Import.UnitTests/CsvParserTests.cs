using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.SDK.Import.Parsers;
using Xunit;

namespace SaaSFoundry.SDK.Import.UnitTests;

public class CsvParserTests
{
    [Fact]
    public async Task ParseAsync_ReadsHeadersAndRowsCorrectly()
    {
        var csv = "Id,Name,Price\n1,Test Product,9.99\n2,\"Quoted, Name\",10.50";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var parser = new CsvParser();
        
        var rows = await parser.ParseAsync(stream, CancellationToken.None).ToListAsync();
        
        Assert.Equal(3, parser.Columns.Count);
        Assert.Equal("Id", parser.Columns[0]);
        Assert.Equal("Name", parser.Columns[1]);
        
        Assert.Equal(2, rows.Count);
        Assert.Equal("1", rows[0]["Id"]);
        Assert.Equal("Test Product", rows[0]["Name"]);
        Assert.Equal("9.99", rows[0]["Price"]);
        
        Assert.Equal("2", rows[1]["Id"]);
        Assert.Equal("Quoted, Name", rows[1]["Name"]);
        Assert.Equal("10.50", rows[1]["Price"]);
    }
}
