// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace SocketsSample.Hubs
{
    public class Chat : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.InvokeAsync("Send", $"{Context.ConnectionId} joined");
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.Others.InvokeAsync("Send", $"{Context.ConnectionId} left");
        }

        public Task Send(string message)
        {
            return Clients.All.InvokeAsync("Send", $"{Context.ConnectionId}: {message}");
        }

        public Task SendToOthers(string message)
        {
            return Clients.Others.InvokeAsync("Send", $"{Context.ConnectionId}: {message}");
        }

        public Task SendToConnection(string connectionId, string message)
        {
            return Clients.Client(connectionId).InvokeAsync("Send", $"Private message from {Context.ConnectionId}: {message}");
        }

        public Task SendToGroup(string groupName, string message)
        {
            return Clients.Group(groupName).InvokeAsync("Send", $"{Context.ConnectionId}@{groupName}: {message}");
        }

        public Task SendToOthersInGroup(string groupName, string message)
        {
            return Clients.OthersInGroup(groupName).InvokeAsync("Send", $"{Context.ConnectionId}@{groupName}: {message}");
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).InvokeAsync("Send", $"{Context.ConnectionId} joined {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Clients.Group(groupName).InvokeAsync("Send", $"{Context.ConnectionId} left {groupName}");

            await Groups.RemoveAsync(Context.ConnectionId, groupName);
        }

        public Task Echo(string message)
        {
            return Clients.Caller.InvokeAsync("Send", $"{Context.ConnectionId}: {message}");
        }
    }
}
