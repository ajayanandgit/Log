using System;
using System.Collections.Generic;
using System.Linq;

namespace Expedien.ERP.Common.Logging
{
    internal class Logger : ILogger
    {
        private readonly LoggerFactory _loggerFactory;
        private readonly string _name;
        private ILogger[] _loggers = new ILogger[0];

        public Logger(LoggerFactory loggerFactory, string name)
        {
            _loggerFactory = loggerFactory;
            _name = name;

            var providers = loggerFactory.GetProviders();
            _loggers = new ILogger[providers.Length];
            for (var index = 0; index != providers.Length; index++)
            {
                _loggers[index] = providers[index].CreateLogger(name);
            }
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (logLevel >= _loggerFactory.MinimumLevel)
            {
                List<Exception> exceptions = null;
                foreach (var logger in _loggers)
                {
                    try
                    {
                        logger.Log(logLevel, eventId, state, exception, formatter);
                    }
                    catch (Exception ex)
                    {
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>();
                        }

                        exceptions.Add(ex);
                    }
                }

                if (exceptions != null && exceptions.Count > 0)
                {
                    throw new AggregateException(
                        message: "An error occurred while writing to logger(s).", innerExceptions: exceptions);
                }
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel < _loggerFactory.MinimumLevel)
            {
                return false;
            }

            List<Exception> exceptions = null;
            foreach (var logger in _loggers)
            {
                try
                {
                    if (logger.IsEnabled(logLevel))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            if (exceptions != null && exceptions.Count > 0)
            {
                throw new AggregateException(
                    message: "An error occurred while writing to logger(s).",
                    innerExceptions: exceptions);
            }

            return false;
        }

        internal void AddProvider(ILoggerProvider provider)
        {
            var logger = provider.CreateLogger(_name);
            _loggers = _loggers.Concat(new[] { logger }).ToArray();
        }
    }
}