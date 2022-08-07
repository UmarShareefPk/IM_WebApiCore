﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using IM.SQL;

namespace IM.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly UsersMethods _usersMethods;
        public NotificationHub(UsersMethods usersMethods)
        {
            _usersMethods = usersMethods;
        }
        public void Send( string message)
        {
            string s = "iGD7rER_rNOT44SNeTLbfA";
            List<string> listData = new List<string>();
            listData.Add(s);
            s = "Mm65FIntNoSYFzhtvvXmuw";
            listData.Add(s);
            IReadOnlyList<string> readOnlyData = listData.AsReadOnly();
            // Call the broadcastMessage method to update clients.
            Clients.Client(s).SendAsync("ReceiveMessage", message + " | " + DateTime.Now);
           //Clients.All.SendAsync("ReceiveMessage", message + " | " + DateTime.Now);

        }

        public async Task SendIncidentUpdate(string incidentId , string userId)
        {
            List<string> hubIds = await _usersMethods.GetHubIdsAsync(incidentId , userId);

            await _usersMethods.LogSignalRAsync("Hub function called");

            foreach(string id in hubIds)
            {
                try
                {
                    await Clients.Client(id).SendAsync("UpdateNotifications", incidentId);
                    await _usersMethods.LogSignalRAsync("No Error");
                }
                catch(Exception ex)
                {
                    await _usersMethods.LogSignalRAsync("Failed:" + ex.Message);
                }
            }
        }


        public async Task SendMessageAsync(string conversationId, string userId, object newMessage, bool isNewConversation)
        {
            string hubId = await _usersMethods.GetHubIdByUserIdAsync(userId);

            if (isNewConversation)
            {
                await Clients.Client(hubId).SendAsync("ReceiveNewConversation", newMessage);
            }            
            else
            {
                await Clients.Client(hubId).SendAsync("ReceiveNewMessage", newMessage);
            }                

        }

    }
}
