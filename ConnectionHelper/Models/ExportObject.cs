using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionHelper.Models
{
    public class ExportObject
    {
        public string CaseName { get; set; }
        public string InterceptName { get; set; }
        public string InterceptId { get; set; }
        public string elasticId { get; set; }
        public DateTime eventDate { get; set; }

        public string call_id { get; set; }
        public string call_direction { get; set; }
        public DateTime call_startTime { get; set; }
        public DateTime call_endTime { get; set; }
        public string call_type { get; set; }
        public string call_status { get; set; }
        public string call_callTerminationReason { get; set; }
        public string call_ringDuration { get; set; }
        public string call_correlation { get; set; }
        public string call_caller { get; set; }
        public string call_callee { get; set; }
        public string call_callerE164 { get; set; }
        public string call_calleeE164 { get; set; }
        public string call_liveListenPipe { get; set; }
        public string call_conversationDuration { get; set; }
        public string call_title { get; set; }
        public string call_product_id { get; set; }
        public string call_product_url { get; set; }
        public string call_product_type { get; set; }
        public string call_product_size { get; set; }
        public string call_product_order { get; set; }
        public string call_product_clip_time_origin { get; set; }
        public string call_product_clip_duration { get; set; }
        public string call_product_name { get; set; }
        public string call_product_description { get; set; }
        public string call_product_exemplar { get; set; }
        public string call_product_call { get; set; }

        public string call_info_id { get; set; }
        public string call_info_timestamp { get; set; }
        public string call_info_type { get; set; }
        public string call_info_partyDesignator { get; set; }
        public string call_info_event { get; set; }
        public string call_info_value { get; set; }


        public string call_participant_imei { get; set; }
        public string call_participant_imsi { get; set; }

        public string celltower_address { get; set; }
        public string celltower_cellid { get; set; }
        public string celltower_latlong { get; set; }


        public string document_id { get; set; }
        public string document_type { get; set; }
        public string document_metadata { get; set; }
        public List<Call_Location_Object> listCGI { get; set; }
    }
    public class LatLonObject
    {
        public string lat { get; set; }
        public string lon { get; set; }
    }

    public class Call_Location_Object
    {
        public DateTime timestamp { get; set; }
        public string value { get; set; }
        public string call { get; set; }
        public string type { get; set; }
        public string id { get; set; }

        public string address { get; set; }
        public string latlon { get; set; }
    }
}
