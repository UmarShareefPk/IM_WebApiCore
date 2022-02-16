using IM.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace IM.SQL
{
    public class IncidentsMethods
    {
        private readonly DataAccessMethods dbAccess;

        public IncidentsMethods(DataAccessMethods dataAccessMethods)
        {
            dbAccess = dataAccessMethods;
        }
        public async Task<DbResponse> AddIncident(Incident incident)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "CreatedBy" , incident.CreatedBy },
                  { "AssignedTo" , incident.AssignedTo },
                  { "Title" , incident.Title },
                  { "Description" , incident.Description },
                  { "AdditionalData" , incident.AdditionalData },                 
                  { "StartTime" , incident.StartTime },
                  { "DueDate" , incident.DueDate },
                  { "Status" , incident.Status.ToUpper() },

            };
            return await dbAccess.ExecuteProcedureAsync("AddNewIncident", parameters);
        }

        public async Task<DbResponse> AddCommentAsync(Comment comment)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "IncidentId" , comment.IncidentId },
                  { "UserId" , comment.UserId },
                  { "CommentText" , comment.CommentText },
                 

            };
            return await dbAccess.ExecuteProcedureAsync("AddComment", parameters);
        }

        public async Task<DbResponse> AddIncidentAttachmentsAsync(IncidentAttachments incidentAttachments)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "FileName" , incidentAttachments.FileName },
                  { "ContentType" , incidentAttachments.ContentType },
                  { "IncidentId" , incidentAttachments.IncidentId }
            };
            return await dbAccess.ExecuteProcedureAsync("AddIncidentAttachment", parameters);
        }

        public async Task<DbResponse> AddCommentAttachmentsAsync(CommentAttachments commentAttachments)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "FileName" , commentAttachments.FileName },
                  { "ContentType" , commentAttachments.ContentType },
                  { "CommentId" , commentAttachments.CommentId }
            };
            var rr = await dbAccess.ExecuteProcedureAsync("AddCommentAttachment", parameters);
            return rr;
        }

        public async Task<List<IncidentAttachments>> GetIncidentAttachmentAsync(string incidentId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "IncidentId" , incidentId }  
            };
            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetAttachmentByIncidentId", parameters);


            var dt =  dbResponse.Ds.Tables[0];

            var attachments = (from rw in dt.AsEnumerable()
                             select new IncidentAttachments()
                             {
                                 Id = rw["Id"].ToString(),   
                                 FileName = rw["FileName"].ToString(),
                                 ContentType = rw["ContentType"].ToString(),
                                 IncidentId = rw["IncidentId"].ToString(),
                                 DateAdded = DateTime.Parse(rw["DateAdded"].ToString())  
                             }).ToList();

            return attachments;
        }

        public async Task<string> DeleteFileAsync(string type, string filetId, string userId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "Id" , filetId},
                  { "UserId" , userId},
            };

            var dbResponse = new DbResponse();
            if (type.ToLower() == "comment")
            {
                dbResponse = await dbAccess.ExecuteProcedureAsync("DeleteCommentAttachment", parameters);
            }
            else
            {
                dbResponse = await dbAccess.ExecuteProcedureAsync("DeleteIncidentAttachment", parameters);
            }
            var ds = dbResponse.Ds;

            if (dbResponse.Error)
                return dbResponse.ErrorMsg;
            else
                return "Success";

        }

        public async Task<Incident> GetIncidentrByIdAsync(string incidentId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "Id" , incidentId},
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetIncidentById", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            var incidentDt = new DataTable();
            var attachmentsDt = new DataTable(); 
            var commentsDt = new DataTable();
            var commentsAttachmentDt = new DataTable();
            var incidentLogsDt = new DataTable();

            incidentDt = ds.Tables[0];
            attachmentsDt = ds.Tables[1];
            commentsDt = ds.Tables[2];
            commentsAttachmentDt = ds.Tables[3];
            incidentLogsDt = ds.Tables[4];

            var attachments = (from rw in attachmentsDt.AsEnumerable()
                               select new IncidentAttachments()
                               {
                                   Id = rw["Id"].ToString(),
                                   DateAdded = DateTime.Parse(rw["DateAdded"].ToString()),
                                   FileName = rw["FileName"].ToString(),
                                   ContentType = rw["ContentType"].ToString(),
                                   IncidentId = rw["IncidentId"].ToString()
                               }).ToList();

            var comments = (from rw in commentsDt.AsEnumerable()
                               select new Comment()
                               {
                                   Id = rw["Id"].ToString(),
                                   CommentText = rw["Comment"].ToString(),
                                   CreateDate = DateTime.Parse(rw["CreateDate"].ToString()),
                                   UserId = rw["UserId"].ToString(),
                                   IncidentId = rw["IncidentId"].ToString(),
                                   attachments = (from row in commentsAttachmentDt.AsEnumerable()
                                                  where row["CommentId"].ToString() == rw["Id"].ToString()
                                                  select new CommentAttachments()
                                                  {
                                                      Id = row["Id"].ToString(),
                                                      DateAdded = DateTime.Parse(row["DateAdded"].ToString()),
                                                      FileName = row["FileName"].ToString(),
                                                      ContentType = row["ContentType"].ToString(),
                                                      CommentId = row["CommentId"].ToString()
                                                  }).ToList()
                               }).ToList();

            var logs = (from rw in incidentLogsDt.AsEnumerable()
                               select new IncidentLogs()
                               {
                                   Id = rw["Id"].ToString(),
                                   UpdateDate = DateTime.Parse(rw["UpdateDate"].ToString()),
                                   Value = rw["Value"].ToString(),
                                   OldValue = rw["OldValue"].ToString(),
                                   UserId = rw["UserId"].ToString(),
                                   IncidentId = rw["IncidentId"].ToString(),
                                   Parameter = rw["Parameter"].ToString(),
                               }).ToList();

            var incidents = (from rw in incidentDt.AsEnumerable()
                         select new Incident()
                         {
                             Id = rw["Id"].ToString(),
                             CreatedBy = rw["CreatedBy"].ToString(),
                             AssignedTo = rw["AssignedTo"].ToString(),
                             CreatedAT = DateTime.Parse(rw["CreatedAT"].ToString()),
                             Title = rw["Title"].ToString(),
                             Description = rw["Description"].ToString(),
                             AdditionalData = rw["AdditionalData"].ToString(),                             
                             StartTime = DateTime.Parse(rw["StartTime"].ToString()),
                             DueDate = DateTime.Parse(rw["DueDate"].ToString()),
                             Status = rw["Status"].ToString(),
                             Comments = comments,
                             Attachments = attachments,
                             Logs = logs
                         }).ToList();

            return incidents.First();
        }

        public async Task<Comment> GetCommentByIdAsync(string commentId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "CommentId" , commentId},
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetCommentById", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            var commentsDt = ds.Tables[0];
            var commentsAttachmentDt = ds.Tables[1];

            var comment = (from rw in commentsDt.AsEnumerable()
                            select new Comment()
                            {
                                Id = rw["Id"].ToString(),
                                CommentText = rw["Comment"].ToString(),
                                CreateDate = DateTime.Parse(rw["CreateDate"].ToString()),
                                UserId = rw["UserId"].ToString(),
                                IncidentId = rw["IncidentId"].ToString(),
                                attachments = (from row in commentsAttachmentDt.AsEnumerable()
                                               where row["CommentId"].ToString() == rw["Id"].ToString()
                                               select new CommentAttachments()
                                               {
                                                   Id = row["Id"].ToString(),
                                                   DateAdded = DateTime.Parse(row["DateAdded"].ToString()),
                                                   FileName = row["FileName"].ToString(),
                                                   ContentType = row["ContentType"].ToString(),
                                                   CommentId = row["CommentId"].ToString()
                                               }).ToList()
                            }).ToList().First();

            return comment;
        }

        public async Task DeleteCommentAsync(string commentId , string userId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "CommentId" , commentId},
                  { "UserId" , userId}
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("DeleteComment", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return;
        }

        public async Task<List<Incident>> GetAllIncidents()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetAllIncidents", parameters);
            var ds =  dbResponse.Ds;

         //   ds.Tables[0].AsEnumerable().Where(dr => dr.HasErrors);

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            dt = ds.Tables[0];

            var incidents = (from rw in dt.AsEnumerable()
                             select new Incident()
                             {
                                 Id = rw["Id"].ToString(),
                                 CreatedBy = rw["CreatedBy"].ToString(),
                                 AssignedTo = rw["AssignedTo"].ToString(),
                                 CreatedAT = DateTime.Parse(rw["CreatedAT"].ToString()),
                                 Title = rw["Title"].ToString(),
                                 Description = rw["Description"].ToString(),
                                 AdditionalData = rw["AdditionalData"].ToString(),
                                 StartTime = DateTime.Parse(rw["StartTime"].ToString()),
                                 DueDate = DateTime.Parse(rw["DueDate"].ToString()),
                                 Status = rw["Status"].ToString()


                             }).ToList();

            return incidents;
        }

        //public async Task<IncidentsWithPage> GetIncidentsPageAsync(int pageSize , int pageNumber, string sortBy, string sortDirection, string Serach)
        //{
        //    var dt = new DataTable();
        //    var parameters = new SortedList<string, object>()
        //    {
        //         { "PageSize" , pageSize},
        //         { "PageNumber" , pageNumber},
        //         { "SortBy" , sortBy},
        //         { "SortDirection" , sortDirection},
        //         { "SearchText" , Serach},
        //    };

        //    var dbResponse = await dbAccess.ExecuteProcedureAsync("GetIncidentsPage", parameters);
        //    var ds = dbResponse.Ds;

        //    if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //        return null;

        //    dt = ds.Tables[1];
        //    int total_incidents = int.Parse(ds.Tables[0].Rows[0][0].ToString());

        //    var incidents = (from rw in dt.AsEnumerable()
        //                     select new Incident()
        //                     {
        //                         Id = rw["Id"].ToString(),
        //                         CreatedBy = rw["CreatedBy"].ToString(),
        //                         AssignedTo = rw["AssignedTo"].ToString(),
        //                         CreatedAT = DateTime.Parse(rw["CreatedAT"].ToString()),
        //                         Title = rw["Title"].ToString(),
        //                         Description = rw["Description"].ToString(),
        //                         AdditionalData = rw["AdditionalData"].ToString(),                                
        //                         StartTime = DateTime.Parse(rw["StartTime"].ToString()),
        //                         DueDate = DateTime.Parse(rw["DueDate"].ToString()),
        //                         Status = rw["Status"].ToString()
        //                     }).ToList();

        //    return new IncidentsWithPage 
        //    { 
        //        Total_Incidents = total_incidents,
        //        Incidents = incidents
        //    };
        //}

        public async Task<object> GetIncidentsPageAsync(int pageSize, int pageNumber, string sortBy, string sortDirection, string Serach)
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

            return await dbAccess.ExecuteProcedureAsync2("GetIncidentsPage", parameters, SetIncidentPageAsync);          
        }

        public async Task<object> SetIncidentPageAsync(SqlDataReader reader)
        {
            List<Incident> incidents = new List<Incident>();
            await reader.ReadAsync();
            int total_incidents = int.Parse(reader[0].ToString());

            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                Incident incident = new Incident()
                {
                    Id = reader["Id"].ToString(),
                    CreatedBy = reader["CreatedBy"].ToString(),
                    AssignedTo = reader["AssignedTo"].ToString(),
                    CreatedAT = DateTime.Parse(reader["CreatedAT"].ToString()),
                    Title = reader["Title"].ToString(),
                    Description = reader["Description"].ToString(),
                    AdditionalData = reader["AdditionalData"].ToString(),
                    StartTime = DateTime.Parse(reader["StartTime"].ToString()),
                    DueDate = DateTime.Parse(reader["DueDate"].ToString()),
                    Status = reader["Status"].ToString()
                };
                incidents.Add(incident);
            }

            return new IncidentsWithPage
            {
                Total_Incidents = total_incidents,
                Incidents = incidents
            };

        }

        public async Task<object> GetIncidentsPageTestAsync(int pageSize, int pageNumber, string sortBy, string sortDirection, string Serach)
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

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetIncidentsPage", parameters);
            var ds = dbResponse.Ds;

            if (dbResponse.Error)
                return dbResponse.ErrorMsg;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            dt = ds.Tables[1];
            int total_incidents = int.Parse(ds.Tables[0].Rows[0][0].ToString());

            var incidents = (from rw in dt.AsEnumerable()
                             select new Incident()
                             {
                                 Id = rw["Id"].ToString(),
                                 CreatedBy = rw["CreatedBy"].ToString(),
                                 AssignedTo = rw["AssignedTo"].ToString(),
                                 CreatedAT = DateTime.Parse(rw["CreatedAT"].ToString()),
                                 Title = rw["Title"].ToString(),
                                 Description = rw["Description"].ToString(),
                                 AdditionalData = rw["AdditionalData"].ToString(),
                                 StartTime = DateTime.Parse(rw["StartTime"].ToString()),
                                 DueDate = DateTime.Parse(rw["DueDate"].ToString()),
                                 Status = rw["Status"].ToString()
                             }).ToList();

            return new IncidentsWithPage
            {
                Total_Incidents = total_incidents,
                Incidents = incidents
            };
        }

        public async Task<DbResponse> UpdateIncidentAsync(string incidentId , string parameter , string value , string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "IncidentId" , incidentId},
                 { "Parameter" , parameter},
                 { "Value" , value},
                 { "UserId" , userId},             
            };

            return await dbAccess.ExecuteProcedureAsync("UpdateIncident", parameters);
        }

        public async Task<DbResponse> UpdateCommentAsync(string commentId, string commentText, string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "CommentText" , commentText},
                 { "CommentId" , commentId},
                 { "UserId" , userId}
            };

            return await dbAccess.ExecuteProcedureAsync("UpdateComment", parameters);
        }

        /////////////////////////////////////// Dashboard //////////////////////
        public async Task<object> KPIAsync(string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "UserId" , userId}
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetKPI", parameters);
            var data = dbResponse.Ds.Tables[0].Rows[0];

            return new
            {
                New = data["New"],
                InProgress = data["InProgress"],
                Closed = data["Closed"],
                Approved = data["Approved"],
                Late = data["Late"],
                AssignedToMe = data["AssignedToMe"]

            };
        }

        public async Task<object> OverallWidgetAsync()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {                 
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetOverallWidget", parameters);
            var data = dbResponse.Ds.Tables[0].Rows[0];

            return new
            {
                New = data["New"],
                InProgress = data["InProgress"],
                Closed = data["Closed"],
                Approved = data["Approved"],
                Late = data["Late"]      
            };
        }

        public async Task<List<Incident>> Last5IncidentsAsync()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetLast5Incidents", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            dt = ds.Tables[0];

            var incidents = (from rw in dt.AsEnumerable()
                             select new Incident()
                             {
                                 Id = rw["Id"].ToString(),
                                 CreatedBy = rw["CreatedBy"].ToString(),
                                 AssignedTo = rw["AssignedTo"].ToString(),
                                 CreatedAT = DateTime.Parse(rw["CreatedAT"].ToString()),
                                 Title = rw["Title"].ToString(),
                                 Description = rw["Description"].ToString(),
                                 AdditionalData = rw["AdditionalData"].ToString(),
                                 StartTime = DateTime.Parse(rw["StartTime"].ToString()),
                                 DueDate = DateTime.Parse(rw["DueDate"].ToString()),
                                 Status = rw["Status"].ToString()
                             }).ToList();

            return incidents;
        }

        public async Task<List<Incident>> Oldest5UnresolvedIncidentsAsync()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetOldest5UnresolvedIncidents", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            dt = ds.Tables[0];

            var incidents = (from rw in dt.AsEnumerable()
                             select new Incident()
                             {
                                 Id = rw["Id"].ToString(),
                                 CreatedBy = rw["CreatedBy"].ToString(),
                                 AssignedTo = rw["AssignedTo"].ToString(),
                                 CreatedAT = DateTime.Parse(rw["CreatedAT"].ToString()),
                                 Title = rw["Title"].ToString(),
                                 Description = rw["Description"].ToString(),
                                 AdditionalData = rw["AdditionalData"].ToString(),
                                 StartTime = DateTime.Parse(rw["StartTime"].ToString()),
                                 DueDate = DateTime.Parse(rw["DueDate"].ToString()),
                                 Status = rw["Status"].ToString()
                             }).ToList();

            return incidents;
        }


        public async Task<object> MostAssignedToUsersIncidentsAsync()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
            };

            var dbResponse = await dbAccess.ExecuteProcedureAsync("GetMostAssignedToUsersIncidents", parameters);
            var data = dbResponse.Ds.Tables[0];

            return (from rw in data.AsEnumerable()
                    select new 
                    {
                        UserId = rw["UserId"].ToString(),
                        Name = rw["Name"].ToString(),
                        Count = rw["Count"].ToString()                       
                    }).ToList();
        }


        ///////////////////////////////////////End Dashboard //////////////////////

    }// end class

    public class IncidentsWithPage
    {
        public int Total_Incidents { get; set; }
        public List<Incident> Incidents { get; set; }
    }
}// end namespace