using IM.Common;
using IM.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IM.SQL
{
    public class UsersMethods
    {
        private readonly DataAccessMethods dbAccess;
        private IConfiguration _configuration;

        public UsersMethods(DataAccessMethods dataAccessMethods, IConfiguration configuration)
        {
            dbAccess = dataAccessMethods;
            _configuration = configuration;
        }
        public async Task<UserLogin> LoginAsync(string username, string password)
        {
            var loginTable = new DataTable();
            var userTable = new DataTable();

            var parameters = new SortedList<string, object>()
            {
                  { "Username" , username },
                  { "Password" , password },
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("Login", parameters);
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
                             }).ToList().First();

            userLogin.UnreadConversationCount = int.Parse(ds.Tables[2].Rows[0][0].ToString());

            return userLogin;
        }

        public async Task<UserLogin> AuthenticateAsync(AuthenticateRequest model)
        {
            var userLogin = await LoginAsync(model.Username, model.Password);
            // authentication successful so generate jwt token
            if (userLogin != null)
                userLogin.Token = JWT.generateJwtToken(userLogin, _configuration.GetValue<string>("JwtSecret"));

            return userLogin;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                  { "UserId" , userId },
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("UserById", parameters);
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

        public async Task<DbResponse> UpdateHubIdAsync(string userId, string hubId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                  { "UserId" , userId },
                  { "HubId" , hubId },
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("UpdateHubId", parameters);
            return dbResponse;
        }

        public async Task<DbResponse> LogSignalRAsync(string msg)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                  { "msg" , msg },
             };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("LogSignalR", parameters);
            return dbResponse;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            { };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetAllUsers", parameters);
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

            //   var ss = dt.AsEnumerable().Where(dr => dr["FirstName"].ToString() == "Umar");
            var ss = dt.AsEnumerable().Where((dr) => { 
                                                    if (dr["FirstName"].ToString() == "Umar") 
                                                        return true; 
                                                    else
                                                 return false;
            }
            );

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

        public async Task<DbResponse> AddUserAsync(User user)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "FirstName" , user.FirstName },
                  { "LastName" , user.LastName },
                  { "Email" , user.Email },
                  { "ProfilePic" , user.ProfilePic },
                  { "Phone" , user.Phone }
            };

            return await dbAccess.ExecuteProcedureAsync("AddNewUser", parameters);
        }

        public async Task<UsersWithPage> GetUsersPageAsync(int pageSize, int pageNumber, string sortBy, string sortDirection, string Serach)
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

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetUsersPage", parameters);
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

        public async Task<List<string>> GetHubIdsAsync(string incidentId, string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "IncidentId" , incidentId}
            };
            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetHubIdByIncident", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0 )
                return new List<string>();

            var hubIds = (from rw in ds.Tables[0].AsEnumerable()
                          where rw["userId"].ToString() != userId
                                 select rw["HubId"].ToString()).ToList();

            return hubIds;
        }

        public async Task<string> GetHubIdByUserIdAsync(string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "UserId" , userId}
            };
            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetHubIdByUserId", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;


            return ds.Tables[0].Rows[0][0].ToString();
        }


        public async Task<DbResponse> UpdateIsReadAsync(string notificationId , bool isRead)
        {
           
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "Id" , notificationId},
                 { "IsRead" , isRead}
            };
            var dbResponse = await dbAccess.ExecuteProcedureAsync("markReadUnread", parameters);            
            return dbResponse;           
        }
        public async Task<List<IncidentNotification>> GetUserNotificationsAsync(string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "UserId" , userId}                
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetUserNotifications", parameters);
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