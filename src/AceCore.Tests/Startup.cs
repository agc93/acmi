using Microsoft.Extensions.DependencyInjection;

namespace AceCore.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) {
            services.Scan(scan =>
                scan.FromAssemblyOf<Identifier>()
                    .AddClasses(classes => classes.AssignableTo(typeof(AceCore.Parsers.IIdentifierParser))).AsImplementedInterfaces().WithSingletonLifetime()
            );
        }
    }
}