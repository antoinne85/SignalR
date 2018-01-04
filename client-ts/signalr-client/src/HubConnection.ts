// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

import { ConnectionClosed } from "./Common"
import { IConnection } from "./IConnection"
import { HttpConnection } from "./HttpConnection"
import { TransportType, TransferMode } from "./Transports"
import { Subject, Observable } from "./Observable"
import { IHubProtocol, ProtocolType, MessageType, HubMessage, CompletionMessage, ResultMessage, InvocationMessage, StreamInvocationMessage, NegotiationMessage } from "./IHubProtocol";
import { JsonHubProtocol, JSON_HUB_PROTOCOL_NAME } from "./JsonHubProtocol";
import { TextMessageFormat } from "./Formatters"
import { Base64EncodedHubProtocol } from "./Base64EncodedHubProtocol"
import { ILogger, LogLevel } from "./ILogger"
import { ConsoleLogger, NullLogger, LoggerFactory } from "./Loggers"
import { IHubConnectionOptions } from "./IHubConnectionOptions"

export { JsonHubProtocol }

const DEFAULT_TIMEOUT_IN_MS: number = 30 * 1000;

const browserWindow: any = <any>window;

interface HubProtocolConstructor {
    new (): IHubProtocol;
}

function resolveProtocol(requestedProtocol: string | IHubProtocol, logger: ILogger): IHubProtocol {
    if(!requestedProtocol) {
        logger.log(LogLevel.Trace, "No protocol requested, using default JsonHubProtocol.");
        return new JsonHubProtocol();
    }
    else if(typeof requestedProtocol === "string") {
        logger.log(LogLevel.Trace, "Protocol name requested, looking for implementation");

        // Special-case the built-in JSON protocol
        if(requestedProtocol === JSON_HUB_PROTOCOL_NAME) {
            logger.log(LogLevel.Trace, "Built-in JSON protocol requested by name.");
            return new JsonHubProtocol();
        }

        // If we're in the browser, resolve off of window.signalR.protocols;
        let constructor: HubProtocolConstructor = null;
        if(browserWindow) {
            if(browserWindow.signalR && browserWindow.signalR.protocols) {
                let candidate = browserWindow.signalR.protocols[requestedProtocol];
                if(typeof candidate !== "function") {
                    throw new Error(`Value 'signalR.protocols.${requestedProtocol}' is not a Function`);
                }
                constructor = candidate;
            }
            if(constructor) {
                logger.log(LogLevel.Trace, `Located protocol constructor: signalR.protocols.${requestedProtocol}`);
            }
        }
        else if(require) {
            // We're in NodeJS, we have require
            let moduleName = `signalr-protocol-${requestedProtocol}`;
            let candidate = require(moduleName);
            if(typeof candidate !== "function") {
                throw new Error(`Module '${moduleName}' does not export a Function`);
            }
            constructor = candidate;
        }
        else {
            throw new Error(`Unknown environment. Cannot resolve protocol: '${requestedProtocol}'`)
        }

        if(!constructor) {
            throw new Error(`Unable to locate requested protocol: '${requestedProtocol}'`);
        } else {
            return new constructor();
        }
    }
    else {
        logger.log(LogLevel.Trace, "IHubProtocol instance provided, using it");
        return requestedProtocol;
    }
}

export class HubConnection {
    private readonly connection: IConnection;
    private readonly logger: ILogger;
    private protocol: IHubProtocol;
    private callbacks: Map<string, (invocationEvent: HubMessage, error?: Error) => void>;
    private methods: Map<string, ((...args: any[]) => void)[]>;
    private id: number;
    private closedCallbacks: ConnectionClosed[];
    private timeoutHandle: NodeJS.Timer;
    private timeoutInMilliseconds: number;

