using System;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;
using SaaSFoundry.EngineeringWorkbench.UI.Services;

namespace SaaSFoundry.EngineeringWorkbench.UI;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var host = new WorkbenchHost(new EmptyServiceProvider());
        var router = new CommandRouter(host);

        if (args.Length > 0)
        {
            // Traditional CLI mode
            Console.WriteLine("===========================================================");
            Console.WriteLine("         SaaSFoundry Engineering Workbench v1.0            ");
            Console.WriteLine("===========================================================");
            Console.WriteLine();

            await router.ExecuteAsync(args);
        }
        else
        {
            // Interactive Engineering Workspace
            var shell = new InteractiveShell(host, router);
            await shell.RunAsync();
        }
    }

    private class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
