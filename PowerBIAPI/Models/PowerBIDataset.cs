using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PowerBIAPI.Models
{
    public class PowerBIDataset
    {
        public string name { get; set; }
        public List<Tables> tables { get; set; }

        public class Tables
        {
            public string name { get; set; }
            public List<Columns> columns { get; set; }

            public class Columns
            {
                public string name { get; set; }
                public string dataType { get; set; }
            }
        }
    }
}