﻿using System;

namespace Expedien.ERP.Common.Logging.EventLog
{
    /// <summary>
    /// Settings for <see cref="EventLogLogger"/>.
    /// </summary>
    public class EventLogSettings
    {
        /// <summary>
        /// Name of the event log. If <c>null</c> or not specified, "Application" is the default.
        /// </summary>
        public string LogName { get; set; }

        /// <summary>
        /// Name of the event log source. If <c>null</c> or not specified, "Application" is the default.
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// Name of the machine having the event log. If <c>null</c> or not specified, local machine is the default.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// The function used to filter events based on the log level.
        /// </summary>
        public Func<string, LogLevel, bool> Filter { get; set; }
    }
}
