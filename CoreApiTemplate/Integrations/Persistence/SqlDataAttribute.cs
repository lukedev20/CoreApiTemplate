namespace CoreApiTemplate.Integrations.Persistence;

[AttributeUsage(AttributeTargets.Class)]
public class SqlDataAttribute(string schemaName, string database, string keyColumnName) : Attribute
{
    public string SchemaName { get; set; } = schemaName;

    public string Database { get; set; } = database;

    public string KeyColumnName { get; set; } = keyColumnName;
}