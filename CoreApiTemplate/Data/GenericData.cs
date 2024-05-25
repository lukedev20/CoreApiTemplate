using CoreApiTemplate.Integrations.Persistence;
using MySqlConnector;

namespace CoreApiTemplate.Data;

public class GenericData<T>(MySqlConnection context, IConfiguration configuration, string schemaName, string database, string keyColumnName) : BaseSqlDataContext<T>(context,
    configuration,
    schemaName: schemaName,
    database: database,
    keyColumnName: keyColumnName), IDataContext<T>;