// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Sockets.Client.Internal
{
    internal static class SocketClientLoggerExtensions
    {
        // Category: Shared with LongPollingTransport, WebSocketsTransport and ServerSentEventsTransport
        private static readonly Action<ILogger, TransferMode, Exception> _startTransport =
            LoggerMessage.Define<TransferMode>(LogLevel.Information, new EventId(0, nameof(StartTransport)), "Starting transport. Transfer mode: {transferMode}.");

        private static readonly Action<ILogger, Exception> _transportStopped =
            LoggerMessage.Define(LogLevel.Debug, new EventId(1, nameof(TransportStopped)), "Transport stopped.");

        private static readonly Action<ILogger, Exception> _startReceive =
            LoggerMessage.Define(LogLevel.Debug, new EventId(2, nameof(StartReceive)), "Starting receive loop.");

        private static readonly Action<ILogger, Exception> _receiveStopped =
            LoggerMessage.Define(LogLevel.Debug, new EventId(3, nameof(ReceiveStopped)), "Receive loop stopped.");

        private static readonly Action<ILogger, Exception> _receiveCanceled =
            LoggerMessage.Define(LogLevel.Debug, new EventId(4, nameof(ReceiveCanceled)), "Receive loop canceled.");

        private static readonly Action<ILogger, Exception> _transportStopping =
            LoggerMessage.Define(LogLevel.Information, new EventId(5, nameof(TransportStopping)), "Transport is stopping.");

        private static readonly Action<ILogger, Exception> _sendStarted =
            LoggerMessage.Define(LogLevel.Debug, new EventId(6, nameof(SendStarted)), "Starting the send loop.");

        private static readonly Action<ILogger, Exception> _sendStopped =
            LoggerMessage.Define(LogLevel.Debug, new EventId(7, nameof(SendStopped)), "Send loop stopped.");

        private static readonly Action<ILogger, Exception> _sendCanceled =
            LoggerMessage.Define(LogLevel.Debug, new EventId(8, nameof(SendCanceled)), "Send loop canceled.");

        // Category: WebSocketsTransport
        private static readonly Action<ILogger, WebSocketCloseStatus?, Exception> _webSocketClosed =
            LoggerMessage.Define<WebSocketCloseStatus?>(LogLevel.Information, new EventId(9, nameof(WebSocketClosed)), "Websocket closed by the server. Close status {closeStatus}.");

        private static readonly Action<ILogger, WebSocketMessageType, int, bool, Exception> _messageReceived =
            LoggerMessage.Define<WebSocketMessageType, int, bool>(LogLevel.Debug, new EventId(10, nameof(MessageReceived)), "Message received. Type: {messageType}, size: {count}, EndOfMessage: {endOfMessage}.");

        private static readonly Action<ILogger, int, Exception> _messageToApp =
            LoggerMessage.Define<int>(LogLevel.Debug, new EventId(11, nameof(MessageToApp)), "Passing message to application. Payload size: {count}.");

        private static readonly Action<ILogger, int, Exception> _receivedFromApp =
            LoggerMessage.Define<int>(LogLevel.Debug, new EventId(12, nameof(ReceivedFromApp)), "Received message from application. Payload size: {count}.");

        private static readonly Action<ILogger, Exception> _sendMessageCanceled =
            LoggerMessage.Define(LogLevel.Information, new EventId(13, nameof(SendMessageCanceled)), "Sending a message canceled.");

        private static readonly Action<ILogger, Exception> _errorSendingMessage =
            LoggerMessage.Define(LogLevel.Error, new EventId(14, nameof(ErrorSendingMessage)), "Error while sending a message.");

        private static readonly Action<ILogger, Exception> _closingWebSocket =
            LoggerMessage.Define(LogLevel.Information, new EventId(15, nameof(ClosingWebSocket)), "Closing WebSocket.");

        private static readonly Action<ILogger, Exception> _closingWebSocketFailed =
            LoggerMessage.Define(LogLevel.Information, new EventId(16, nameof(ClosingWebSocketFailed)), "Closing webSocket failed.");

        private static readonly Action<ILogger, Exception> _cancelMessage =
            LoggerMessage.Define(LogLevel.Debug, new EventId(17, nameof(CancelMessage)), "Canceled passing message to application.");

        // Category: ServerSentEventsTransport and LongPollingTransport
        private static readonly Action<ILogger, int, Uri, Exception> _sendingMessages =
            LoggerMessage.Define<int, Uri>(LogLevel.Debug, new EventId(9, nameof(SendingMessages)), "Sending {count} message(s) to the server using url: {url}.");

        private static readonly Action<ILogger, Exception> _sentSuccessfully =
            LoggerMessage.Define(LogLevel.Debug, new EventId(10, nameof(SentSuccessfully)), "Message(s) sent successfully.");

        private static readonly Action<ILogger, Exception> _noMessages =
            LoggerMessage.Define(LogLevel.Debug, new EventId(11, nameof(NoMessages)), "No messages in batch to send.");

        private static readonly Action<ILogger, Uri, Exception> _errorSending =
            LoggerMessage.Define<Uri>(LogLevel.Error, new EventId(12, nameof(ErrorSending)), "Error while sending to '{url}'.");

        // Category: ServerSentEventsTransport
        private static readonly Action<ILogger, Exception> _eventStreamEnded =
            LoggerMessage.Define(LogLevel.Debug, new EventId(13, nameof(EventStreamEnded)), "Server-Sent Event Stream ended.");

        // Category: LongPollingTransport
        private static readonly Action<ILogger, Exception> _closingConnection =
            LoggerMessage.Define(LogLevel.Debug, new EventId(13, nameof(ClosingConnection)), "The server is closing the connection.");

        private static readonly Action<ILogger, Exception> _receivedMessages =
            LoggerMessage.Define(LogLevel.Debug, new EventId(14, nameof(ReceivedMessages)), "Received messages from the server.");

        private static readonly Action<ILogger, Uri, Exception> _errorPolling =
            LoggerMessage.Define<Uri>(LogLevel.Error, new EventId(15, nameof(ErrorPolling)), "Error while polling '{pollUrl}'.");

        // Category: HttpConnection
        private static readonly Action<ILogger, Exception> _httpConnectionStarting =
            LoggerMessage.Define(LogLevel.Debug, new EventId(0, nameof(HttpConnectionStarting)), "Starting connection.");

        private static readonly Action<ILogger, Exception> _httpConnectionClosed =
            LoggerMessage.Define(LogLevel.Debug, new EventId(1, nameof(HttpConnectionClosed)), "Connection was closed from a different thread.");

        private static readonly Action<ILogger, DateTime, string, string, Uri, Exception> _startingTransport =
            LoggerMessage.Define<DateTime, string, string, Uri>(LogLevel.Debug, new EventId(2, nameof(StartingTransport)), "{time}: Connection Id {connectionId}: Starting transport '{transport}' with Url: {url}.");

        private static readonly Action<ILogger, DateTime, string, Exception> _raiseConnected =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(3, nameof(RaiseConnected)), "{time}: Connection Id {connectionId}: Raising Connected event.");

        private static readonly Action<ILogger, DateTime, string, Exception> _processRemainingMessages =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(4, nameof(ProcessRemainingMessages)), "{time}: Connection Id {connectionId}: Ensuring all outstanding messages are processed.");

        private static readonly Action<ILogger, DateTime, string, Exception> _drainEvents =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(5, nameof(DrainEvents)), "{time}: Connection Id {connectionId}: Draining event queue.");

        private static readonly Action<ILogger, DateTime, string, Exception> _completeClosed =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(6, nameof(CompleteClosed)), "{time}: Connection Id {connectionId}: Completing Closed task.");

        private static readonly Action<ILogger, DateTime, Uri, Exception> _establishingConnection =
            LoggerMessage.Define<DateTime, Uri>(LogLevel.Debug, new EventId(7, nameof(EstablishingConnection)), "{time}: Establishing Connection at: {url}.");

        private static readonly Action<ILogger, DateTime, Uri, Exception> _errorWithNegotiation =
            LoggerMessage.Define<DateTime, Uri>(LogLevel.Error, new EventId(8, nameof(ErrorWithNegotiation)), "{time}: Failed to start connection. Error getting negotiation response from '{url}'.");

        private static readonly Action<ILogger, DateTime, string, string, Exception> _errorStartingTransport =
            LoggerMessage.Define<DateTime, string, string>(LogLevel.Error, new EventId(9, nameof(ErrorStartingTransport)), "{time}: Connection Id {connectionId}: Failed to start connection. Error starting transport '{transport}'.");

        private static readonly Action<ILogger, DateTime, string, Exception> _httpReceiveStarted =
            LoggerMessage.Define<DateTime, string>(LogLevel.Trace, new EventId(10, nameof(HttpReceiveStarted)), "{time}: Connection Id {connectionId}: Beginning receive loop.");

        private static readonly Action<ILogger, DateTime, string, Exception> _skipRaisingReceiveEvent =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(11, nameof(SkipRaisingReceiveEvent)), "{time}: Connection Id {connectionId}: Message received but connection is not connected. Skipping raising Received event.");

        private static readonly Action<ILogger, DateTime, string, Exception> _scheduleReceiveEvent =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(12, nameof(ScheduleReceiveEvent)), "{time}: Connection Id {connectionId}: Scheduling raising Received event.");

        private static readonly Action<ILogger, DateTime, string, Exception> _raiseReceiveEvent =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(13, nameof(RaiseReceiveEvent)), "{time}: Connection Id {connectionId}: Raising Received event.");

        private static readonly Action<ILogger, DateTime, string, Exception> _failedReadingMessage =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(14, nameof(FailedReadingMessage)), "{time}: Connection Id {connectionId}: Could not read message.");

        private static readonly Action<ILogger, DateTime, string, Exception> _errorReceiving =
            LoggerMessage.Define<DateTime, string>(LogLevel.Error, new EventId(15, nameof(ErrorReceiving)), "{time}: Connection Id {connectionId}: Error receiving message.");

        private static readonly Action<ILogger, DateTime, string, Exception> _endReceive =
            LoggerMessage.Define<DateTime, string>(LogLevel.Trace, new EventId(16, nameof(EndReceive)), "{time}: Connection Id {connectionId}: Ending receive loop.");

        private static readonly Action<ILogger, DateTime, string, Exception> _sendingMessage =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(17, nameof(SendingMessage)), "{time}: Connection Id {connectionId}: Sending message.");

        private static readonly Action<ILogger, DateTime, string, Exception> _stoppingClient =
            LoggerMessage.Define<DateTime, string>(LogLevel.Information, new EventId(18, nameof(StoppingClient)), "{time}: Connection Id {connectionId}: Stopping client.");

        private static readonly Action<ILogger, DateTime, string, string, Exception> _exceptionThrownFromCallback =
            LoggerMessage.Define<DateTime, string, string>(LogLevel.Error, new EventId(19, nameof(ExceptionThrownFromCallback)), "{time}: Connection Id {connectionId}: An exception was thrown from the '{callback}' callback.");

        private static readonly Action<ILogger, DateTime, string, Exception> _disposingClient =
            LoggerMessage.Define<DateTime, string>(LogLevel.Information, new EventId(20, nameof(DisposingClient)), "{time}: Connection Id {connectionId}: Disposing client.");

        private static readonly Action<ILogger, DateTime, string, Exception> _abortingClient =
            LoggerMessage.Define<DateTime, string>(LogLevel.Error, new EventId(21, nameof(AbortingClient)), "{time}: Connection Id {connectionId}: Aborting client.");

        private static readonly Action<ILogger, Exception> _errorDuringClosedEvent =
            LoggerMessage.Define(LogLevel.Error, new EventId(22, nameof(ErrorDuringClosedEvent)), "An exception was thrown in the handler for the Closed event.");

        private static readonly Action<ILogger, DateTime, string, Exception> _skippingStop =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(23, nameof(SkippingStop)), "{time}: Connection Id {connectionId}: Skipping stop, connection is already stopped.");

        private static readonly Action<ILogger, DateTime, string, Exception> _skippingDispose =
            LoggerMessage.Define<DateTime, string>(LogLevel.Debug, new EventId(24, nameof(SkippingDispose)), "{time}: Connection Id {connectionId}: Skipping dispose, connection is already disposed.");

        private static readonly Action<ILogger, DateTime, string, string, string, Exception> _connectionStateChanged =
            LoggerMessage.Define<DateTime, string, string, string>(LogLevel.Debug, new EventId(25, nameof(ConnectionStateChanged)), "{time}: Connection Id {connectionId}: Connection state changed from {previousState} to {newState}.");

        public static void StartTransport(this ILogger logger, TransferMode transferMode)
        {
            _startTransport(logger, transferMode, null);
        }

        public static void TransportStopped(this ILogger logger, Exception exception)
        {
            _transportStopped(logger, exception);
        }

        public static void StartReceive(this ILogger logger)
        {
            _startReceive(logger, null);
        }

        public static void TransportStopping(this ILogger logger)
        {
            _transportStopping(logger, null);
        }

        public static void WebSocketClosed(this ILogger logger, WebSocketCloseStatus? closeStatus)
        {
            _webSocketClosed(logger, closeStatus, null);
        }

        public static void MessageReceived(this ILogger logger, WebSocketMessageType messageType, int count, bool endOfMessage)
        {
            _messageReceived(logger, messageType, count, endOfMessage, null);
        }

        public static void MessageToApp(this ILogger logger, int count)
        {
            _messageToApp(logger, count, null);
        }

        public static void ReceiveCanceled(this ILogger logger)
        {
            _receiveCanceled(logger, null);
        }

        public static void ReceiveStopped(this ILogger logger)
        {
            _receiveStopped(logger, null);
        }

        public static void SendStarted(this ILogger logger)
        {
            _sendStarted(logger, null);
        }

        public static void ReceivedFromApp(this ILogger logger, int count)
        {
            _receivedFromApp(logger, count, null);
        }

        public static void SendMessageCanceled(this ILogger logger)
        {
            _sendMessageCanceled(logger, null);
        }

        public static void ErrorSendingMessage(this ILogger logger, Exception exception)
        {
            _errorSendingMessage(logger, exception);
        }

        public static void SendCanceled(this ILogger logger)
        {
            _sendCanceled(logger, null);
        }

        public static void SendStopped(this ILogger logger)
        {
            _sendStopped(logger, null);
        }

        public static void ClosingWebSocket(this ILogger logger)
        {
            _closingWebSocket(logger, null);
        }

        public static void ClosingWebSocketFailed(this ILogger logger, Exception exception)
        {
            _closingWebSocketFailed(logger, exception);
        }

        public static void CancelMessage(this ILogger logger)
        {
            _cancelMessage(logger, null);
        }

        public static void SendingMessages(this ILogger logger, int count, Uri url)
        {
            _sendingMessages(logger, count, url, null);
        }

        public static void SentSuccessfully(this ILogger logger)
        {
            _sentSuccessfully(logger, null);
        }

        public static void NoMessages(this ILogger logger)
        {
            _noMessages(logger, null);
        }

        public static void ErrorSending(this ILogger logger, Uri url, Exception exception)
        {
            _errorSending(logger, url, exception);
        }

        public static void EventStreamEnded(this ILogger logger)
        {
            _eventStreamEnded(logger, null);
        }

        public static void ClosingConnection(this ILogger logger)
        {
            _closingConnection(logger, null);
        }

        public static void ReceivedMessages(this ILogger logger)
        {
            _receivedMessages(logger, null);
        }

        public static void ErrorPolling(this ILogger logger, Uri pollUrl, Exception exception)
        {
            _errorPolling(logger, pollUrl, exception);
        }

        public static void HttpConnectionStarting(this ILogger logger)
        {
            _httpConnectionStarting(logger, null);
        }

        public static void HttpConnectionClosed(this ILogger logger)
        {
            _httpConnectionClosed(logger, null);
        }

        public static void StartingTransport(this ILogger logger, string connectionId, string transport, Uri url)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _startingTransport(logger, DateTime.Now, connectionId, transport, url, null);
            }
        }

        public static void RaiseConnected(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _raiseConnected(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void ProcessRemainingMessages(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _processRemainingMessages(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void DrainEvents(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _drainEvents(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void CompleteClosed(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _completeClosed(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void EstablishingConnection(this ILogger logger, Uri url)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _establishingConnection(logger, DateTime.Now, url, null);
            }
        }

        public static void ErrorWithNegotiation(this ILogger logger, Uri url, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _errorWithNegotiation(logger, DateTime.Now, url, exception);
            }
        }

        public static void ErrorStartingTransport(this ILogger logger, string connectionId, string transport, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _errorStartingTransport(logger, DateTime.Now, connectionId, transport, exception);
            }
        }

        public static void HttpReceiveStarted(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                _httpReceiveStarted(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void SkipRaisingReceiveEvent(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _skipRaisingReceiveEvent(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void ScheduleReceiveEvent(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _scheduleReceiveEvent(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void RaiseReceiveEvent(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _raiseReceiveEvent(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void FailedReadingMessage(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _failedReadingMessage(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void ErrorReceiving(this ILogger logger, string connectionId, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _errorReceiving(logger, DateTime.Now, connectionId, exception);
            }
        }

        public static void EndReceive(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                _endReceive(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void SendingMessage(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _sendingMessage(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void AbortingClient(this ILogger logger, string connectionId, Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _abortingClient(logger, DateTime.Now, connectionId, ex);
            }
        }

        public static void StoppingClient(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _stoppingClient(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void DisposingClient(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _disposingClient(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void SkippingDispose(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _skippingDispose(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void ConnectionStateChanged(this ILogger logger, string connectionId, HttpConnection.ConnectionState previousState, HttpConnection.ConnectionState newState)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _connectionStateChanged(logger, DateTime.Now, connectionId, previousState.ToString(), newState.ToString(), null);
            }
        }

        public static void SkippingStop(this ILogger logger, string connectionId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _skippingStop(logger, DateTime.Now, connectionId, null);
            }
        }

        public static void ExceptionThrownFromCallback(this ILogger logger, string connectionId, string callbackName, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _exceptionThrownFromCallback(logger, DateTime.Now, connectionId, callbackName, exception);
            }
        }

        public static void ErrorDuringClosedEvent(this ILogger logger, Exception exception)
        {
            _errorDuringClosedEvent(logger, exception);
        }
    }
}
