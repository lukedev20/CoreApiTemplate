namespace CoreApiTemplate.Intergrations.Persistance;

public interface IDataContextFactory
{
    IDataContext<T> Get<T>() where T : class;
}