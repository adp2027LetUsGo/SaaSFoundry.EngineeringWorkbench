using System;
using SaaSFoundry.SDK.Core.Results;
using Xunit;

namespace SaaSFoundry.SDK.Core.UnitTests;

public class ResultTests
{
    [Fact]
    public void Result_Success_IsSuccessTrue()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Result_Failure_IsSuccessFalse()
    {
        var result = Result.Failure("error");
        Assert.False(result.IsSuccess);
        Assert.Equal("error", result.ErrorMessage);
    }

    [Fact]
    public void ResultT_Success_IsSuccessTrue()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ResultT_Failure_IsSuccessFalse()
    {
        var result = Result<int>.Failure("error");
        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.Equal("error", result.ErrorMessage);
    }

    [Fact]
    public void ValidationResult_Success_NoErrors()
    {
        var result = ValidationResult.Success();
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Failure_HasErrors()
    {
        var error = new ValidationError("CODE1", "Message");
        var result = ValidationResult.Failure(new[] { error });
        
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("CODE1", result.Errors[0].Code);
    }
}
