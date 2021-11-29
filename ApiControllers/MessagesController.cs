using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IM.Common;
using IM.Models;
using IM.SQL;
using IM_Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace IM_Core.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public MessagesController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("AddCMessage")]
        [Authorize]
        public async Task<IActionResult> AddComment()
        {
            
           string From = HttpContext.Request.Form["CommentText"];
           string To = HttpContext.Request.Form["IncidentId"];
           string MessageText = HttpContext.Request.Form["UserId"];

            DateTime dt = new DateTime();
            if ( string.IsNullOrWhiteSpace(From) || string.IsNullOrWhiteSpace(To) || string.IsNullOrWhiteSpace(MessageText) 
                )
            {
                return BadRequest(new { message = "Please enter all required fields." });
            }
            var result = MessagesMethods.AddMessage(From, To, MessageText);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }

            Message message = (Message)result;    
         
            return Ok(message);
        }
    }
}
