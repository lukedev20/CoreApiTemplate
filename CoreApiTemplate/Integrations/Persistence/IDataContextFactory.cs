namespace CoreApiTemplate.Integrations.Persistence;

public interface IDataContextFactory
{
    IDataContext<T> Get<T>() where T : class;
}