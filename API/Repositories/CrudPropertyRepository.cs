using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Util;
using API.Constants;
using API.Extensions;
using API.Models;
using Newtonsoft.Json;

namespace API.Repositories
{
    // PK   |	SK        |   GSI    |    GSISK   |    ID    |    CREATED_AT, GSI_SK
    // T#T1	|	M#DATE    |   T#T1   |    M#M1    |    M1    |    DATE
    // T#T1	|	M#DATE    |   T#T1   |    M#M2    |    M2    |    DATE
    // T#T1	|	M#DATE    |   T#T1   |    M#M3    |    M3    |    DATE
    // ----------------------------------------------------------------
    // P#P1	|	T#DATE    |   P#P1   |    T#T1    |    T1    |    DATE
    // P#P1	|	T#DATE    |   P#P1   |    T#T2    |    T2    |    DATE
    // P#P2	|	T#DATE    |   P#P2   |    T#T1    |    T1    |    DATE


    public interface ICrudPropertyRepository<T> where T : class, ICrudPropModel
    {
        // Methods
        public Task<T> Create(string parentId, T model);
        public Task<T> Read(string parentId, string id);

        public Task<T> Update(string parentId, T model);

        public Task<T> Update(string parentId, string modelId, Action<T> update);

        public Task<T> UpdateValue(string parentId, string id, Action<T> update);
        public Task Delete(string parentId, string id);

        public Task<IEnumerable<T>> ReadAll(string parentId);
        public Task<T> ReadFirstOrDefault(string parentId);

        // DYNAMO
        public DynamoDBContext Context { get; }
        public IAmazonDynamoDB Client { get; }
        public T ToDynamoItem(string parentId, T model);
        public T FromDynamoItem(Dictionary<string, AttributeValue> item);
        public string PartitionKeyByParentId(string id);
        string GSIPartitionKey(T model);
        public string SortKeyById(string id);

        public Task DeleteBatch(IEnumerable<T> models);
        public Task UpdateBatch(IEnumerable<T> models);
    }

    public abstract class CrudPropertyRepository<TProperty> : ICrudPropertyRepository<TProperty>
        where TProperty : class, ICrudPropModel, new()
    {
        public string GlobalSecondaryIndex => CrudPropModel.GlobalSecondaryIndex;

        public IAmazonDynamoDB Client { get; }

        public DynamoDBContext Context { get; }
        public abstract string ParentPrefix { get; }
        public abstract string ModelPrefix { get; }
        public string PartitionKeyByParentId(string id) => $"{ParentPrefix}#{id}";

        public virtual string SortKeyValue(TProperty model) =>
            model.CreatedAt.ToUniversalTime().ToString(Languages.ISO8601DateFormat);

        public virtual string SortKey(TProperty model) => SortKeyById(model.Id);
        public virtual string SortKeyById(string id) => $"{ModelPrefix}#{id}";

        public virtual string GSIPartitionKeyByParentId(string id) => $"{ParentPrefix}#{id}";

        public virtual string GSIPartitionKey(TProperty model) => GSIPartitionKeyByParentId(model.ParentId);

        public virtual string GSISortKey(TProperty model) => $"{ModelPrefix}#{SortKeyValue(model)}";

        public virtual bool Descending => true;

        public CrudPropertyRepository(IAmazonDynamoDB client)
        {
            Client = client;
            Context = new DynamoDBContext(client);
        }

        public virtual TProperty ToDynamoItem(string parentId, TProperty model)
        {
            model.Id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id;
            model.Id = model.Id.IdUrlEncode();
            model.ParentId = parentId;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.PK = PartitionKeyByParentId(parentId);
            model.SK = SortKey(model);
            model.GSIPK = GSIPartitionKey(model);
            model.GSISK = GSISortKey(model);
            return model;
        }

        public TProperty FromDynamoItem(Dictionary<string, AttributeValue> item)
        {
            var attributeMap = Document.FromAttributeMap(item);
            return Context.FromDocument<TProperty>(attributeMap);
        }

        public virtual async Task<TProperty> Create(string parentId, TProperty model)
        {
            model = ToDynamoItem(parentId, model);
            await Context.SaveAsync(model);
            return model;
        }

        public async Task<TProperty> Read(string parentId, string id)
        {
            return await Context.LoadAsync<TProperty>(PartitionKeyByParentId(parentId), SortKeyById(id));
        }

        public virtual async Task<TProperty> Update(string parentId, TProperty model)
        {
            model.UpdatedAt = DateTime.Now;
            model.ParentId = parentId;
            model.PK = PartitionKeyByParentId(parentId);
            model.SK = SortKey(model);
            model.GSIPK = GSIPartitionKey(model);
            model.GSISK = GSISortKey(model);
            await Context.SaveAsync(model);
            return model;
        }

        public virtual async Task<TProperty> Update(string parentId, string modelId, Action<TProperty> update)
        {
            var model = await Read(parentId, modelId);
            update(model);
            return await Update(parentId, model);
        }

        public virtual async Task<TProperty> UpdateValue(string parentId, string id, Action<TProperty> update)
        {
            var model = await Read(parentId, id);
            update.Invoke(model);
            model.UpdatedAt = DateTime.Now;
            return await Update(parentId, model);
            ;
        }

        public async Task Delete(string parentId, string id)
        {
            await Context.DeleteAsync<TProperty>(PartitionKeyByParentId(parentId), SortKeyById(id));
        }

        public virtual async Task<IEnumerable<TProperty>> ReadAll(string parentId)
        {
            return await Context
                .QueryAsync<TProperty>(GSIPartitionKeyByParentId(parentId), QueryOperator.BeginsWith,
                    new[] { ModelPrefix },
                    new DynamoDBOperationConfig { IndexName = GlobalSecondaryIndex, BackwardQuery = Descending })
                .GetRemainingAsync();
        }

        public async Task<TProperty> ReadFirstOrDefault(string parentId)
        {
            return (await ReadAll(parentId)).FirstOrDefault();
        }

        public async Task DeleteBatch(IEnumerable<TProperty> models)
        {
            var batch = Context.CreateBatchWrite<TProperty>();
            batch.AddDeleteItems(models);
            await batch.ExecuteAsync();
        }

        public async Task UpdateBatch(IEnumerable<TProperty> models)
        {
            foreach (var model in models)
            {
                await Context.SaveAsync(model, CancellationToken.None);
            }
        }
    }
}