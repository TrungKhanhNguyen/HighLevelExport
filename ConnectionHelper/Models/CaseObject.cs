using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionHelper.Models
{
    public class CaseObject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string brief { get; set; }
        public string owner { get; set; }
        public string dateCreated { get; set; }
        public string dateUpdated { get; set; }
        public string sensitivity { get; set; }
        public string priority { get; set; }
        public string status { get; set; }
        public string group { get; set; }
        public string trashedTime { get; set; }
    }
}
