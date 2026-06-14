using System;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Questionable.Logging;

// Inline replacement for Dalamud.Extensions.MicrosoftLogging 4.0.1 (API15 / net10).
// Bridges Microsoft.Extensions.Logging.ILogger calls to Dalamud's IPluginLog.
internal static class DalamudLoggerExtensions
{
    public static ILoggingBuilder AddDalamudLogger(this ILoggingBuilder builder,
        IPluginLog pluginLog, Func<string, string>? categoryNameFormatter = null)
    {
        builder.AddProvider(new DalamudLoggerProvider(pluginLog, categoryNameFormatter));
        return builder;
    }
}

internal sealed class DalamudLoggerProvider : ILoggerProvider
{
    private readonly IPluginLog _pluginLog;
    private readonly Func<string, string>? _categoryFormatter;

    public DalamudLoggerProvider(IPluginLog pluginLog, Func<string, string>? categoryFormatter)
    {
        _pluginLog = pluginLog;
        _categoryFormatter = categoryFormatter;
    }

    public ILogger CreateLogger(string categoryName) =>
        new DalamudLogger(_pluginLog, _categoryFormatter?.Invoke(categoryName) ?? categoryName);

    public void Dispose() { }
}

internal sealed class DalamudLogger : ILogger
{
    private readonly IPluginLog _pluginLog;
    private readonly string _category;

    public DalamudLogger(IPluginLog pluginLog, string category)
    {
        _pluginLog = pluginLog;
        _category = category;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var msg = $"[{_category}] {formatter(state, exception)}";
        switch (logLevel)
        {
            case LogLevel.Trace: _pluginLog.Verbose(exception, msg); break;
            case LogLevel.Debug: _pluginLog.Debug(exception, msg); break;
            case LogLevel.Information: _pluginLog.Information(exception, msg); break;
            case LogLevel.Warning: _pluginLog.Warning(exception, msg); break;
            case LogLevel.Error: _pluginLog.Error(exception, msg); break;
            case LogLevel.Critical: _pluginLog.Fatal(exception, msg); break;
        }
    }
}
