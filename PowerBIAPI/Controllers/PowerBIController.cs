using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PowerBIAPI.Controllers
{
    public class PowerBIController : ApiController
    {
        public static AuthenticationResult authorization;
    }
}
