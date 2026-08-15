using global::System;
using global::System.Net.Http;
using global::System.Security.Cryptography;
using global::System.Security.Cryptography.X509Certificates;
using global::System.Threading;
using global::System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Xunit;
using Grpc.Net.Client;
using Testcontainers.PostgreSql;
using SaaSFoundry.Transport.Generated;
using SaaSFoundry.Persistence.Idempotency;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;

namespace VibeStock.System.Cell.IntegrationTests;

public class GrpcTransportIntegrationTests : IAsyncLifetime
{
    private IHost _host = null!;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder().WithImage("postgres:16-alpine").Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var serverCert = GenerateCert("CN=Server", "spiffe://saasfoundry/vibestock/system.cell");

        _host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                    options.ListenLocalhost(5001, listenOptions =>
                    {
                        listenOptions.UseHttps(serverCert, httpsOptions =>
                        {
                            httpsOptions.ClientCertificateMode = Microsoft.AspNetCore.Server.Kestrel.Https.ClientCertificateMode.RequireCertificate;
                            httpsOptions.ClientCertificateValidation = (cert, chain, errors) => true; // We will do auth in middleware
                        });
                    });
                });

                webBuilder.ConfigureServices(services =>
                {
                    services.AddGrpc();
                    services.AddSingleton<IIdempotencyEnforcer>(new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5)));
                    
                    // We must manually add what the Factory adds in Program.cs
                    // builder.Services.AddGrpcTransport();
                    GrpcServiceCollectionExtensions.AddGrpcTransport(services);
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    
                    // The factory uses IStartupFilter to add MtlsAuthorizationMiddleware and MapGrpcService
                    // So we must execute the startup filters
                    var filters = app.ApplicationServices.GetServices<Microsoft.AspNetCore.Hosting.IStartupFilter>();
                    Action<IApplicationBuilder> pipeline = builder => 
                    {
                        builder.UseEndpoints(endpoints => {});
                    };
                    
                    foreach (var filter in filters)
                    {
                        pipeline = filter.Configure(pipeline);
                    }
                    
                    pipeline(app);
                });
            })
            .Build();

        await _host.StartAsync();
        
        // Init tables
        using var conn = new Npgsql.NpgsqlConnection(_dbContainer.GetConnectionString());
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS sys_idempotency_keys (
                tenant_id VARCHAR(100) NOT NULL,
                idempotency_key VARCHAR(100) NOT NULL,
                status VARCHAR(50) NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
                PRIMARY KEY (tenant_id, idempotency_key)
            );
        ";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task Test_Certification_Gate()
    {
        var validClientCert = GenerateCert("CN=Core", "spiffe://saasfoundry/vibestock/core.cell");
        var client = CreateGrpcClient(validClientCert);

        // 1. gRPC server startup + 2. generated client connection + 3. mTLS + 5. SPIFFE URI
        var pingRes = await client.PingAsync(new PingRequest());
        Assert.Equal("Ok", pingRes.CellIdentity);

        // 6. CN-only identity rejection
        var cnOnlyCert = GenerateCert("CN=Core", null);
        var cnClient = CreateGrpcClient(cnOnlyCert);
        await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () => await cnClient.PingAsync(new PingRequest()));

        // 11. Unknown cell
        var unknownCert = GenerateCert("CN=Rogue", "spiffe://saasfoundry/vibestock/rogue.cell");
        var unknownClient = CreateGrpcClient(unknownCert);
        await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () => await unknownClient.PingAsync(new PingRequest()));

        // 22. Idempotency execution
        var metadata = new Grpc.Core.Metadata();
        metadata.Add("x-idempotency-key", "test-key-1");
        var commandRes = await client.ProcessCommandAsync(new CommandRequest { Payload = "Test" }, headers: metadata);
        Assert.Equal("Processed: Test", commandRes.Result);
    }

    private SaaSFoundry.Transport.Generated.InfrastructureService.InfrastructureServiceClient CreateGrpcClient(X509Certificate2 clientCert)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(clientCert);
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        return new SaaSFoundry.Transport.Generated.InfrastructureService.InfrastructureServiceClient(channel);
    }

    private X509Certificate2 GenerateCert(string cn, string? spiffeUri)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(cn, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        if (spiffeUri != null)
        {
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddUri(new Uri(spiffeUri));
            req.CertificateExtensions.Add(sanBuilder.Build());
        }
        var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1));
        #pragma warning disable SYSLIB0057
        return new X509Certificate2(cert.Export(X509ContentType.Pfx));
        #pragma warning restore SYSLIB0057
    }
}
