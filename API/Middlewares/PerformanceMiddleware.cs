﻿using System.Diagnostics;

namespace API.Middlewares;

public class PerformanceMiddleware : IMiddleware
{
    private readonly Stopwatch _stopwatch;

    public PerformanceMiddleware(Stopwatch stopwatch)
    {
        _stopwatch = stopwatch;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _stopwatch.Restart();
        _stopwatch.Start();
        Console.WriteLine("Start performance record");
        await next(context);
        Console.WriteLine("End performance record");
        _stopwatch.Stop();
        var timeTaken = _stopwatch.Elapsed;
        Console.WriteLine("Time taken: " + timeTaken.ToString(@"m\:ss\.fff"));
    }
}