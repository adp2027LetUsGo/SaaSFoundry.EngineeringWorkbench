using System;
using System.Collections.Generic;

namespace SaaSFoundry.SDK.Testing.Results;

public class TestVerificationResult
{
    public bool IsSuccess { get; }
    public IReadOnlyList<string> Messages { get; }

    public TestVerificationResult(bool isSuccess, IReadOnlyList<string> messages)
    {
        IsSuccess = isSuccess;
        Messages = messages ?? Array.Empty<string>();
    }

    public static TestVerificationResult Success() => new TestVerificationResult(true, Array.Empty<string>());
    public static TestVerificationResult Failure(string message) => new TestVerificationResult(false, new[] { message });
}
