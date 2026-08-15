import os

p3 = r'tests\VibeStock.System.Cell.IntegrationTests\VibeStockEndToEndCommerceFlowTests.cs'

with open(p3, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Update ExecutePromptAsync signature to ExecuteAsync
content = content.replace('public Task<AIResult<T>> ExecutePromptAsync<T>(string prompt, CancellationToken cancellationToken = default)', 'public Task<AIResult<TOutput>> ExecuteAsync<TInput, TOutput>(AIRequest<TInput> request, CancellationToken cancellationToken = default)')

# 2. Update Success calls
content = content.replace('AIResult<T>.Success((T)(object)mockResult)', 'AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)mockResult, AIConfidence.High, "mocked"))')
content = content.replace('AIResult<T>.Success((T)(object)mockConceptResult)', 'AIResult<TOutput>.Success(new AIResponse<TOutput>((TOutput)(object)mockConceptResult, AIConfidence.High, "mocked"))')

# 3. Update Failure calls
content = content.replace('AIResult<T>.Failure("Simulated AI Failure")', 'AIResult<TOutput>.Failure(AIResultStatus.TransientFailure, "Simulated AI Failure")')

with open(p3, 'w', encoding='utf-8') as f:
    f.write(content)

