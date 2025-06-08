using Microsoft.Extensions.Logging;

namespace Orleans.Grains.Filters;

public class LoggingIncomingGrain:IIncomingGrainCallFilter
{
    private readonly ILogger<LoggingIncomingGrain> _logger;

    public LoggingIncomingGrain(ILogger<LoggingIncomingGrain> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        _logger.LogInformation(
            $"Incoming Silo grain filter: Received grain on '{context.Grain}' to '{context.MethodName}' method");
        await context.Invoke();

    }
}