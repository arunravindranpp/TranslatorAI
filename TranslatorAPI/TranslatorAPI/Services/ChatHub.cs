using Microsoft.AspNetCore.SignalR;
using System.Data;
using TranslatorAPI.Helpers;
using TranslatorAPI.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;

namespace TranslatorAPI.Services
{
    public class ChatHub : Hub
    {
        private readonly string _connectionString;
        private static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();
        private readonly string [] _users = { "user1", "user2", "user3", "user4" ,"user5","user6"};
        public ChatHub(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PACEDatabase");
        }

        // Send a message to a specific recipient (private message) or broadcast to all (public message)
        public async Task SendMessage(string sender, string? recipient, string message)
        {
            var chatMessage = new Message
            {
                Sender = sender ?? "Anonymous",
                Receiver = recipient,
                MessageText = message,
                Timestamp = DateTime.Now
            };

            try
            {
                using (var sqlCommand = new SqlCommand())
                {
                    sqlCommand.CommandText = @"Insert into TranslatorMessages(Sender, Receiver, MessageText,TimeStamp)
                                               values (@Sender, @Receiver, @MessageText,@TimeStamp)";

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Sender", chatMessage.Sender),
                        new SqlParameter("@Receiver", chatMessage.Receiver ?? (object)DBNull.Value),
                        new SqlParameter("@MessageText", chatMessage.MessageText),
                        new SqlParameter("@TimeStamp", chatMessage.Timestamp)
                    };

                    EYSql.ExecuteNonQuery(_connectionString, CommandType.Text, sqlCommand.CommandText, parameters);
                }
            }
            catch (Exception ex)
            {
                // Do nothing for now
            }

            // If there's a recipient, send it to them directly. Otherwise, send to all users.
            if (string.IsNullOrEmpty(recipient))
            {
                await Clients.All.SendAsync("ReceiveMessage", sender, message, DateTime.Now);
            }
            else
            {
                if (_userConnections.TryGetValue(recipient, out var recipientConnectionId))
                {
                    await Clients.Client(recipientConnectionId).SendAsync("ReceiveMessage", sender, message, DateTime.Now);
                }
            }
        }
        public async Task<List<string>> GetActiveUsers()
        {
            var activeusers = _userConnections.Keys.ToList();
            //activeusers.AddRange(_users.Where(user => !activeusers.Contains(user)));
            return activeusers;
        }
        public override async Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext()?.Request.Query["username"];

            if (!string.IsNullOrEmpty(username))
            {
                _userConnections[username] = Context.ConnectionId; // Add user to dictionary
                var activeusers = _userConnections.Keys.ToList();
                //activeusers.AddRange(_users.Where(user => !activeusers.Contains(user)));
                // Notify all clients about the updated user list
                await Clients.All.SendAsync("UpdateUserList", activeusers);
            }

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (!string.IsNullOrEmpty(username))
            {
                _userConnections.TryRemove(username, out _); // Remove user from dictionary

                // Notify all clients about the updated user list
                await Clients.All.SendAsync("UpdateUserList", _userConnections.Keys.ToList());
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
