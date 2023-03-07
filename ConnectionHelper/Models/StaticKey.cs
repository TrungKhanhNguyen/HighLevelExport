using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionHelper.Models
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
        public static string EXPORT_FOLDER = @"C:\Export";
        public static string BACKUP_FOLDER = @"E:\LMCBackup";
        public static string EXPORT_2MINS_FOLDER = @"C:\Export2Mins";
        public static string EXPORT_MANUAL_FOLDER = @"C:\ExportManual";

        public static string ELASTIC_IP = "http://172.25.3.34:9200";
        public static string ELASTIC_LOCAL_IP = "http://localhost:9200/";

        public static string SIGNALR_IP = "http://172.10.2.39:8089/signalr";
        public static string SIGNALR_LOCAL_IP = "http://192.168.1.4:8087/signalr";
    }

    public enum StatusInfo
    {
        Successful = 0,
        Failed = 1,
        Alert = 2,
        Info = 3
    }

    public enum ReExportType
    {
        Hour = 0,
        Minute = 1,
        Callback = 2,
        Backup = 3
    }

    public enum ErrorType
    {
        HourError = 0,
        MinuteError = 1,
        CallbackError = 2
    }
}
