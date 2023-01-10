using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMSIntellegoSync
{
    public class NewSyncObject
    {
        public string InterceptId { get; set; }
        public string InterceptName { get; set; }

        public string CASEID { get; set; }
        //public string NAME { get; set; }
        public string OPTIONVALUE { get; set; }

        public string STOPDATETIME { get; set; }
        public string MOD_TS { get; set; }
        public string STATE { get; set; }
    }
}
