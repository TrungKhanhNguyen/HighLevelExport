using Elasticsearch.Net;
using HighLevelExport.Helper;
using HighLevelExport.Models;
using MySql.Data.MySqlClient;
using Nest;
using Newtonsoft.Json;
using Rebex.Net;
using Renci.SshNet;
using SMSPDULib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinSCP;


namespace HighLevelExport
{
    public partial class Main : Form
    {

        private ExportHistoryEntities db = new ExportHistoryEntities();
        private List<ExportTarget> listTarget = new List<ExportTarget>();
        private MySqlConnection connection;
        private DBHelper helper = new DBHelper();
        private Utility utility = new Utility();
        private string connectionString = "";
        public Main()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //timer1.Start();
            //var currentTime = DateTime.Now;
            //var startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, currentTime.Hour, 0, 0);
            //var endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, currentTime.Hour, 10, 0);

            //if(currentTime >= startTime && currentTime <=endTime)
            //{

            //}
            exportData();
            //writeDataToFile();
            var source = "ftp://172.2.10.22/NAS/contentvmc/84785696789/2022-09-21/202209210221/1/target/TELEPHONY/3d99258e-28a1-4158-9940-53592bd27a3c";
            var dess = @"C:\Work\export\3d99258e-28a1-4158-9940-53592bd27a3c";
            

        }