    constructor(url: string, options?: IHubConnectionOptions);
    constructor(connection: IConnection, options?: IHubConnectionOptions);
    constructor(urlOrConnection: string | IConnection, options: IHubConnectionOptions = {}) {
        options = options || {};

        this.timeoutInMilliseconds = options.timeoutInMilliseconds || DEFAULT_TIMEOUT_IN_MS;

        if (typeof urlOrConnection === "string") {
            this.connection = new HttpConnection(urlOrConnection, options);
        }
        else {
            this.connection = urlOrConnection;
        }

        this.logger = LoggerFactory.createLogger(options.logger);

        this.protocol = resolveProtocol(options.protocol, this.logger);
        this.connection.onreceive = (data: any) => this.processIncomingData(data);
        this.connection.onclose = (error?: Error) => this.connectionClosed(error);

        this.callbacks = new Map<string, (invocationEvent: HubMessage, error?: Error) => void>();
        this.methods = new Map<string, ((...args: any[]) => void)[]>();
        this.closedCallbacks = [];
        this.id = 0;
    }

    private processIncomingData(data: any) {
        if (this.timeoutHandle !== undefined) {
            clearTimeout(this.timeoutHandle);
        }

        // Parse the messages
        let messages = this.protocol.parseMessages(data);

        for (var i = 0; i < messages.length; ++i) {
            var message = messages[i];

            switch (message.type) {
                case MessageType.Invocation:
                    this.invokeClientMethod(<InvocationMessage>message);
                    break;
                case MessageType.StreamItem:
                case MessageType.Completion:
                    let callback = this.callbacks.get((<any>message).invocationId);
                    if (callback != null) {
                        if (message.type === MessageType.Completion) {
                            this.callbacks.delete((<any>message).invocationId);
                        }
                        callback(message);
                    }
                    break;
                case MessageType.Ping:
                    // Don't care about pings
                    break;
                default:
                    this.logger.log(LogLevel.Warning, "Invalid message type: " + data);
                    break;
            }
        }

        this.configureTimeout();
    }

    private configureTimeout() {
        if (!this.connection.features || !this.connection.features.inherentKeepAlive) {
            // Set the timeout timer
            this.timeoutHandle = setTimeout(() => this.serverTimeout(), this.timeoutInMilliseconds);
        }
    }

    private serverTimeout() {
        // The server hasn't talked to us in a while. It doesn't like us anymore ... :(
        // Terminate the connection
        this.connection.stop(new Error("Server timeout elapsed without receiving a message from the server."));
    }

    private invokeClientMethod(invocationMessage: InvocationMessage) {
        let methods = this.methods.get(invocationMessage.target.toLowerCase());
        if (methods) {
            methods.forEach(m => m.apply(this, invocationMessage.arguments));
            if (invocationMessage.invocationId) {
                // This is not supported in v1. So we return an error to avoid blocking the server waiting for the response.
                let message = "Server requested a response, which is not supported in this version of the client."
                this.logger.log(LogLevel.Error, message);
                this.connection.stop(new Error(message))
            }
        }
        else {
            this.logger.log(LogLevel.Warning, `No client method with the name '${invocationMessage.target}' found.`);
        }
    }

    private connectionClosed(error?: Error) {
        this.callbacks.forEach(callback => {
            callback(undefined, error ? error : new Error("Invocation canceled due to connection being closed."));
        });
        this.callbacks.clear();

        this.closedCallbacks.forEach(c => c.apply(this, [error]));
    }

    async start(): Promise<void> {
        let requestedTransferMode =
            (this.protocol.type === ProtocolType.Binary)
                ? TransferMode.Binary
                : TransferMode.Text;

        this.connection.features.transferMode = requestedTransferMode
        await this.connection.start();
        var actualTransferMode = this.connection.features.transferMode;

        await this.connection.send(
            TextMessageFormat.write(
                JSON.stringify(<NegotiationMessage>{ protocol: this.protocol.name })));

        this.logger.log(LogLevel.Information, `Using HubProtocol '${this.protocol.name}'.`);

        if (requestedTransferMode === TransferMode.Binary && actualTransferMode === TransferMode.Text) {
            this.protocol = new Base64EncodedHubProtocol(this.protocol);
        }

        this.configureTimeout();
    }

