using System;

namespace Expedien.ERP.Common.Logging
{
    public interface ILoggerProvider : IDisposable
    {
        ILogger CreateLogger(string name);
    }
}