        private void exportData()
        {
            listTarget = db.ExportTargets.ToList();
            connection = new MySqlConnection(connectionString);
            var tempListExportObject = new List<ExportObject>();
            var currentTime = new DateTime(2022,09,15,07,00,00);
            try
            {
                connection.Open();
                var cmd = helper.getIntellegoCaseAndId(connection, listTarget);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var tempObj = new ExportObject
                    {
                        CaseName = rdr.GetString(0),
                        InterceptName = rdr.GetString(1),
                        InterceptId = rdr.GetString(2)
                    };
                    tempListExportObject.Add(tempObj);
                }
                connection.Close();


                /****Phần truy vấn Elastic****/
                var nodes = new Uri[]
                        {
                            new Uri("http://localhost:9200/"),
                        };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming();
                //var elasticClient = new ElasticClient(connectionSettings);
                var elasticClient = new ElasticClient(connectionSettings.DefaultIndex("intellego"));

                var tempListExportObject2 = new List<ExportObject>();
                var startTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0).ToString("yyyy-MM-ddTHH:mm:ssZ");
                var endTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0).AddHours(3).ToString("yyyy-MM-ddTHH:mm:ssZ");
                foreach (var interceptItem in tempListExportObject)
                {
                    try
                    {
                        var tempQuery = helper.getElasticQuery(interceptItem.InterceptId, startTime, endTime);

                        var searchResult = elasticClient.LowLevel.Search<SearchResponse<ElasticObject>>(tempQuery);
                        var kkk = searchResult.Total;
                        foreach (var item in searchResult.Hits)
                        {
                            //var articleIds = item.Id;
                            //var source = item.Source;
                            var eventDate = item.Fields["eventDate"].As<DateTime[]>().First();
                            var tempObj = new ExportObject
                            {
                                CaseName = interceptItem.CaseName,
                                InterceptName = interceptItem.InterceptName,
                                InterceptId = interceptItem.InterceptId,
                                elasticId = item.Id,
                                eventDate = eventDate
                            };
                            tempListExportObject2.Add(tempObj);
                        }
                    }
                    catch
                    {
                        var message = "Exception on InterceptName= " + interceptItem.InterceptName + ", InterceptId= " + interceptItem.InterceptId;
                        txtMultiLog.Text += Environment.NewLine + message;
                        helper.InsertLogToDB(message, StatusInfo.Failed.ToString());
                    }
                    
                }
                //var sdd = tempListExportObject2;
                /****Kết thúc truy vấn Elastic****/

                /****Phần truy vấn MySQL theo type****/
                var tempListExportObjectMySQL2 = new List<ExportObject>();
                try
                {
                    //var cmd = helper.getIntellegoCaseAndId(connection, listTarget);
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        //var listExecutedItem = new List<ExportObject>();
                        using (MySqlCommand cmd2 = helper.getLocationUpdateInterceptInfo(connection, tempListExportObject2))
                        {
                            
                            using (MySqlDataReader reader = cmd2.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader.HasRows)
                                    {
                                        var docid = reader.GetString(0);
                                        var doctype = reader.GetString(1);
                                        var docmetadata = reader.GetString(2);
                                        var tempItem = tempListExportObject2.Where(m => m.elasticId == docid).FirstOrDefault();
                                        tempItem.document_type = doctype;
                                        tempItem.document_metadata = docmetadata;
                                        tempItem.document_id = docid;

                                        var indexOfMCC = docmetadata.IndexOf("452-");
                                        var stringFromMCC = "";
                                        if (indexOfMCC >= 0)
                                        {
                                            stringFromMCC = docmetadata.Substring(indexOfMCC, 18).Trim();
                                            var indexofzero = stringFromMCC.IndexOf('\0');
                                            if(indexofzero >= 0)
                                            {
                                                stringFromMCC = stringFromMCC.Remove(indexofzero);
                                            }
                                            
                                        }
                                        tempItem.celltower_cellid = stringFromMCC;
                                        tempListExportObjectMySQL2.Add(tempItem);
                                    }
                                    //tempListExportObject.Add(tempObj);
                                }
                            }
                        }
                        foreach (var item in tempListExportObject2)
                        {
                            if (tempListExportObjectMySQL2.IndexOf(item) == -1)
                            {
                                try
                                {
                                    using (MySqlCommand tempCmd = helper.getSMSInterceptInfo(connection, item))
                                    {
                                        using (MySqlDataReader tempRdr = tempCmd.ExecuteReader())
                                        {
                                            while (tempRdr.Read())
                                            {
                                                if (tempRdr.HasRows)
                                                {
                                                    int directionIndex = tempRdr.GetOrdinal("direction");
                                                    int urlIndex = tempRdr.GetOrdinal("url");
                                                    //int metadataIndex = tempRdr.GetOrdinal("metadata");
                                                    int imeiIndex = tempRdr.GetOrdinal("imei");
                                                    int imsiIndex = tempRdr.GetOrdinal("imsi");

                                                    int callerIndex = tempRdr.GetOrdinal("caller");
                                                    int calleeIndex = tempRdr.GetOrdinal("callee");

                                                    var calldirection = tempRdr.IsDBNull(directionIndex) ? string.Empty : tempRdr.GetString(directionIndex);
                                                    //var metadata = tempRdr.IsDBNull(metadataIndex) ? string.Empty : tempRdr.GetString(metadataIndex);
                                                    var imei = tempRdr.IsDBNull(imeiIndex) ? string.Empty : tempRdr.GetString(imeiIndex);
                                                    var imsi = tempRdr.IsDBNull(imsiIndex) ? string.Empty : tempRdr.GetString(imsiIndex);
                                                    var url = tempRdr.IsDBNull(urlIndex) ? string.Empty : tempRdr.GetString(urlIndex);

                                                    var caller = tempRdr.IsDBNull(callerIndex) ? string.Empty : tempRdr.GetString(callerIndex);
                                                    var callee = tempRdr.IsDBNull(calleeIndex) ? string.Empty : tempRdr.GetString(calleeIndex);

                                                    var docid = tempRdr.GetString(0);
                                                    var doctype = tempRdr.GetString(1);
                                                    var callType = tempRdr.GetString(7);

                                                    var tempItem = tempListExportObject2.Where(m => m.elasticId == docid).FirstOrDefault();
                                                    tempItem.document_type = doctype;
                                                    //tempItem.document_metadata = metadata;
                                                    tempItem.document_id = docid;
                                                    tempItem.call_direction = calldirection;
                                                    tempItem.call_participant_imei = imei;
                                                    tempItem.call_participant_imsi = imsi;
                                                    tempItem.call_product_url = url;
                                                    tempItem.call_type = callType;
                                                    tempItem.call_caller = caller;
                                                    tempItem.call_callee = callee;
                                                    tempItem.listCGI = new List<Call_Location_Object>();

                                                    tempListExportObjectMySQL2.Add(tempItem);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex) { helper.InsertLogToDB(ex.Message, StatusInfo.Failed.ToString()); }
                            }


                        }

                        foreach(var item in tempListExportObject2)
                        {
                            if (tempListExportObjectMySQL2.IndexOf(item) == -1)
                            {
                                try
                                {
                                    using (MySqlCommand callCmd = helper.getCallInterceptInfo(connection, item))
                                    {
                                        using (MySqlDataReader callRdr = callCmd.ExecuteReader())
                                        {
                                            while (callRdr.Read())
                                            {
                                                if (callRdr.HasRows)
                                                {
                                                    int directionIndex = tempRdr.GetOrdinal("direction");
                                                    int urlIndex = tempRdr.GetOrdinal("url");
                                                    //int metadataIndex = tempRdr.GetOrdinal("metadata");
                                                    int imeiIndex = tempRdr.GetOrdinal("imei");
                                                    int imsiIndex = tempRdr.GetOrdinal("imsi");

                                                    int callerIndex = tempRdr.GetOrdinal("caller");
                                                    int calleeIndex = tempRdr.GetOrdinal("callee");

                                                    var calldirection = tempRdr.IsDBNull(directionIndex) ? string.Empty : tempRdr.GetString(directionIndex);
                                                    //var metadata = tempRdr.IsDBNull(metadataIndex) ? string.Empty : tempRdr.GetString(metadataIndex);
                                                    var imei = tempRdr.IsDBNull(imeiIndex) ? string.Empty : tempRdr.GetString(imeiIndex);
                                                    var imsi = tempRdr.IsDBNull(imsiIndex) ? string.Empty : tempRdr.GetString(imsiIndex);
                                                    var url = tempRdr.IsDBNull(urlIndex) ? string.Empty : tempRdr.GetString(urlIndex);

                                                    var caller = tempRdr.IsDBNull(callerIndex) ? string.Empty : tempRdr.GetString(callerIndex);
                                                    var callee = tempRdr.IsDBNull(calleeIndex) ? string.Empty : tempRdr.GetString(calleeIndex);

                                                    var docid = tempRdr.GetString(0);
                                                    var doctype = tempRdr.GetString(1);
                                                    var callType = tempRdr.GetString(7);

                                                    var tempItem = tempListExportObject2.Where(m => m.elasticId == docid).FirstOrDefault();
                                                    tempItem.document_type = doctype;
                                                    //tempItem.document_metadata = metadata;
                                                    tempItem.document_id = docid;
                                                    tempItem.call_direction = calldirection;
                                                    tempItem.call_participant_imei = imei;
                                                    tempItem.call_participant_imsi = imsi;
                                                    tempItem.call_product_url = url;
                                                    tempItem.call_type = callType;
                                                    tempItem.call_caller = caller;
                                                    tempItem.call_callee = callee;

                                                    tempListExportObjectMySQL2.Add(tempItem);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex) { helper.InsertLogToDB(ex.Message, StatusInfo.Failed.ToString()); }
                            }
                        }

                        /****Phần truy vấn MySQL Call_location****/
                        var listSMSAndCallItem = tempListExportObjectMySQL2.Where(m => m.document_type == "6").ToList();
                        if(listSMSAndCallItem.Count > 0)
                        {
                            using (MySqlCommand cmdCallLocation = helper.getCallLocationInfo(connection, listSMSAndCallItem))
                            {
                                using (MySqlDataReader callLocationRdr = cmdCallLocation.ExecuteReader())
                                {
                                    while (callLocationRdr.Read())
                                    {
                                        if (callLocationRdr.HasRows)
                                        {
                                            var id = callLocationRdr.GetString(0);
                                            var tempTimeStamp = callLocationRdr.GetDateTime(1);
                                            var tempValue = callLocationRdr.GetString(2);
                                            var tempType = callLocationRdr.GetString(3);
                                            var tempCall = callLocationRdr.GetString(5);
                                            var cgi = new Call_Location_Object
                                            {
                                                call = tempCall,
                                                id = id,
                                                timestamp = tempTimeStamp,
                                                value = tempValue,
                                                type = tempType
                                            };
                                            var tempItem = tempListExportObjectMySQL2.Where(m => m.elasticId == tempCall).FirstOrDefault();
                                            tempItem.listCGI.Add(cgi);
                                        }
                                        //tempListExportObject.Add(tempObj);
                                    }
                                }
                            }
                        }

                        

                    }
                }
                catch
                {

                }

                /****Kết thúc phần truy vấn MySQL theo type****/

                /****Phần truy vấn MySQL BSA****/
                var finalExportList = tempListExportObjectMySQL2;
                var listLocationObj = new List<ExportObject>();
                
                string BSAConnectionString = helper.getBSAConnectionString();
                using (MySqlConnection connection2 = new MySqlConnection(BSAConnectionString))
                {
                    using (MySqlCommand cmd3 = helper.getCellTowerByCellId(connection2, tempListExportObjectMySQL2))
                    {
                        connection2.Open();
                        using (MySqlDataReader reader = cmd3.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    var cellid = reader.GetString(0);
                                    var address = reader.GetString(1);
                                    var latlong = reader.GetString(2);
                                    foreach (var item in finalExportList)
                                    {
                                        if (item.document_type == "19")
                                        {
                                            
                                        }
                                    }
                                    //var tempObj = finalExportList.Where(m=>m.listCGI. .IndexOf(cellid) >=0).fir

                                    finalExportList.Where(w => w.celltower_cellid == cellid).ToList().ForEach(s => s.celltower_address = address);
                                    finalExportList.Where(w => w.celltower_cellid == cellid).ToList().ForEach(s => s.celltower_latlong = latlong);
                                }
                            }
                        }
                    }
                }

                /****Phần ghi dữ liệu ra file****/

                /****Kết thúc phần ghi dữ liệu ra file****/
                var cmda = finalExportList;
                WriteFile(finalExportList);
            }
            /****Kết thúc phần truy vấn MySQL BSA****/
            catch (Exception ex)
            {
                var message = "Unhandled error: " + ex.Message;
                txtMultiLog.Text += Environment.NewLine + message;
                helper.InsertLogToDB(message,StatusInfo.Failed.ToString());
                connection.Close();
            }
        }

        private void WriteFile(List<ExportObject> listExport)
        {
            string initialData = "const dataHI2 = [";
            foreach(var item in listExport)
            {
                if(item.document_type == "19")
                {
                    initialData += getLocationJsonString(item) +",";
                }
                else
                {
                    initialData += getSMSJsonString(item) + ",";
                }
            }
            initialData += "]";
            var indexOfCloseCharacter = initialData.LastIndexOf(']');
            var indexOfLastComma = initialData.LastIndexOf(',');
            if (indexOfCloseCharacter > -1)
            {
                if ((indexOfLastComma + 1) == indexOfCloseCharacter)
                {
                    initialData = initialData.Remove(indexOfCloseCharacter - 1, 1);
                }

            }
            System.IO.File.WriteAllText(@"C:\Work\export\HI2.js", initialData);
        }


        private void btnStop_Click(object sender, EventArgs e)
        {

            var sd = "452-01-13623-0\0\0\0";
            var indexof = sd.IndexOf('\0');
            if(indexof != -1)
            {
                var sddsd = sd.Remove(indexof);
            }
           
            //write string to file
            //System.IO.File.WriteAllText(@"C:\Work\export\HI2.js", data);
        }
        private string getSMSJsonString(ExportObject exportObject)
        {
            var data = "### SMS ###";
            try
            {
                var decodedMsg = utility.getDecodedSMSFromUrl(exportObject.call_product_url);

                var caller = exportObject.call_caller == null ? "" : exportObject.call_caller;
                var callee = exportObject.call_callee == null ? "" : exportObject.call_callee;

                var listDic = new string[][] {
                new string[] { "Date:", exportObject.eventDate.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.eventDate.ToString("HH.mm.ss")},
                new string[] { "CalledPartyNumber:", caller + "-" + callee} ,
                new string[] { "IMEI:", exportObject.call_participant_imei == null? "" : exportObject.call_participant_imei },
                new string[] { "IMSI:", exportObject.call_participant_imsi == null? "" : exportObject.call_participant_imsi},
                new string[] { "Monitored Target:", exportObject.InterceptName},
                new string[] { "SMS-transfer-status:", ""},
                new string[] { "Decoded SMS:", decodedMsg},
                new string[] { "Decoded Number:", "" },
            };

                var export = new ExportLocation
                {
                    check = "",
                    iriRecords = listDic,
                    hi3File = "",
                    important = false,
                    note = "",
                    phoneBook = "",
                    _id = exportObject.elasticId,
                    orclId = exportObject.elasticId,
                    phone = exportObject.InterceptName,
                    mito = "",
                    targetId = exportObject.InterceptId,
                    liuId = exportObject.InterceptName,
                    type = "SMS",
                    sequence = "",
                    displayDate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                    length = "0",
                    network = utility.getNetworkByCGI(exportObject.celltower_cellid),
                    direction = exportObject.call_direction == "1" ? "IN" : "OUT",
                    normCin = "",
                    lastUpdate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                    displayDateStamp = utility.getUnixTimeByDate(exportObject.eventDate)
                };
                string json = JsonConvert.SerializeObject(export);
                data += json;
                return data;
            }
            catch { return data +  "{ },"; }
            
        }

        private string getLocationJsonString(ExportObject exportObject)
        {
            var data = "### Location ###";
            try
            {
                
                //var latlon = "['a','d']";

                var listDic = new string[][] {
                new string[] { "Date", exportObject.eventDate.ToString("dd/MM/yyyy")},
                new string[] { "Time", exportObject.eventDate.ToString("HH.mm.ss")},
                new string[] { "CGI", exportObject.celltower_cellid} ,
                new string[] { "Address", exportObject.celltower_address == null? "" : exportObject.celltower_address },
                new string[] { "City", ""},
                new string[] { "Latitude", utility.getLatLonFromString(exportObject.celltower_latlong).lat},
                new string[] { "Longitude", utility.getLatLonFromString(exportObject.celltower_latlong).lon},
            };
                var export = new ExportLocation
                {
                    check = "",
                    iriRecords = listDic,
                    hi3File = "",
                    important = false,
                    note = "",
                    phoneBook = "",
                    _id = exportObject.elasticId,
                    orclId = exportObject.elasticId,
                    phone = exportObject.InterceptName,
                    mito = "",
                    targetId = exportObject.InterceptId,
                    liuId = exportObject.InterceptName,
                    type = "CELL",
                    sequence = null,
                    displayDate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                    length = null,
                    network = utility.getNetworkByCGI(exportObject.celltower_cellid),
                    direction = null,
                    normCin = null,
                    lastUpdate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                    displayDateStamp = utility.getUnixTimeByDate(exportObject.eventDate)
                };
                string json = JsonConvert.SerializeObject(export);
                data += json;
                return data ;
            }
            catch { return "{ },"; }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            
        }

        private void Main_Load(object sender, EventArgs e)
        {
            timer1.Interval = 120000; // specify interval time as you want
            timer1.Tick += new EventHandler(timer1_Tick);
            connectionString = helper.getConnectionString();
            
        }
    }
}


