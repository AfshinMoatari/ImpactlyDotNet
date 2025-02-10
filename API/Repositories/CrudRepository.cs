using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using API.Models;
using Newtonsoft.Json;

namespace API.Repositories
{
    // PK        	|	SK, GSI            	    |	ID    |	  CREATED_AT, GSI_SK
    // MODEL#M1	|	    MODEL#METADATA          |	M1    |   DATE
    // MODEL#M2	|	    MODEL#METADATA          |	M2    |   DATE
    // JOURNAL#J1	|	PROJECT#P2	            |	J1    |   D1
    // JOURNAL#J1	|	USER#U1#PROJECT#P1		|	J1	  |   D1
    // ENTRY#E1		|	JOURNAL#J1#ENTRY	    |	E1    |
    // JOURNAL#J1	|	ENTRY#CREATED_AT#E1	    |	E1    |

    public interface ICrudRepository<T>
    {
        public Task<T> Create(T model);
        public Task<T> Read(string id);
        public Task<T> Update(T model);
        public Task<T> UpdateValue(string id, Action<T> update);
        public Task Delete(string id);
        public Task<IEnumerable<T>> ReadAll();
        public Task<T> ReadFirstOrDefault();

        public Task DeleteBatch(IEnumerable<T> models);

        public Task UpdateBatch(IEnumerable<T> models);
        // DYNAMO
        public DynamoDBContext Context { get; }
        public T ToDynamoItem(T model);
    }

    public class CrudRepository<T> : CrudPropertyRepository<T>, ICrudRepository<T> where T : CrudModel, new()
    {
        public const string Metakey = "META";
        public virtual string Prefix { get; }
        public override string ParentPrefix => Prefix;
        public override string ModelPrefix => Prefix;


        public CrudRepository(IAmazonDynamoDB client) : base(client)
        {
            Prefix = typeof(T).Name.ToUpper();
        }

        public virtual T ToDynamoItem(T model)
        {
            return base.ToDynamoItem(Metakey, model);
        }

        public virtual async Task<T> Create(T model)
        {
            return await base.Create(Metakey, model);
        }

        public async Task<T> Read(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return await base.Read(Metakey, id);
        }

        public virtual async Task<T> Update(T model)
        {
            return await base.Update(Metakey, model);
        }

        public virtual async Task<T> UpdateValue(string id, Action<T> update)
        {
            var model = await Read(id);
            update.Invoke(model);
            return await base.Update(Metakey, model);
        }

        public async Task Delete(string id)
        {
            await base.Delete(Metakey, id);
        }

        public async Task<IEnumerable<T>> ReadAll()
        {
            return await base.ReadAll(Metakey);
        }

        public async Task<T> ReadFirstOrDefault()
        {
            return await base.ReadFirstOrDefault(Metakey);
        }


    }
}