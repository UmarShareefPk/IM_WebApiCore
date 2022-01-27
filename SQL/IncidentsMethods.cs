using IM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        public DbResponse AddIncident(Incident incident)
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
            return dbAccess.ExecuteProcedure("AddNewIncident", parameters);
        }

        public DbResponse AddComment(Comment comment)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "IncidentId" , comment.IncidentId },
                  { "UserId" , comment.UserId },
                  { "CommentText" , comment.CommentText },
                 

            };
            return dbAccess.ExecuteProcedure("AddComment", parameters);
        }

        public  DbResponse AddIncidentAttachments(IncidentAttachments incidentAttachments)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "FileName" , incidentAttachments.FileName },
                  { "ContentType" , incidentAttachments.ContentType },
                  { "IncidentId" , incidentAttachments.IncidentId }
            };
            return dbAccess.ExecuteProcedure("AddIncidentAttachment", parameters);
        }

        public  DbResponse AddCommentAttachments(CommentAttachments commentAttachments)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "FileName" , commentAttachments.FileName },
                  { "ContentType" , commentAttachments.ContentType },
                  { "CommentId" , commentAttachments.CommentId }
            };
            var rr = dbAccess.ExecuteProcedure("AddCommentAttachment", parameters);
            return rr;
        }

        public List<IncidentAttachments> GetIncidentAttachment(string incidentId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "IncidentId" , incidentId }  
            };
            var dbResponse = dbAccess.ExecuteProcedure("GetAttachmentByIncidentId", parameters);


            var dt = dbResponse.Ds.Tables[0];

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

        public string DeleteFile(string type, string filetId, string userId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "Id" , filetId},
                  { "UserId" , userId},
            };

            var dbResponse = new DbResponse();
            if (type.ToLower() == "comment")
            {
                dbResponse = dbAccess.ExecuteProcedure("DeleteCommentAttachment", parameters);
            }
            else
            {
                dbResponse = dbAccess.ExecuteProcedure("DeleteIncidentAttachment", parameters);
            }
            var ds = dbResponse.Ds;

            if (dbResponse.Error)
                return dbResponse.ErrorMsg;
            else
                return "Success";

        }

            public Incident GetIncidentrById(string incidentId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "Id" , incidentId},
            };

            var dbResponse = dbAccess.ExecuteProcedure("GetIncidentById", parameters);
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

        public  Comment GetCommentById(string commentId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "CommentId" , commentId},
            };

            var dbResponse = dbAccess.ExecuteProcedure("GetCommentById", parameters);
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

        public  void DeleteComment(string commentId , string userId)
        {
            var parameters = new SortedList<string, object>()
            {
                  { "CommentId" , commentId},
                  { "UserId" , userId}
            };

            var dbResponse = dbAccess.ExecuteProcedure("DeleteComment", parameters);
            var ds = dbResponse.Ds;

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return;
        }

        public  List<Incident> GetAllIncidents()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
            };

            var dbResponse = dbAccess.ExecuteProcedure("GetAllIncidents", parameters);
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

        public  IncidentsWithPage GetIncidentsPage(int pageSize , int pageNumber, string sortBy, string sortDirection, string Serach)
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

            var dbResponse = dbAccess.ExecuteProcedure("GetIncidentsPage", parameters);
            var ds = dbResponse.Ds;

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

        public  object GetIncidentsPageTest(int pageSize, int pageNumber, string sortBy, string sortDirection, string Serach)
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

            var dbResponse = dbAccess.ExecuteProcedure("GetIncidentsPage", parameters);
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

        public  DbResponse UpdateIncident(string incidentId , string parameter , string value , string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "IncidentId" , incidentId},
                 { "Parameter" , parameter},
                 { "Value" , value},
                 { "UserId" , userId},             
            };

            return dbAccess.ExecuteProcedure("UpdateIncident", parameters);
        }

        public DbResponse UpdateComment(string commentId, string commentText, string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "CommentText" , commentText},
                 { "CommentId" , commentId},
                 { "UserId" , userId}
            };

            return dbAccess.ExecuteProcedure("UpdateComment", parameters);
        }

        /////////////////////////////////////// Dashboard //////////////////////
        public  object KPI(string userId)
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
                 { "UserId" , userId}
            };

            var dbResponse =  dbAccess.ExecuteProcedure("GetKPI", parameters);
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

        public  object OverallWidget()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {                 
            };

            var dbResponse = dbAccess.ExecuteProcedure("GetOverallWidget", parameters);
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

        public  List<Incident> Last5Incidents()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
            };

            var dbResponse = dbAccess.ExecuteProcedure("GetLast5Incidents", parameters);
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

        public  List<Incident> Oldest5UnresolvedIncidents()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
            };

            var dbResponse = dbAccess.ExecuteProcedure("GetOldest5UnresolvedIncidents", parameters);
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


        public  object MostAssignedToUsersIncidents()
        {
            var dt = new DataTable();
            var parameters = new SortedList<string, object>()
            {
            };

            var dbResponse = dbAccess.ExecuteProcedure("GetMostAssignedToUsersIncidents", parameters);
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