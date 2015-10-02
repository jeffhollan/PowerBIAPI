using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PowerBIAPI.Models
{
    public class PowerBIRows
    {
        public List<JToken> rows { get; set; }
    }
}