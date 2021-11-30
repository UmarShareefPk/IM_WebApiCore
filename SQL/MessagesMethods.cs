using IM.Common;
using IM.Models;
using IM_Core.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace IM.SQL
{
    public class MessagesMethods
    {
           
        public static object AddMessage(string From, string To, string MessageText)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "From" , From },
                  { "To" , To },
                  { "MessageText" , MessageText },                
            };

            var dbResponse =  DataAccessMethods.ExecuteProcedure("AddMessage", parameters);

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

        public static object GetMessagesByUser(string UserId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "UserId" , UserId },
            };

            var dbResponse = DataAccessMethods.ExecuteProcedure("GetMessagesByUser", parameters);

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