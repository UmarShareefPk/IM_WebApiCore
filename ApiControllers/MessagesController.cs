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
         
            return Ok(result);
        }



        [HttpGet("ConversationsByUser")]
        [Authorize]
        public async Task<IActionResult> GetConversationsByUser(string UserId)
        {
            var result = await _messagesMethods.GetConversationsByUserAsync(UserId);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }


            return Ok(result);
        }

        [HttpPost("DeleteConversation")]
        [Authorize]
        public async Task<IActionResult> DeleteConversation(string ConversationId)
        {
            var result = await _messagesMethods.DeleteConversationAsync(ConversationId);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }

            return Ok();
        }

        [HttpPost("DeleteMessage")]
        [Authorize]
        public async Task<IActionResult> DeletMessage([FromQuery]string MessageId)
        {
            var result = await _messagesMethods.DeleteMessageAsync(MessageId);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }

            return Ok();
        }

        [HttpPost("ChangeMessageStatus")]
        [Authorize]
        public async Task<IActionResult> ChangeMessageStatus(string MessageId, string Status)
        {
            var result = await _messagesMethods.ChangeMessageStatusAsync(MessageId, Status);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }

            return Ok();
        }


        [HttpGet("MessagesByConversations")]
        [Authorize]
        public async Task<IActionResult> GetMessagesByConversations(string ConversationId)
        {
            var result = await _messagesMethods.GetMessagesByConversationsAsync(ConversationId);

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
            var result = await _messagesMethods.GetMessagesByUserAsync(UserId);

            if (result.GetType() == typeof(DbResponse))
            {
                var error = (DbResponse)result;
                return StatusCode(500, new { message = error.ErrorMsg });
            }


            return Ok(result);
        }


    }// end of class
}
