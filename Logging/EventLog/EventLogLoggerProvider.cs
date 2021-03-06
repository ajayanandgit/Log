﻿using System;

namespace Expedien.ERP.Common.Logging.EventLog
{
    /// <summary>
    /// The provider for the <see cref="EventLogLogger"/>.
    /// </summary>
    public class EventLogLoggerProvider : ILoggerProvider
    {
        private readonly EventLogSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogLoggerProvider"/> class.
        /// </summary>
        public EventLogLoggerProvider()
            : this(settings: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogLoggerProvider"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="EventLogSettings"/>.</param>
        public EventLogLoggerProvider(EventLogSettings settings)
        {
            _settings = settings;
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string name)
        {
            return new EventLogLogger(name, _settings ?? new EventLogSettings());
        }

        public void Dispose()
        {
        }
    }
}
