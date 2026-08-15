using System;

namespace SaaSFoundry.SDK.Testing.Determinism;

public static class DeterminismVerifier
{
    public static void AssertDeterministic<TInput, TResult>(
        TInput input,
        Func<TInput, TResult> operation,
        Func<TResult, TResult, bool> equalityCheck)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));
            
        if (equalityCheck == null)
            throw new ArgumentNullException(nameof(equalityCheck));

        var result1 = operation(input);
        var result2 = operation(input);
        var result3 = operation(input);

        if (!equalityCheck(result1, result2) || !equalityCheck(result2, result3))
        {
            throw new InvalidOperationException("Operation did not produce deterministic results across multiple invocations.");
        }
    }
}
