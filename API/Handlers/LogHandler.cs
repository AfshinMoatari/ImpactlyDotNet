using System;
using System.Threading.Tasks;
using API.Models.Logs;
using API.Repositories;

namespace API.Handlers;

public interface ILogHandler
{
    public Task AddLog(Log log);
}

public class LogHandler: ILogHandler
{
    private readonly ILogContext _logContext;

    public LogHandler(ILogContext logContext)
    {
        _logContext = logContext;
    }


    public async Task AddLog(Log log)
    {
        await _logContext.AddLog(_logContext.Logs.ToDynamoItem(log));
    }
}