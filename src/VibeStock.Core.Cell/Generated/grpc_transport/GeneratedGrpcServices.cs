using System;
using System.Threading.Tasks;
using Grpc.Core;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;

namespace SaaSFoundry.Transport.Generated;

public class InfrastructureServiceImpl : InfrastructureService.InfrastructureServiceBase
{
    private readonly IIdempotencyEnforcer _idempotency;

    public InfrastructureServiceImpl(IIdempotencyEnforcer idempotency)
    {
        _idempotency = idempotency;
    }

    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PingResponse { CellIdentity = "Ok" });
    }

    public override async Task<CommandResponse> ProcessCommand(CommandRequest request, ServerCallContext context)
    {
        var idempotencyKey = context.RequestHeaders.Get("x-idempotency-key")?.Value ?? Guid.NewGuid().ToString();

        await _idempotency.TryAcquireAsync("system", idempotencyKey, context.CancellationToken);
        
        try
        {
            var res = new CommandResponse { Result = "Processed: " + request.Payload };
            await _idempotency.CompleteAsync("system", idempotencyKey, context.CancellationToken);
            return res;
        }
        catch
        {
            throw;
        }
    }
}