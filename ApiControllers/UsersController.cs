using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class UsersController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public UsersController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = UsersMethods.Authenticate(model);
            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });          

            return Ok(response);
        }

        [HttpGet("AllUsers")]
        //[Authorize]
        public IActionResult AllUsers()
        {
            return Ok(UsersMethods.GetAllUsers());
        }

        [HttpGet("UserNotifications")]
        //[Authorize]
        public IActionResult UserNotifications(string userId)
        {
            return Ok(UsersMethods.GetUserNotifications(userId));
        }


        [HttpPost("UpdateHubId")]
        //[Authorize]
        public IActionResult UpdateHubId([FromBody] HubUpdate HU)
        {
            var dbResponse = UsersMethods.UpdateHubId(HU.UserId, HU.HubId);
            if (dbResponse.Error)
                return BadRequest(new { message = "Could not update hubId. Error : " + dbResponse.ErrorMsg});
            return Ok();
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser()
        {
            User user = new User();
            user.FirstName = HttpContext.Request.Form["FirstName"];
            user.LastName = HttpContext.Request.Form["LastName"];
            user.Email = HttpContext.Request.Form["Email"];
            user.Phone = HttpContext.Request.Form["Phone"];
            user.ProfilePic = HttpContext.Request.Form.Files.Count > 0 ? HttpContext.Request.Form.Files[0].FileName : "";

            if (user == null || string.IsNullOrWhiteSpace(user.FirstName)
                 || string.IsNullOrWhiteSpace(user.LastName) || string.IsNullOrWhiteSpace(user.Email)
                )
            {
                return BadRequest(new { message = "Please enter all required fields." });                
            }

            var dbResponse = UsersMethods.AddUser(user);
            if (dbResponse.Error)
            {
                if (dbResponse.ErrorMsg.Contains("UNQ__Users__Username"))
                    return BadRequest(new { message = "Username already exists." });             
                else if (dbResponse.ErrorMsg.Contains("UNQ__Users__Email"))
                    return BadRequest(new { message = "Email already exists." });               
                else
                    return BadRequest(new { message = "Internal Error. " + dbResponse.ErrorMsg });               
            }

            string user_Id = dbResponse.Ds.Tables[0].Rows[0][0].ToString();

            if (HttpContext.Request.Form.Files.Count > 0)
            {
                foreach (var formFile in HttpContext.Request.Form.Files)
                {
                    if (formFile.Length > 0)
                    {
                        string folder = _hostingEnvironment.ContentRootPath + "\\Attachments\\Users\\" + user_Id;
                        Directory.CreateDirectory(folder);
                        string path = folder + "\\" + formFile.FileName;

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                    }
                }

            }//end of if count > 0
            return Ok("New User has been added.");
        }

        [HttpGet("GetUsersWithPage")]
        public IActionResult GetUsersWithPage(int PageSize, int PageNumber, string SortBy, string SortDirection, string Search)
        {
            var response = UsersMethods.GetUsersPage(PageSize, PageNumber, SortBy, SortDirection, Search);
            return Ok(response);
        }

        // GET: api/<UsersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UsersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
