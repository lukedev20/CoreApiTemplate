using CoreApiTemplate.Exceptions;

namespace CoreApiTemplate.Integrations.Persistence;

public class DataContextFactory(IServiceProvider serviceProvider) : IDataContextFactory
{
    public IDataContext<T> Get<T>() where T : class
    {
        var service = serviceProvider.GetService(typeof(IDataContext<T>));

        if (service == null) throw new DataContextFactoryException(typeof(T).Name);

        return (IDataContext<T>) service;
    }
}