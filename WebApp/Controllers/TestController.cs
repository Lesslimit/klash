using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class TestController : Controller
{
    private readonly ILogger logger;
    public TestController (ILogger logger)
    {
        this.logger = logger;
    }

    [Route("test")]
    public IActionResult Index()
    {
        return Ok("Test Controller");
    }
}

class CustomLogger : ILogger
{
    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }

    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        Console.WriteLine(state);
    }
}