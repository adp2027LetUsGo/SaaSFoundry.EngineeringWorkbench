using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Planning;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;

namespace SaaSFoundry.EngineeringWorkbench.Planning;

public sealed class EngineeringPlanner : IEngineeringPlanner
{
    private readonly EngineeringCatalog _catalog;

    public EngineeringPlanner(EngineeringCatalog catalog)
    {
        _catalog = catalog;
    }

    public PlanningResult CreatePlan(EngineeringPlanningContext context)
    {
        return RuleEngine.CalculateExecutionPlan(context, _catalog);
    }
}
