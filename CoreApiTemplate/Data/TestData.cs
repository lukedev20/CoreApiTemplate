using CoreApiTemplate.Integrations.Persistence;
using CoreApiTemplate.Models;
using MySqlConnector;

namespace CoreApiTemplate.Data;

public class TestData(MySqlConnection context, IConfiguration configuration) : BaseSqlDataContext<Test>(context,
    configuration,
    schemaName: "",
    database: "",
    keyColumnName: ""), IDataContext<Test>;