using System;

namespace Expedien.ERP.Common.Logging
{
    /// </summary>
    public interface ILoggerFactory : IDisposable
    {
        LogLevel MinimumLevel { get; set; }

        ILogger CreateLogger(string categoryName);

        void AddProvider(ILoggerProvider provider);
    }
}