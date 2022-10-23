using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighLevelExport.Models
{
    public class ExportLocation
    {
        public string check { get; set; }
        public string[][] iriRecords { get; set; }
        public string hi3File { get; set; }
        public bool important { get; set; }
        public string note { get; set; }
        public string phoneBook { get; set; }
        public string _id { get; set; }
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
        public double displayDateStamp { get; set; }
    }

}
