using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IM.Common;
using IM.Models;
using IM.SQL;
using IM_Core.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IM_Core.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersMethods _usersMethods;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public UsersController(UsersMethods usersMethods, IWebHostEnvironment webHostEnvironment, IEmailService emailService, IConfiguration configuration)
        {        
            _usersMethods = usersMethods;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet("Test")]
        // [Authorize]
        public async Task<IActionResult> Test()
        {
            //_emailService.SendEmail();
            var storageConnectionString = "Nothing yet";
            if(_configuration.GetValue<string>("ConnectionStringAzure") != null)
                storageConnectionString = _configuration.GetValue<string>("ConnectionStringAzure");
            return Ok(storageConnectionString);
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(AuthenticateRequest model)
        {
            var response = await _usersMethods.AuthenticateAsync(model);
            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });          

            return Ok(response);
        }

        [HttpGet("AllUsers")]
       // [Authorize]
        public async Task<IActionResult> AllUsersAsync()
        {
            //_emailService.SendEmail();
            return Ok( await _usersMethods.GetAllUsersAsync());
        }

        [HttpGet("UpdateIsRead")]
        [Authorize]
        public async Task<IActionResult> UpdateIsReadAsync(string notificationId, string isRead)
        {
            bool isReadStatus = bool.Parse(isRead);
            var response = await _usersMethods.UpdateIsReadAsync(notificationId, isReadStatus);
            if (response.Error)
                return StatusCode(500);
            return Ok();
        }


        [HttpGet("UserNotifications")]
        [Authorize]
        public async Task<IActionResult> UserNotificationsAsync(string userId)
        {
            return Ok(await _usersMethods.GetUserNotificationsAsync(userId));
        }


        [HttpPost("UpdateHubId")]
        [Authorize]
        public async Task<IActionResult> UpdateHubIdAsync([FromBody] HubUpdate HU)
        {
            var dbResponse = await _usersMethods.UpdateHubIdAsync(HU.UserId, HU.HubId);
            if (dbResponse.Error)
                return BadRequest(new { message = "Could not update hubId. Error : " + dbResponse.ErrorMsg});
            return Ok();
        }

        [HttpPost("AddUser")]
        [Authorize]
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

            var dbResponse = await _usersMethods.AddUserAsync(user);
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
                        
                        string folder = _webHostEnvironment.ContentRootPath + "\\Attachments\\Users\\" + user_Id;
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
        [Authorize]
        public async Task<IActionResult> GetUsersWithPageAsync(int PageSize, int PageNumber, string SortBy, string SortDirection, string Search)
        {
            var response = await _usersMethods.GetUsersPageAsync(PageSize, PageNumber, SortBy, SortDirection, Search);
            return Ok(response);
        }

    }
}
