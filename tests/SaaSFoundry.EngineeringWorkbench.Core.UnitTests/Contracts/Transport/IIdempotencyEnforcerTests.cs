using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.Core.UnitTests.Contracts.Transport;

public class IIdempotencyEnforcerTests
{
    [Fact]
    public void IIdempotencyEnforcer_HasExpectedMethods()
    {
        var type = typeof(IIdempotencyEnforcer);
        Assert.True(type.IsInterface);

        var tryAcquireMethod = type.GetMethod("TryAcquireAsync");
        Assert.NotNull(tryAcquireMethod);
        Assert.Equal(typeof(Task<IdempotencyAcquisitionStatus>), tryAcquireMethod.ReturnType);
        var p1 = tryAcquireMethod.GetParameters();
        Assert.Equal(3, p1.Length);
        Assert.Equal(typeof(string), p1[0].ParameterType);
        Assert.Equal("tenantId", p1[0].Name);
        Assert.Equal(typeof(string), p1[1].ParameterType);
        Assert.Equal("idempotencyKey", p1[1].Name);
        Assert.Equal(typeof(CancellationToken), p1[2].ParameterType);

        var completeMethod = type.GetMethod("CompleteAsync");
        Assert.NotNull(completeMethod);
        Assert.Equal(typeof(Task), completeMethod.ReturnType);
        var p2 = completeMethod.GetParameters();
        Assert.Equal(3, p2.Length);
        Assert.Equal(typeof(string), p2[0].ParameterType);
        Assert.Equal("tenantId", p2[0].Name);
        Assert.Equal(typeof(string), p2[1].ParameterType);
        Assert.Equal("idempotencyKey", p2[1].Name);
        Assert.Equal(typeof(CancellationToken), p2[2].ParameterType);

        Assert.Null(type.GetMethod("IsAlreadyProcessedAsync"));
        Assert.Null(type.GetMethod("RecordAsProcessedAsync"));
    }

    [Fact]
    public void IdempotencyAcquisitionStatus_HasExpectedValues()
    {
        var enumType = typeof(IdempotencyAcquisitionStatus);
        Assert.True(enumType.IsEnum);
        var names = Enum.GetNames(enumType);
        Assert.Contains("Acquired", names);
        Assert.Contains("AlreadyProcessed", names);
        Assert.Contains("InProgress", names);
        Assert.Equal(3, names.Length);
    }
}
