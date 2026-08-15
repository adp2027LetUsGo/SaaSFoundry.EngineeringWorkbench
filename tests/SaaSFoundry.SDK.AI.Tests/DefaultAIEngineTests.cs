using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SaaSFoundry.SDK.AI.Engine;
using SaaSFoundry.SDK.AI.Models;
using SaaSFoundry.SDK.AI.Validation;

namespace SaaSFoundry.SDK.AI.Tests;

public class TestProvider : IAIProvider
{
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;
    public bool ThrowError { get; set; } = false;
    public string ErrorMessage { get; set; } = "Provider failed";

    public async Task<AIResponse<TOutput>> InvokeAsync<TInput, TOutput>(AIRequest<TInput> request, CancellationToken cancellationToken)
    {
        if (Delay > TimeSpan.Zero)
            await Task.Delay(Delay, cancellationToken);
            
        if (ThrowError)
            throw new Exception(ErrorMessage);

        return new AIResponse<TOutput>(default!, AIConfidence.High, "Tested");
    }
}

public class TestValidator : IAIValidator<string>
{
    public bool Valid { get; set; } = true;
    public string Message { get; set; } = "Validation Error";

    public ValidationResult Validate(string output) => Valid ? ValidationResult.Success() : ValidationResult.Failure(Message);
}

public class DefaultAIEngineTests
{
    [Fact]
    public async Task ExecuteAsync_WhenNoProvider_ReturnsUnavailable()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAIEngine, DefaultAIEngine>();
        var sp = services.BuildServiceProvider();

        var engine = sp.GetRequiredService<IAIEngine>();
        var result = await engine.ExecuteAsync<string, string>(new AIRequest<string>("Input", "Test", TimeSpan.FromSeconds(5)));

        Assert.Equal(AIResultStatus.Unavailable, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_Timeout_ReturnsTimeoutStatus()
    {
        var provider = new TestProvider { Delay = TimeSpan.FromSeconds(5) };
        var services = new ServiceCollection();
        services.AddSingleton<IAIEngine, DefaultAIEngine>();
        services.AddSingleton<IAIProvider>(provider);
        var sp = services.BuildServiceProvider();

        var engine = sp.GetRequiredService<IAIEngine>();
        var result = await engine.ExecuteAsync<string, string>(new AIRequest<string>("Input", "Test", TimeSpan.FromMilliseconds(50)));

        Assert.Equal(AIResultStatus.Timeout, result.Status);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task ExecuteAsync_ProviderThrows_ReturnsProviderError()
    {
        var provider = new TestProvider { ThrowError = true };
        var services = new ServiceCollection();
        services.AddSingleton<IAIEngine, DefaultAIEngine>();
        services.AddSingleton<IAIProvider>(provider);
        var sp = services.BuildServiceProvider();

        var engine = sp.GetRequiredService<IAIEngine>();
        var result = await engine.ExecuteAsync<string, string>(new AIRequest<string>("Input", "Test", TimeSpan.FromSeconds(5)));

        Assert.Equal(AIResultStatus.ProviderError, result.Status);
        Assert.Equal("Provider failed", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFails_ReturnsValidationFailed()
    {
        var provider = new TestProvider();
        var validator = new TestValidator { Valid = false, Message = "Bad format" };
        var services = new ServiceCollection();
        services.AddSingleton<IAIEngine, DefaultAIEngine>();
        services.AddSingleton<IAIProvider>(provider);
        services.AddSingleton<IAIValidator<string>>(validator);
        var sp = services.BuildServiceProvider();

        var engine = sp.GetRequiredService<IAIEngine>();
        var result = await engine.ExecuteAsync<string, string>(new AIRequest<string>("Input", "Test", TimeSpan.FromSeconds(5)));

        Assert.Equal(AIResultStatus.ValidationFailed, result.Status);
        Assert.Equal("Bad format", result.ErrorMessage);
    }
    
    [Fact]
    public async Task ExecuteAsync_Success_ReturnsSuccess()
    {
        var provider = new TestProvider();
        var services = new ServiceCollection();
        services.AddSingleton<IAIEngine, DefaultAIEngine>();
        services.AddSingleton<IAIProvider>(provider);
        var sp = services.BuildServiceProvider();

        var engine = sp.GetRequiredService<IAIEngine>();
        var result = await engine.ExecuteAsync<string, string>(new AIRequest<string>("Input", "Test", TimeSpan.FromSeconds(5)));

        Assert.Equal(AIResultStatus.Success, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal(AIConfidence.High, result.Response.Confidence);
    }
}
