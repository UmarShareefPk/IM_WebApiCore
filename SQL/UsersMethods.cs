using IM.Common;
using IM.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace IM.SQL
{
    public class UsersMethods
    {
        private readonly DataAccessMethods dbAccess;

        public UsersMethods(DataAccessMethods dataAccessMethods)
        {
            dbAccess = dataAccessMethods;
        }
        public UserLogin Login(string username, string password)
        {
            var loginTable = new DataTable();
            var userTable = new DataTable();

            var parameters = new SortedList<string, object>()
            {
                  { "Username" , username },
                  { "Password" , password },
            };

            var dbResponse = dbAccess.ExecuteProcedure("Login", parameters);
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

        public UserLogin Authenticate(AuthenticateRequest model)
        {
            var userLogin = Login(model.Username, model.Password);
            // authentication successful so generate jwt token
            if (userLogin != null)
                userLogin.Token = JWT.generateJwtToken(userLogin, "This is very secret.");

            return userLogin;
        }

        public  User GetUserById(string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                  { "UserId" , userId },
            };

            var dbResponse = dbAccess.ExecuteProcedure("UserById", parameters);
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

        public  DbResponse UpdateHubId(string userId, string hubId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                  { "UserId" , userId },
                  { "HubId" , hubId },
            };

            var dbResponse = dbAccess.ExecuteProcedure("UpdateHubId", parameters);
            return dbResponse;
        }

        public List<User> GetAllUsers()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            { };

            var dbResponse = dbAccess.ExecuteProcedure("GetAllUsers", parameters);
            if(dbResponse.Error)
            {
                var users = new List<User>();
                users.Add(new User
                                {
                                    Id= dbResponse.ErrorMsg,
                                    CreateDate = DateTime.Now,
                                    FirstName = dbResponse.ErrorMsg,
                                    LastName = dbResponse.ErrorMsg,
                                    ProfilePic = dbResponse.ErrorMsg,
                                    Email = dbResponse.ErrorMsg,
                                    Phone = dbResponse.ErrorMsg,
                                }
                );
                return users;
            }

            dt = dbResponse.Ds.Tables[0];

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
                             HubId = rw["HubId"].ToString(),
                         }).ToList();

            return Users;
        }

        public  DbResponse AddUser(User user)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "FirstName" , user.FirstName },
                  { "LastName" , user.LastName },
                  { "Email" , user.Email },
                  { "ProfilePic" , user.ProfilePic },
                  { "Phone" , user.Phone }
            };

            return dbAccess.ExecuteProcedure("AddNewUser", parameters);
        }

        public  UsersWithPage GetUsersPage(int pageSize, int pageNumber, string sortBy, string sortDirection, string Serach)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "PageSize" , pageSize},
                 { "PageNumber" , pageNumber},
                 { "SortBy" , sortBy},
                 { "SortDirection" , sortDirection},
                 { "SearchText" , Serach},
            };

            var dbResponse = dbAccess.ExecuteProcedure("GetUsersPage", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            dt = ds.Tables[1];
            int Total_Users = int.Parse(ds.Tables[0].Rows[0][0].ToString());

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

            return new UsersWithPage
            {
                Total_Users = Total_Users,
                Users = Users
            };
        }

        public  List<string> GetHubIds(string incidentId, string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "IncidentId" , incidentId}
            };
            var dbResponse = dbAccess.ExecuteProcedure("GetHubIdByIncident", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0 )
                return new List<string>();

            var hubIds = (from rw in ds.Tables[0].AsEnumerable()
                          where rw["userId"].ToString() != userId
                                 select rw["HubId"].ToString()).ToList();

            return hubIds;
        }

        public string GetHubIdByUserId(string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "IncidentId" , userId}
            };
            var dbResponse = dbAccess.ExecuteProcedure("GetHubIdByUserId", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;


            return ds.Tables[0].Rows[0][0].ToString();
        }


        public DbResponse UpdateIsRead(string notificationId , bool isRead)
        {
           
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "Id" , notificationId},
                 { "IsRead" , isRead}
            };
            var dbResponse = dbAccess.ExecuteProcedure("markReadUnread", parameters);            
            return dbResponse;           
        }
        public List<IncidentNotification> GetUserNotifications(string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "UserId" , userId}                
            };

            var dbResponse = dbAccess.ExecuteProcedure("GetUserNotifications", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            dt = ds.Tables[0];
     

            var notifications = (from rw in dt.AsEnumerable()
                         select new IncidentNotification()
                         {
                             Id = rw["Id"].ToString(),
                             IncidentId = rw["IncidentId"].ToString(),
                             SourceUserId = rw["SourceUserId"].ToString(),
                             IsRead = bool.Parse(rw["IsRead"].ToString()),
                             ReadDate = rw.IsNull("ReadDate") ? DateTime.Now :  DateTime.Parse(rw["ReadDate"].ToString()),
                             CreateDate = DateTime.Parse(rw["CreateDate"].ToString()),
                             UserId = rw["UserId"].ToString(),
                             NotifyAbout = rw["NotifyAbout"].ToString()
                         }).ToList();

            return notifications;

        }
    } // end of class

    public class UsersWithPage
    {
        public int Total_Users { get; set; }
        public List<User> Users { get; set; }
    }

} //end of namespace