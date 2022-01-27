using System;
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

        public void SendIncidentUpdate(string incidentId , string userId)
        {
            List<string> hubIds = _usersMethods.GetHubIds(incidentId , userId);
            foreach(string id in hubIds)
            {
                Clients.Client(id).SendAsync("UpdateNotifications", incidentId);
            }
        }


        public void SendMessage(string conversationId, string userId)
        {
            string hubId = _usersMethods.GetHubIdByUserId(userId);

            Clients.Client(hubId).SendAsync("UpdateConversation", conversationId);

        }

    }
}
