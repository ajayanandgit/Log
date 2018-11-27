using System;

namespace Expedien.ERP.Common.Logging
{
    public interface ILogger
    {
        void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter);

        bool IsEnabled(LogLevel logLevel);
    }
}
