using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.SDK.Commerce.Models;

namespace SaaSFoundry.SDK.Commerce;

public interface ICommerceProductManager
{
    Task<CommerceResult<CommerceProduct>> CreateAsync(CommerceProduct product, CancellationToken cancellationToken = default);
    Task<CommerceResult<CommerceProduct>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
}
