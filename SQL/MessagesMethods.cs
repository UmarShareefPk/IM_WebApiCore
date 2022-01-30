using IM.Common;
using IM.Models;
using IM_Core.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IM.SQL
{
    public class MessagesMethods
    {
        private readonly DataAccessMethods dbAccess;

        public MessagesMethods(DataAccessMethods dataAccessMethods)
        {
            dbAccess = dataAccessMethods;
        }
        public async Task<object> AddMessageAsync(string From, string To, string MessageText)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "From" , From },
                  { "To" , To },
                  { "MessageText" , MessageText },                
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("AddMessage", parameters);

            if (dbResponse.Error)
                return dbResponse;

            return (from rw in dbResponse.Ds.Tables[0].AsEnumerable()
                         select new Message()
                         {
                             Id = rw["Id"].ToString(),
                             From = rw["From"].ToString(),
                             To = rw["To"].ToString(),
                             Date = DateTime.Parse(rw["Date"].ToString()),
                             MessageText = rw["MessageText"].ToString(),
                             Status = rw["Status"].ToString(),
                             Deleted = bool.Parse(rw["Deleted"].ToString()),
                         }).First();
        }


        public async Task<object> GetConversationsByUserAsync(string UserId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "UserId" , UserId },
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetConversationsByUser", parameters);

            if (dbResponse.Error)
                return dbResponse;

            return (from rw in dbResponse.Ds.Tables[0].AsEnumerable()
                    select new Conversation()
                    {
                        Id = rw["Id"].ToString(),
                        User1 = rw["User1"].ToString(),
                        User2 = rw["User2"].ToString(),
                        LastMessageTime = DateTime.Parse(rw["LastMessageTime"].ToString()),
                        LastMessage = rw["LastMessage"].ToString(),   
                        UnReadCount = int.Parse(rw["UnreadCount"].ToString())
                    }).ToList();
        }

        public async Task<object> GetMessagesByConversationsAsync(string ConversationId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "ConversationId" , ConversationId },
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetMessagesByConversation", parameters);

            if (dbResponse.Error)
                return dbResponse;

            return (from rw in dbResponse.Ds.Tables[0].AsEnumerable()
                    select new Message()
                    {
                        Id = rw["Id"].ToString(),
                        From = rw["From"].ToString(),
                        To = rw["To"].ToString(),
                        Date = DateTime.Parse(rw["Date"].ToString()),
                        MessageText = rw["MessageText"].ToString(),
                        Status = rw["Status"].ToString(),
                        Deleted = bool.Parse(rw["Deleted"].ToString()),
                        ConversationId = rw["ConversationId"].ToString()
                    }).ToList();
        }

        public async Task<object> GetMessagesByUserAsync(string UserId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "UserId" , UserId },
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetMessagesByUser", parameters);

            if (dbResponse.Error)
                return dbResponse;

            return (from rw in dbResponse.Ds.Tables[0].AsEnumerable()
                    select new Message()
                    {
                        Id = rw["Id"].ToString(),
                        From = rw["From"].ToString(),
                        To = rw["To"].ToString(),
                        Date = DateTime.Parse(rw["Date"].ToString()),
                        MessageText = rw["MessageText"].ToString(),
                        Status = rw["Status"].ToString(),
                        Deleted = bool.Parse(rw["Deleted"].ToString()),
                    }).ToList();
        }

     
        
    } // end of class

   

} //end of namespace