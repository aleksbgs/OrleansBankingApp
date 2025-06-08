using Microsoft.Extensions.Logging;

namespace Orleans.Grains.Filters;

public class LogginOutgoingFilter:IOutgoingGrainCallFilter
{
    private readonly Logger<LogginOutgoingFilter> _logger;

    public LogginOutgoingFilter(Logger<LogginOutgoingFilter> logger)
    {
        _logger = logger;
    }
    public async Task Invoke(IOutgoingGrainCallContext context)
    {
        _logger.LogInformation(
            $"Outgoing Silo grain filter: Received grain on '{context.Grain}' to '{context.MethodName}' method");
        await context.Invoke();
    }
}