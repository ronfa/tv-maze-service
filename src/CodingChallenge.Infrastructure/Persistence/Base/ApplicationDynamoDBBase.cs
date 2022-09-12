using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CodingChallenge.Infrastructure.Extensions;
using CodingChallenge.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Infrastructure.Persistence.Base;

public interface IApplicationDynamoDBBase<T> where T : class
{
    Task DeleteAsync(List<T> dataModelItems, CancellationToken cancellationToken);
    Task<T> GetAsync(string id, CancellationToken cancellationToken);
    Task SaveAsync(List<T> dataModelItems, CancellationToken cancellationToken);
    Task UpdateAsync(List<T> dataModelItems, CancellationToken cancellationToken);
}

public class ApplicationDynamoDBBase<T> : DynamoDBRepository, IApplicationDynamoDBBase<T> where T : class
{
    protected virtual DynamoDBOperationConfig DynamoDBOperationConfig { get; }

    public ApplicationDynamoDBBase(ILogger logger, AWSAppProject awsApplication) : base(logger, awsApplication)
    {
        DynamoDBOperationConfig = GetOperationConfig(AwsApplication.GetDynamodbTableName(typeof(T)));
    }

    public async Task DeleteAsync(List<T> dataModelItems, CancellationToken cancellationToken)
    {
        var dataModelBatch = Context.CreateBatchWrite<T>(DynamoDBOperationConfig);
        dataModelBatch.AddDeleteItems(dataModelItems);
        await dataModelBatch.ExecuteAsync();
    }

    public async Task<T> GetAsync(string id, CancellationToken cancellationToken)
    {
        var result = await Context.LoadAsync<T>(id, DynamoDBOperationConfig);
        return result;
    }

    public async Task<T> GetAsync(int id, CancellationToken cancellationToken)
    {
        var result = await Context.LoadAsync<T>(id, DynamoDBOperationConfig);
        return result;
    }

    public async Task<Tuple<List<T>, string>> GetListAsync(int limit, string? paginationToken = null,
        CancellationToken cancellationToken = default)
    {
        var table = Context.GetTargetTable<T>(DynamoDBOperationConfig);

        var config = new ScanOperationConfig();
        config.Limit = limit;
        //config. Filter = new QueryFilter(sortKeyField, QueryOperator.Equal, 2012);
        if (!string.IsNullOrWhiteSpace(paginationToken)) config.PaginationToken = paginationToken;
        var results = table.Scan(config);
        var items = await results.GetNextSetAsync(cancellationToken);
        var employees = Context.FromDocuments<T>(items);
        var returnList = employees.ToList();
        return new Tuple<List<T>, string>(returnList, results.PaginationToken);
    }

    public async Task<Tuple<List<T>, string>> GetListViaQueryAsync(int limit, string? paginationToken = null)
    {
        var table = Context.GetTargetTable<T>(DynamoDBOperationConfig);
        //var filter = new QueryFilter("TVMazeIndex", QueryOperator.GreaterThan,0);
        var config = new QueryOperationConfig();
        config.Limit = limit;
        //config.Filter = filter;
        config.KeyExpression = new Expression
        {
            ExpressionStatement = "TVMazeIndex = :pk",
            ExpressionAttributeValues =
            {
                {"pk", 1}
            }
        };

        //config. Filter = new QueryFilter(sortKeyField, QueryOperator.Equal, 2012);    
        if (!string.IsNullOrWhiteSpace(paginationToken)) config.PaginationToken = paginationToken;
        var results = table.Query(config);
        var items = await results.GetNextSetAsync();
        var employees = Context.FromDocuments<T>(items);
        var returnList = employees.ToList();
        return new Tuple<List<T>, string>(returnList, results.PaginationToken);
    }

    public async Task<List<T>> GetBySortKeyAsync(string sortKeyField, string sortKeyValue)
    {
        return await Context.ScanAsync<T>(
                new ScanCondition[] {new(sortKeyField, ScanOperator.Equal, sortKeyValue)}, DynamoDBOperationConfig)
            .GetRemainingAsync();
    }

    public async Task SaveAsync(List<T> dataModelItems, CancellationToken cancellationToken)
    {
        var dataModelBatch = Context.CreateBatchWrite<T>(DynamoDBOperationConfig);
        dataModelBatch.AddPutItems(dataModelItems);
        await dataModelBatch.ExecuteAsync(cancellationToken);
    }

    public async Task UpdateAsync(List<T> dataModelItems, CancellationToken cancellationToken)
    {
        var dataModelBatch = Context.CreateBatchWrite<T>(DynamoDBOperationConfig);
        dataModelBatch.AddPutItems(dataModelItems);
        await dataModelBatch.ExecuteAsync(cancellationToken);
    }
}