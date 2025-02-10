using System.Threading.Tasks;
using Amazon.CloudWatchLogs;
using Amazon.DynamoDBv2;
using API.Models.Logs;
using Microsoft.AspNetCore.Mvc;

namespace API.Repositories;

public interface ILogContext
{
    public ICrudRepository<Log> Logs { get; }

    public Task<ActionResult> AddLog(Log log);
}

public class LogContext : ILogContext
{
    public ICrudRepository<Log> Logs { get; }

    public LogContext(IAmazonDynamoDB client)
    {
        Logs = new LogRepository(client);
    }
    
    public async Task<ActionResult> AddLog(Log log)
    {
        var result = await Logs.Create(log);
        return new OkResult();
    }

    public class LogRepository : CrudRepository<Log>
    {
        public LogRepository(IAmazonDynamoDB client) : base(client)
        {
            
        }
        public override Log ToDynamoItem(Log model)
        {
            return base.ToDynamoItem(model.Type, model);
        }

        public override async Task<Log> Create(Log model)
        {
            model = ToDynamoItem(model.Type, model);
            await Context.SaveAsync(model);
            return model;
        }
    }
}