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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly MessagesMethods _messagesMethods;

        public MessagesController(IWebHostEnvironment webHostEnvironmentt, MessagesMethods messagesMethods)
        {
            _webHostEnvironment = webHostEnvironmentt;
            _messagesMethods = messagesMethods;
        }

        [HttpPost("AddMessage")]
        [Authorize]
        public async Task<IActionResult> AddMessage()
        {

            string From = HttpContext.Request.Form["From"];
            string To = HttpContext.Request.Form["To"];
            string MessageText = HttpContext.Request.Form["MessageText"];

            //string From = "16819E79-25A3-46C9-8B1F-8FB6C6F8AC61";
            //string To = "BF041A9D-B923-4A89-9E5A-11E59808A617";
            //string MessageText = "One year today. Those who were lucky enough to know my father knew him as the personification of extraordinary integrity,";

            DateTime dt = new DateTime();
            if ( string.IsNullOrWhiteSpace(From) || string.IsNullOrWhiteSpace(To) || string.IsNullOrWhiteSpace(MessageText) 
                )
            {
                return BadRequest(new { message = "Please enter all required fields." });
            }
            var result = await _messagesMethods.AddMessageAsync(From, To, MessageText);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }

            Message message = (Message)result;    
         
            return Ok(message);
        }



        [HttpGet("ConversationsByUser")]
        [Authorize]
        public async Task<IActionResult> GetConversationsByUser(string UserId)
        {
            var result = _messagesMethods.GetConversationsByUserAsync(UserId);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }


            return Ok(result);
        }

        [HttpGet("MessagesByConversations")]
        [Authorize]
        public async Task<IActionResult> GetMessagesByConversations(string ConversationId)
        {
            var result = _messagesMethods.GetMessagesByConversationsAsync(ConversationId);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }


            return Ok(result);
        }

        [HttpGet("MessagesByUser")]
        [Authorize]
        public async Task<IActionResult> GetMessagesByUser(string UserId)
        {        
            var result = _messagesMethods.GetMessagesByUserAsync(UserId);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }


            return Ok(result);
        }


    }// end of class
}
