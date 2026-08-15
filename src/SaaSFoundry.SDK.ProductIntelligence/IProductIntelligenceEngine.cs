using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.SDK.ProductIntelligence.Models;

namespace SaaSFoundry.SDK.ProductIntelligence;

public interface IProductIntelligenceEngine
{
    Task<ProductIntelligenceReport> AnalyzeAsync(ProductIntelligenceRequest request, CancellationToken cancellationToken = default);
}