    stop(): Promise<void> {
        if (this.timeoutHandle) {
            clearTimeout(this.timeoutHandle);
        }
        return this.connection.stop();
    }

    stream<T>(methodName: string, ...args: any[]): Observable<T> {
        let invocationDescriptor = this.createStreamInvocation(methodName, args);

        let subject = new Subject<T>();

        this.callbacks.set(invocationDescriptor.invocationId, (invocationEvent: HubMessage, error?: Error) => {
            if (error) {
                subject.error(error);
                return;
            }

            if (invocationEvent.type === MessageType.Completion) {
                let completionMessage = <CompletionMessage>invocationEvent;
                if (completionMessage.error) {
                    subject.error(new Error(completionMessage.error));
                }
                else {
                    subject.complete();
                }
            }
            else {
                subject.next(<T>(<ResultMessage>invocationEvent).item);
            }
        });

        let message = this.protocol.writeMessage(invocationDescriptor);

        this.connection.send(message)
            .catch(e => {
                subject.error(e);
                this.callbacks.delete(invocationDescriptor.invocationId);
            });

        return subject;
    }

    send(methodName: string, ...args: any[]): Promise<void> {
        let invocationDescriptor = this.createInvocation(methodName, args, true);

        let message = this.protocol.writeMessage(invocationDescriptor);

        return this.connection.send(message);
    }

    invoke(methodName: string, ...args: any[]): Promise<any> {
        let invocationDescriptor = this.createInvocation(methodName, args, false);

        let p = new Promise<any>((resolve, reject) => {
            this.callbacks.set(invocationDescriptor.invocationId, (invocationEvent: HubMessage, error?: Error) => {
                if (error) {
                    reject(error);
                    return;
                }
                if (invocationEvent.type === MessageType.Completion) {
                    let completionMessage = <CompletionMessage>invocationEvent;
                    if (completionMessage.error) {
                        reject(new Error(completionMessage.error));
                    }
                    else {
                        resolve(completionMessage.result);
                    }
                }
                else {
                    reject(new Error(`Unexpected message type: ${invocationEvent.type}`));
                }
            });

            let message = this.protocol.writeMessage(invocationDescriptor);

            this.connection.send(message)
                .catch(e => {
                    reject(e);
                    this.callbacks.delete(invocationDescriptor.invocationId);
                });
        });

        return p;
    }

    on(methodName: string, method: (...args: any[]) => void) {
        if (!methodName || !method) {
            return;
        }

        methodName = methodName.toLowerCase();
        if (!this.methods.has(methodName)) {
            this.methods.set(methodName, []);
        }

        this.methods.get(methodName).push(method);
    }

    off(methodName: string, method: (...args: any[]) => void) {
        if (!methodName || !method) {
            return;
        }

        methodName = methodName.toLowerCase();
        let handlers = this.methods.get(methodName);
        if (!handlers) {
            return;
        }
        var removeIdx = handlers.indexOf(method);
        if (removeIdx != -1) {
            handlers.splice(removeIdx, 1);
        }
    }

    onclose(callback: ConnectionClosed) {
        if (callback) {
            this.closedCallbacks.push(callback);
        }
    }

    private createInvocation(methodName: string, args: any[], nonblocking: boolean): InvocationMessage {
        if (nonblocking) {
            return <InvocationMessage>{
                type: MessageType.Invocation,
                target: methodName,
                arguments: args,
            };
        }
        else {
            let id = this.id;
            this.id++;

            return <InvocationMessage>{
                type: MessageType.Invocation,
                invocationId: id.toString(),
                target: methodName,
                arguments: args,
            };
        }
    }

    private createStreamInvocation(methodName: string, args: any[]): StreamInvocationMessage {
        let id = this.id;
        this.id++;

        return <StreamInvocationMessage>{
            type: MessageType.StreamInvocation,
            invocationId: id.toString(),
            target: methodName,
            arguments: args,
        };
    }
}
