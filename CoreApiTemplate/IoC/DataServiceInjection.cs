using System.Reflection;
using CoreApiTemplate.Integrations.Persistence;
using MySqlConnector;

namespace CoreApiTemplate.IoC;

public class DataServiceInjection
{
    public static void Inject(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<MySqlConnection>();

        var instances = Assembly.GetExecutingAssembly()
            .GetTypes().Where(item => item.GetInterfaces().Where(i => i.IsGenericType).Any(i =>
                i.GetGenericTypeDefinition() == typeof(IDataContext<>) &&
                !item.IsAbstract && !item.IsInterface))
            .ToList();

        foreach (var instance in instances)
        {
            var definition = instance.GetInterfaces().First(item =>
                item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IDataContext<>));
            serviceCollection.AddSingleton(definition, instance);
        }
    }
}