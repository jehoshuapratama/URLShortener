using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

public class CosmosDbRepository<T> where T : class
{
    private readonly Container _container;

    public CosmosDbRepository(string connectionString, string databaseId, string containerId)
    {
        var cosmosClient = new CosmosClient(connectionString);
        var database = cosmosClient.GetDatabase(databaseId);
        _container = database.GetContainer(containerId);
    }

    public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
    {
        var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
        var results = new List<T>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<T> CreateItemAsync(T item)
    {
        var response = await _container.CreateItemAsync(item);
        return response.Resource;
    }

    public async Task<T> UpdateItemAsync(T item)
    {
        var response = await _container.UpsertItemAsync(item);
        return response.Resource;
    }
}