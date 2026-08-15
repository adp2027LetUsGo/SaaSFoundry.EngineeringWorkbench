using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SaaSFoundry.Transport.Generated;

public static class GrpcServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcTransport(this IServiceCollection services)
    {
        services.AddGrpc();
        services.AddTransient<IStartupFilter, GrpcTransportStartupFilter>();
        return services;
    }
}

public class GrpcTransportStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseRouting();
            app.UseMiddleware<MtlsAuthorizationMiddleware>();
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapGrpcService<InfrastructureServiceImpl>();
            });
            next(app);
        };
    }
}