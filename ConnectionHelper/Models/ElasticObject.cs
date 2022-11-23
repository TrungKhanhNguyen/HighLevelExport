using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionHelper.Models
{
    public class ElasticObject
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }


        public string _score { get; set; }
        public string _routing { get; set; }
        public EventDate fields { get; set; }

    }

    public class EventDate
    {
        public DateTime eventD { get; set; }
    }
}
