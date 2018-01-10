// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Sockets.Internal
{
    internal static class SocketLoggerExtensions
    {
        // Category: ConnectionManager
        private static readonly Action<ILogger, Exception> _createdNewConnection =
            LoggerMessage.Define(LogLevel.Debug, new EventId(0, nameof(CreatedNewConnection)), "New connection created.");

        private static readonly Action<ILogger, Exception> _removedConnection =
            LoggerMessage.Define(LogLevel.Debug, new EventId(1, nameof(RemovedConnection)), "Removing connection from the list of connections.");

        private static readonly Action<ILogger, Exception> _failedDispose =
            LoggerMessage.Define(LogLevel.Error, new EventId(2, nameof(FailedDispose)), "Failed disposing connection.");

        private static readonly Action<ILogger, Exception> _connectionReset =
            LoggerMessage.Define(LogLevel.Trace, new EventId(3, nameof(ConnectionReset)), "Connection was reset.");

        private static readonly Action<ILogger, Exception> _connectionTimedOut =
            LoggerMessage.Define(LogLevel.Trace, new EventId(4, nameof(ConnectionTimedOut)), "Connection timed out.");

        private static readonly Action<ILogger, Exception> _scanningConnections =
            LoggerMessage.Define(LogLevel.Trace, new EventId(5, nameof(ScanningConnections)), "Scanning connections.");

        private static readonly Action<ILogger, TimeSpan, Exception> _scannedConnections =
            LoggerMessage.Define<TimeSpan>(LogLevel.Trace, new EventId(6, nameof(ScannedConnections)), "Scanned connections in {duration}.");

        public static void CreatedNewConnection(this ILogger logger)
        {
            _createdNewConnection(logger, null);
        }

        public static void RemovedConnection(this ILogger logger)
        {
            _removedConnection(logger, null);
        }

        public static void FailedDispose(this ILogger logger, Exception exception)
        {
            _failedDispose(logger, exception);
        }

        public static void ConnectionTimedOut(this ILogger logger)
        {
            _connectionTimedOut(logger, null);
        }

        public static void ConnectionReset(this ILogger logger, Exception exception)
        {
            _connectionReset(logger, exception);
        }

        public static void ScanningConnections(this ILogger logger)
        {
            _scanningConnections(logger, null);
        }

        public static void ScannedConnections(this ILogger logger, TimeSpan duration)
        {
            _scannedConnections(logger, duration, null);
        }
    }
}
