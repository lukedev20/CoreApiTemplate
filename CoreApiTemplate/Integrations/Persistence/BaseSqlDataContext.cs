using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using CoreApiTemplate.Exceptions;
using Dapper;
using MySqlConnector;

namespace CoreApiTemplate.Integrations.Persistence;

public abstract class BaseSqlDataContext<T>(
    MySqlConnection context,
    IConfiguration configuration,
    string schemaName,
    string database,
    string keyColumnName,
    string connectionName = "Default")
    : IDisposable, IAsyncDisposable
{

    private string? _connectionString;

    public virtual IEnumerable<T> GetAllEntries()
    {
        return GetAllEntries(null);
    }

    public virtual async Task<IEnumerable<T>> GetAllEntriesAsync()
    {
        return await GetAllEntriesAsync(null);
    }

    public virtual IEnumerable<T> GetAllEntries(IDictionary<string, dynamic>? filters)
    {
        OpenConnection();
        string query = $"select * from {schemaName}.{database} {CreateFilterConditions(filters)}";
        return context.Query<T>(query);
    }

    public async Task<IEnumerable<T>> GetAllEntriesAsync(IDictionary<string, dynamic>? filters)
    {
        OpenConnection();
        string query = $"select * from {schemaName}.{database} {CreateFilterConditions(filters)}";
        return await context.QueryAsync<T>(query);
    }

    public virtual T GetEntry(int entryId)
    {
        return GetAllEntries(new Dictionary<string, dynamic>
        {
            {keyColumnName, entryId}
        }).FirstOrDefault() ?? throw new DataContextException();
    }

    public virtual async Task<T> GetEntryAsync(int entryId)
    {
        return (await GetAllEntriesAsync(new Dictionary<string, dynamic>
        {
            {keyColumnName, entryId}
        })).FirstOrDefault() ?? throw new DataContextException();
    }

    public virtual T GetEntry(T entry)
    {
        OpenConnection();
        string query = $"SELECT * FROM {schemaName}.{database} {CreateFilterConditions(entry)}";

        return context.QuerySingle<T>(query);
    }

    public virtual T SaveEntry(T entry)
    {
        OpenConnection();
        string query = $"INSERT INTO {schemaName}.{database} VALUES ({CreateValueArray(entry)}); SELECT LAST_INSERT_ID();";

        var insertedEntry = context.QuerySingle<int>(query);

        return GetEntry(insertedEntry);
    }

    public virtual async Task<T> SaveEntryAsync(T entry)
    {
        OpenConnection();
        string query = $"INSERT INTO {schemaName}.{database} VALUES ({CreateValueArray(entry)}); SELECT LAST_INSERT_ID();";

        var insertedEntry = context.QuerySingle<int>(query);

        return await GetEntryAsync(insertedEntry);
    }

    public virtual void RemoveEntry(int entryId)
    {
        OpenConnection();
        string query = $"DELETE FROM {schemaName}.{database} WHERE {keyColumnName} = {entryId};";

        context.Execute(query);
    }

    public virtual async Task RemoveEntryAsync(int entryId)
    {
        OpenConnection();
        string query = $"DELETE FROM {schemaName}.{database} WHERE {keyColumnName} = {entryId};";

        await context.ExecuteAsync(query);
    }

    public virtual T UpdateEntry(int entryId, IDictionary<string, dynamic> updates)
    {
        OpenConnection();
        string query = $"UPDATE {schemaName}.{database} SET {CreateFilterConditions(updates)} WHERE {keyColumnName} = {entryId}";
        context.Execute(query);

        return GetEntry(entryId);
    }

    public virtual async Task<T> UpdateEntryAsync(int entryId, IDictionary<string, dynamic> updates)
    {
        OpenConnection();
        string query = $"UPDATE {schemaName}.{database} SET {CreateFilterConditions(updates)} WHERE {keyColumnName} = {entryId}";
        await context.ExecuteAsync(query);

        return await GetEntryAsync(entryId);
    }

    protected void OpenConnection()
    {
        if (context.State == ConnectionState.Open)
        {
            return;
        }

        _connectionString ??= configuration.GetConnectionString(connectionName) ?? throw new DataContextException();
        context.ConnectionString = _connectionString;
        context.Open();
    }

    protected string CreateFilterConditions(IDictionary<string, dynamic>? filters)
    {
        string filterString = "";

        if (filters != null)
        {
            int numberOfFilters = filters.Count;

            if (filters.Any())
            {
                for (int filter = 0; filter < numberOfFilters; filter++)
                {
                    if (filter == 0) filterString += " WHERE ";
                    if (filter != 0) filterString += " AND ";

                    var element = filters.ElementAt(filter);

                    if (element.Value is string)
                    {
                        filterString += $"{element.Key} = '{(string)element.Value}'";
                    }
                    else if (element.Value is int)
                    {
                        filterString += $"{element.Key} = {(int)element.Value}";
                    }
                    else if (element.Value is decimal)
                    {
                        filterString += $"{element.Key} = {(decimal)element.Value}";
                    }
                    else if (element.Value is DateTime)
                    {
                        filterString += $"{element.Key} = {((DateTime)element.Value).ToString("yyyy-MM-dd HH:mm:ss")}";
                    }
                    else
                    {
                        filterString += $"{element.Key} = '{(string)element.Value}'";
                    }
                }
            }
        }

        return filterString;
    }

    protected string CreateFilterConditions(T? data)
    {
        var conditions = string.Empty;
        if (data != null)
        {
            foreach (var prop in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(prop, typeof(NotMappedAttribute))) continue;

                if (conditions == string.Empty)
                {
                    conditions += CreateCondition(prop, data, "WHERE");
                    continue;
                }
                conditions += CreateCondition(prop, data, "AND");
            }
        }

        return conditions;
    }

    protected string CreateValueArray(T? data)
    {
        var conditions = string.Empty;
        if (data != null)
        {
            foreach (var prop in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(prop, typeof(NotMappedAttribute))) continue;

                if (conditions == string.Empty)
                {
                    conditions += CreateCondition(prop, data, "", false);
                    continue;
                }
                conditions += CreateCondition(prop, data, ",", false);
            }
        }

        return conditions;
    }

    protected string CreateValueConditions(T? data)
    {
        var conditions = "";
        if (data != null)
        {
            foreach (var prop in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(prop, typeof(NotMappedAttribute))) continue;

                if (conditions == string.Empty)
                {
                    conditions += CreateCondition(prop, data, "" );
                    continue;
                }
                conditions += CreateCondition(prop, data, "," );
            }
        }

        return conditions;
    }

    protected string CreateCondition(PropertyInfo? property, T item, string separator, bool includeKey = true)
    {
        string NullCondition(string propertyName)
        {
            if (includeKey)
            {
                return $"{separator} {propertyName} = null";
            }

            return $"{separator}null";
        }

        var type = Nullable.GetUnderlyingType(property!.PropertyType) ?? property.PropertyType;
        var propertyName = property.Name;

        if (type == typeof(int))
        {
            var value = (int?) property.GetValue(item);

            if(value == null) return NullCondition(propertyName);

            if (includeKey)
            {
                return $"{separator} {propertyName} = {value}";
            }

            return $"{separator}{value}";
        }
        if (type == typeof(string))
        {
            var value = (string?) property.GetValue(item);

            if(value == null) return NullCondition(propertyName);

            if (includeKey)
            {
                return $"{separator} {propertyName} =  '{value}'";
            }
            return $"{separator}'{value}'";
        }
        if (type == typeof(decimal))
        {
            var value = (decimal?) property.GetValue(item);

            if(value == null) return NullCondition(propertyName);

            if (includeKey)
            {
                return $"{separator} {propertyName} =  {value}";
            }
            return $"{separator}{value}";
        }
        if (type == typeof(double))
        {
            var value = (double?) property.GetValue(item);

            if(value == null) return NullCondition(propertyName);

            if (includeKey)
            {
                return $"{separator} {propertyName} =  {value}";
            }
            return $"{separator}{value}";
        }
        if (type == typeof(DateTime))
        {
            var value = (DateTime?) property.GetValue(item);

            if(value == null) return NullCondition(propertyName);

            if (includeKey)
            {
                return $"{separator} {propertyName} =  '{((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss")}'";
            }
            return $"{separator}'{((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss")}'";
        }
        else
        {
            var value = (string?) property.GetValue(item);

            if(value == null) return NullCondition(propertyName);

            if (includeKey)
            {
                return $"{separator} {propertyName} =  '{value}'";
            }
            return $"{separator}'{value}'";
        }
    }

    public virtual void Dispose()
    {
        context.Dispose();
    }

    public virtual async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }
}