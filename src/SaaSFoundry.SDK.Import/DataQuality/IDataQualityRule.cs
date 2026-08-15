using System.Threading.Tasks;
using SaaSFoundry.SDK.Import.Models;

namespace SaaSFoundry.SDK.Import.DataQuality;

public interface IDataQualityRule<T>
{
    ValueTask EvaluateAsync(ImportRecord<T> record);
}
