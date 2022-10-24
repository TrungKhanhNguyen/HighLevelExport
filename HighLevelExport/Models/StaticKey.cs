using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighLevelExport.Models
{
    public class StaticKey
    {
        public static string SERVER_IP = "172.10.2.33";
        public static string DATABASE_NAME = "intellego";
        public static string USER_NAME = "ss8mysql";
        public static string PASSWORD = "ss81234";
        public static string PORT = "3306";


        public static string BSA_SERVER_IP = "172.10.2.37";
        public static string BSA_DATABASE_NAME = "slpdb_controller";
        public static string BSA_USER_NAME = "loader";
        public static string BSA_PASSWORD = "Ss8Inc$756";

        public static string MAIN_URL_FOLDER = @"Z:\";
        public static string EXPORT_FOLDER = @"C:\Work\export";
    }

    
    public enum StatusInfo {
        Successful = 0,
        Failed = 1,
        Alert = 2,
        Info = 3
    }

}
