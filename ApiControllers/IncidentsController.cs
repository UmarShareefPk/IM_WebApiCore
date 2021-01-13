using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IM.Common;
using IM.Models;
using IM.SQL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IM_Core.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public IncidentsController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("AddIncident")]
        public async Task<IActionResult> AddIncident()
        {
            Incident incident = new Incident();
            incident.Title = HttpContext.Request.Form["Title"];
            incident.Description = HttpContext.Request.Form["Description"];
            incident.AdditionalData = HttpContext.Request.Form["AdditionalDeta"];
            incident.AssignedTo = HttpContext.Request.Form["AssignedTo"];
            incident.CreatedBy = HttpContext.Request.Form["CreatedBy"];
            incident.DueDate = DateTime.Parse(HttpContext.Request.Form["DueDate"]);
            incident.StartTime = DateTime.Parse(HttpContext.Request.Form["StartTime"]);
            incident.Status = HttpContext.Request.Form["Status"];

            // return null;

            DateTime dt = new DateTime();
            if (incident == null || string.IsNullOrWhiteSpace(incident.CreatedBy) || !DateTime.TryParse(incident.CreatedAT.ToString(), out dt)
                 || string.IsNullOrWhiteSpace(incident.AssignedTo) || string.IsNullOrWhiteSpace(incident.Title)
                 || string.IsNullOrWhiteSpace(incident.Description)
                 || !DateTime.TryParse(incident.StartTime.ToString(), out dt) || !DateTime.TryParse(incident.DueDate.ToString(), out dt)
                 || string.IsNullOrWhiteSpace(incident.Status)
                )
            {
                return BadRequest(new { message = "Please enter all required fields and make sure datetime fields are in correct format." });                
            }

            string[] statusValidValues =
                                    {
                                            "N", // New 
                                            "O", // Open
                                            "I", // In Progress
                                            "C", // Closed
                                            "A"  // Approved
                                    };

            if (!statusValidValues.Contains(incident.Status.ToUpper()))
                return BadRequest(new { message = "Invalid Status Value. Valid values are N,O,I,C,A" });           

            var dbResponse = IncidentsMethods.AddIncident(incident);

            if (dbResponse.Error)
            {
                if (dbResponse.ErrorMsg.Contains("FK_Incidents_CreatedBy"))
                    return BadRequest(new { message = "This Creator Id does not exist in or system." });               
                else if (dbResponse.ErrorMsg.Contains("FK_Incidents_AssignedTo"))
                    return BadRequest(new { message = "This Assignee Id does not exist in or system." });                
                else if (dbResponse.ErrorMsg.Contains("SqlDateTime overflow"))
                    return BadRequest(new { message = "Incorrect Datetime value." }); 
                else
                    return BadRequest(new { message = "Internal Error. " + dbResponse.ErrorMsg });                
            }

            string incident_Id = dbResponse.Ds.Tables[0].Rows[0][0].ToString();

            if (HttpContext.Request.Form.Files.Count > 0)
            {               
                foreach (var formFile in HttpContext.Request.Form.Files)
                {
                    string folder = _hostingEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incident_Id;
                    Directory.CreateDirectory(folder);
                    if (formFile.Length > 0)
                    {
                        var attachment = new IncidentAttachments();
                        attachment.FileName = formFile.FileName;
                        attachment.ContentType = formFile.ContentType;
                        attachment.IncidentId = incident_Id;
                        string path = folder + "\\" + formFile.FileName;

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                        IncidentsMethods.AddIncidentAttachments(attachment);
                    }
                }

            }//end of if count > 0

            return Ok("New incident has been added.");
        }


        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment()
        {
            Comment comment = new Comment();
            comment.CommentText = HttpContext.Request.Form["CommentText"];
            comment.IncidentId = HttpContext.Request.Form["IncidentId"];
            comment.UserId = HttpContext.Request.Form["UserId"];

            DateTime dt = new DateTime();
            if (comment == null || string.IsNullOrWhiteSpace(comment.CommentText) || string.IsNullOrWhiteSpace(comment.IncidentId)
                )
            {
                return BadRequest(new { message = "Please enter all required fields." });                
            }
            var dbResponse = IncidentsMethods.AddComment(comment);

            string comment_Id = dbResponse.Ds.Tables[0].Rows[0][0].ToString();

            if (HttpContext.Request.Form.Files.Count > 0)
            {
                foreach (var formFile in HttpContext.Request.Form.Files)
                {
                    string folder = _hostingEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + comment.IncidentId + "\\Comments\\" + comment_Id;
                    Directory.CreateDirectory(folder);
                    if (formFile.Length > 0)
                    {
                        var attachment = new CommentAttachments();
                        attachment.FileName = formFile.FileName;
                        attachment.ContentType = formFile.ContentType;
                        attachment.CommentId = comment_Id;

                        string path = folder + "\\" + formFile.FileName;

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                        IncidentsMethods.AddCommentAttachments(attachment);
                    }
                }
            }//end of if count > 0

            var newComment = IncidentsMethods.GetCommentById(comment_Id);
            return Ok(newComment);
        }

        [HttpGet("IncidentById")]
        public IActionResult IncidentById(string Id)
        {
            //Thread.Sleep(500);
            return Ok(IncidentsMethods.GetIncidentrById(Id));
        }


        [HttpGet("DownloadFile")]
        public ActionResult DownloadFile(string type, string commentId, string incidentId, string filename, string contentType)
        {
            string ContentType = contentType;
            //Physical Path of Root Folder
            var rootPath = "";
            if (type.ToLower() == "comment")
            {
                rootPath = _hostingEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId + "\\Comments\\" + commentId;
            }
            else
            {
                rootPath = _hostingEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId;
            }
            var fileFullPath = Path.Combine(rootPath, filename);

            byte[] fileBytes = System.IO.File.ReadAllBytes(fileFullPath);
            return File(fileBytes, ContentType, filename);
        }

        [HttpGet("DeleteFile")]
        public string DeleteFile(string type, string commentId, string incidentId, string userId, string fileId, string filename, string contentType)
        {
            string ContentType = contentType;
            //Physical Path of Root Folder
            var rootPath = "";
            if (type.ToLower() == "comment")
            {
                IncidentsMethods.DeleteFile("comment", fileId, userId);
                rootPath = _hostingEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId + "\\Comments\\" + commentId;
               // rootPath = System.Web.HttpContext.Current.Server.MapPath("~/Attachments/Incidents/" + incidentId + "/Comments/" + commentId);
            }
            else
            {
                IncidentsMethods.DeleteFile("incident", fileId, userId);
                rootPath = _hostingEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId;
                //rootPath = System.Web.HttpContext.Current.Server.MapPath("~/Attachments/Incidents/" + incidentId);
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var fileFullPath = System.IO.Path.Combine(rootPath, filename);

            if (System.IO.File.Exists(@fileFullPath))
            {
                System.IO.File.Delete(@fileFullPath);
            }
            return "Fiile Delete";
        }


        [HttpGet("DeleteComment")]
        public string DeleteComment(string commentId, string incidentId, string userId)
        {

            IncidentsMethods.DeleteComment(commentId, userId);
            string path = _hostingEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId + "\\Comments\\" + commentId;
           // string path = System.Web.HttpContext.Current.Server.MapPath("~/Attachments/Incidents/" + incidentId + "/Comments/" + commentId);

            if (Directory.Exists(@path))
            {
                Helper.DeleteDirectory(@path);
            }
            return "Comment Delete";
        }


        // GET api/<controller>/5

        [HttpGet]
        public List<Incident> GetAllIncidents()
        {
            return IncidentsMethods.GetAllIncidents();
        }

        [HttpPost("UpdateIncident")]
        public void UpdateIncident([FromBody] IncidentUpdate IU) //IU = IncidentUpdate, 
        {
            IncidentsMethods.UpdateIncident(IU.IncidentId, IU.Parameter, IU.Value, IU.UserId);
        }

        [HttpPost("UpdateComment")]
        public void UpdateComment([FromBody] Comment C)
        {
            IncidentsMethods.UpdateComment(C.Id, C.CommentText, C.UserId);
        }

        // [Authorize]
        [HttpGet("GetIncidentsWithPage")]
        public IncidentsWithPage GetIncidentsWithPage(int PageSize, int PageNumber, string SortBy, string SortDirection, string Search)
        {
            // Thread.Sleep(7000);
            return IncidentsMethods.GetIncidentsPage(PageSize, PageNumber, SortBy, SortDirection, Search);
        }

        // GET: api/<IncidentsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<IncidentsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<IncidentsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<IncidentsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<IncidentsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
