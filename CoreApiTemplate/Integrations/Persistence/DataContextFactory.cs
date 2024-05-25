using CoreApiTemplate.Data;
using CoreApiTemplate.Exceptions;
using MySqlConnector;

namespace CoreApiTemplate.Integrations.Persistence;

public class DataContextFactory(IServiceProvider serviceProvider, MySqlConnection context,
    IConfiguration configuration) : IDataContextFactory
{
    public IDataContext<T> Get<T>() where T : class
    {
        var service = serviceProvider.GetService(typeof(IDataContext<T>));

        if (service == null)
        {
            var attributes = typeof(T).GetCustomAttributes(
                typeof(SqlDataAttribute), true
            ).FirstOrDefault() as SqlDataAttribute ?? throw new DataContextFactoryException(typeof(T).Name);

            service = new GenericData<T>(context, configuration, attributes.SchemaName, attributes.Database,
                attributes.KeyColumnName);
        }

        return (IDataContext<T>) service;
    }
}