using CoreApiTemplate.Integrations.Persistence;

namespace CoreApiTemplate.IoC;

public class ServiceInjection
{
    public static void Inject(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDataContextFactory, DataContextFactory>();

    }
}