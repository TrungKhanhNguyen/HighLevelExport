using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionHelper.Models
{
    public class ExportLocation
    {
        public string orclId { get; set; }
        public string phone { get; set; }
        public string mito { get; set; }
        public string targetId { get; set; }
        public string liuId { get; set; }
        public string type { get; set; }
        public string sequence { get; set; }
        public string displayDate { get; set; }
        public string length { get; set; }
        public string network { get; set; }
        public string direction { get; set; }
        public string normCin { get; set; }
        public string lastUpdate { get; set; }
        public string[][] iriRecords { get; set; }
        public string hi3File { get; set; }
        public string fullPath { get; set; }
    }
}
