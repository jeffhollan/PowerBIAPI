using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PowerBIAPI.Models
{
    public class AuthResult
    {
        public DateTime Expires { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}