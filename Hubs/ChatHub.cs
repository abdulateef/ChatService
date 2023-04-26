using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connections;
        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            _botUser = "MyChat bot";
            _connections = connections;
        }
        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.room);
            _connections[Context.ConnectionId] = userConnection;
            await Clients.Group(userConnection.room).SendAsync("Recievemessage", _botUser, $"{userConnection.user} has joined the {userConnection.room}");
            await SendConnectedUsers(userConnection.room);
        }

        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.room).SendAsync("Recievemessage", userConnection.user, message);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.room)
                    .SendAsync("RecieveMessage", _botUser, $"{userConnection.user} has left");
            }

             SendConnectedUsers(userConnection.room);

            return base.OnDisconnectedAsync(exception);

        }

        public Task SendConnectedUsers(string room)
        {
            var users = _connections.Values
                .Where(c => c.room == room)
                .Select(c => c.user);
            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }
    }
}
