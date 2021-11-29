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
        
        public static UserLogin Login(string username, string password)
        {
            var loginTable = new DataTable();
            var userTable = new DataTable();

            var parameters = new SortedList<string, object>()
            {
                  { "Username" , username },
                  { "Password" , password },
            };

            var dbResponse = DataAccessMethods.ExecuteProcedure("Login", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            loginTable = ds.Tables[0];
            userTable = ds.Tables[1];

            var Users = (from rw in userTable.AsEnumerable()
                         select new User()
                         {
                             Id = rw["Id"].ToString(),
                             CreateDate = DateTime.Parse(rw["CreateDate"].ToString()),
                             FirstName = rw["FirstName"].ToString(),
                             LastName = rw["LastName"].ToString(),
                             ProfilePic = rw["ProfilePic"].ToString(),
                             Email = rw["Email"].ToString(),
                             Phone = rw["Phone"].ToString(),
                         }).ToList();


            var userLogin = (from rw in loginTable.AsEnumerable()
                             select new UserLogin()
                             {
                                 Id = rw["Id"].ToString(),
                                 user = Users.First(),
                                 Username = rw["Username"].ToString(),
                                 Password = rw["Password"].ToString(),
                                 CreateDate = DateTime.Parse(rw["CreateDate"].ToString()),
                                 LastLogin = DateTime.Now // DateTime.Parse(rw["LastLogin"].ToString())
                             }).ToList();

            return userLogin.First();
        }

        public static UserLogin Authenticate(AuthenticateRequest model)
        {
            var userLogin = Login(model.Username, model.Password);
            // authentication successful so generate jwt token
            if (userLogin != null)
                userLogin.Token = JWT.generateJwtToken(userLogin, "This is very secret.");

            return userLogin;
        }

        public static User GetUserById(string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                  { "UserId" , userId },
            };

            var dbResponse = DataAccessMethods.ExecuteProcedure("UserById", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            dt = ds.Tables[0];

            var Users = (from rw in dt.AsEnumerable()
                         select new User()
                         {
                             Id = rw["Id"].ToString(),
                             CreateDate = DateTime.Parse(rw["CreateDate"].ToString()),
                             FirstName = rw["FirstName"].ToString(),
                             LastName = rw["LastName"].ToString(),
                             ProfilePic = rw["ProfilePic"].ToString(),
                             Email = rw["Email"].ToString(),
                             Phone = rw["Phone"].ToString(),
                         }).ToList();

            return Users.First();
        }

  
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