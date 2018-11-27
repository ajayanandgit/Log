using JetBrains.Annotations;
using System;

namespace Expedien.ERP.Common.Logging.EventLog
{
    /// <summary>
    /// Extension methods for the <see cref="ILoggerFactory"/> class.
    /// </summary>
    public static class EventLoggerFactoryExtensions
    {
        /// <summary>
        /// Adds an event logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// </summary>
        /// <param name="factory">The extension method argument.</param>
        public static ILoggerFactory AddEventLog([NotNull] this ILoggerFactory factory)
        {
            return AddEventLog(factory, LogLevel.Information);
        }

        /// <summary>
        /// Adds an event logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory">The extension method argument.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        public static ILoggerFactory AddEventLog([NotNull] this ILoggerFactory factory, LogLevel minLevel)
        {
            return AddEventLog(factory, new EventLogSettings()
            {
                Filter = (_, logLevel) => logLevel >= minLevel
            });
        }

        /// <summary>
        /// Adds an event logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory">The extension method argument.</param>
        /// <param name="settings">The <see cref="EventLogSettings"/>.</param>
        public static ILoggerFactory AddEventLog(
            [NotNull] this ILoggerFactory factory,
            [NotNull] EventLogSettings settings)
        {
            factory.AddProvider(new EventLogLoggerProvider(settings));
            return factory;
        }
    }
}