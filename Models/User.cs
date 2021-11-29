using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IM.Models
{
    public class User
    {
        public string Id { get; set; }      
        public DateTime CreateDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string HubId { get; set; }


    }
}

