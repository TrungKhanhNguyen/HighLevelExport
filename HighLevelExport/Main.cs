using Elasticsearch.Net;
using HighLevelExport.Helper;
using HighLevelExport.Models;
using MySql.Data.MySqlClient;
using Nest;
using Newtonsoft.Json;
using Renci.SshNet;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HighLevelExport
{
    public partial class Main : Form
    {

        private ExportHistoryEntities db = new ExportHistoryEntities();
        private List<ExportTarget> listTarget = new List<ExportTarget>();
        private DBHelper helper = new DBHelper();
        private Utility utility = new Utility();
        private string connectionString = "";
        public Main()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //exportData();
            exportSingleData();
        }

        private void ExecuteInterceptName(ExportObject interceptNameObject, string startTime, string endTime, string startTimeWrite)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var tempListExportElasticObject = new List<ExportObject>(); //list dữ liệu khi truy vấn Elastic
                    var tempListExportInterceptInfo = new List<ExportObject>();  //list dữ liệu khi thêm thông tin Intercept Info
                    /****Phần truy vấn Elastic****/

                    var nodes = new Uri[]
                    {
                                new Uri(StaticKey.ELASTIC_IP),
                    };
                    var connectionPool = new StaticConnectionPool(nodes);
                    var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming();
                    //var elasticClient = new ElasticClient(connectionSettings);
                    var elasticClient = new ElasticClient(connectionSettings.DefaultIndex("intellego"));

                    var tempQuery = helper.getElasticQuery(interceptNameObject.InterceptId, startTime, endTime);
                    var searchResult = elasticClient.LowLevel.Search<SearchResponse<ElasticObject>>(tempQuery);
                    foreach (var itemHit in searchResult.Hits)
                    {
                        var eventDate = itemHit.Fields["eventDate"].As<DateTime[]>().First();
                        var tempObj = new ExportObject
                        {
                            CaseName = interceptNameObject.CaseName,
                            InterceptName = interceptNameObject.InterceptName,
                            InterceptId = interceptNameObject.InterceptId,
                            elasticId = itemHit.Id,
                            eventDate = eventDate.AddHours(7)
                        };
                        tempListExportElasticObject.Add(tempObj);
                    }

                    if (tempListExportElasticObject.Count() > 0)
                    {
                        /****Phần truy vấn MySQL theo type****/
                        //Phần Location Update
                        using (MySqlCommand cmd2 = helper.getLocationUpdateInterceptInfo(connection, tempListExportElasticObject))
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
                                        var tempItem = tempListExportElasticObject.Where(m => m.elasticId == docid).FirstOrDefault();
                                        tempItem.document_type = doctype;
                                        tempItem.document_metadata = docmetadata;
                                        tempItem.document_id = docid;

                                        var indexOfMCC = docmetadata.IndexOf("452-");
                                        var stringFromMCC = "";
                                        if (indexOfMCC >= 0)
                                        {
                                            stringFromMCC = docmetadata.Substring(indexOfMCC, 18).Trim();
                                            var indexofzero = stringFromMCC.IndexOf('\0');
                                            if (indexofzero >= 0)
                                            {
                                                stringFromMCC = stringFromMCC.Remove(indexofzero);
                                            }
                                        }
                                        tempItem.celltower_cellid = stringFromMCC;
                                        tempListExportInterceptInfo.Add(tempItem);
                                    }
                                    //tempListExportObject.Add(tempObj);
                                }
                            }
                        }
                        //Phần lấy thông tin SMS
                        foreach (var itemElastic in tempListExportElasticObject)
                        {
                            if (tempListExportInterceptInfo.IndexOf(itemElastic) == -1)
                            {
                                using (MySqlCommand tempCmd = helper.getSMSInterceptInfo(connection, itemElastic))
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

                                                var tempItem = tempListExportElasticObject.Where(m => m.elasticId == docid).FirstOrDefault();
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

                                                tempListExportInterceptInfo.Add(tempItem);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //Phần lấy thông tin Call
                        foreach (var itemElastic in tempListExportElasticObject)
                        {
                            if (tempListExportInterceptInfo.IndexOf(itemElastic) == -1)
                            {
                                using (MySqlCommand callCmd = helper.getCallInterceptInfo(connection, itemElastic))
                                {
                                    using (MySqlDataReader callRdr = callCmd.ExecuteReader())
                                    {
                                        while (callRdr.Read())
                                        {
                                            if (callRdr.HasRows)
                                            {
                                                int directionIndex = callRdr.GetOrdinal("direction");
                                                int urlIndex = callRdr.GetOrdinal("url");
                                                //int metadataIndex = tempRdr.GetOrdinal("metadata");
                                                int imeiIndex = callRdr.GetOrdinal("imei");
                                                int imsiIndex = callRdr.GetOrdinal("imsi");

                                                int callerIndex = callRdr.GetOrdinal("caller");
                                                int calleeIndex = callRdr.GetOrdinal("callee");

                                                int startTimeIndex = callRdr.GetOrdinal("startTime");
                                                int endTimeIndex = callRdr.GetOrdinal("endTime");

                                                int conversationDurationIndex = callRdr.GetOrdinal("conversationDuration");

                                                var calldirection = callRdr.IsDBNull(directionIndex) ? string.Empty : callRdr.GetString(directionIndex);
                                                var imei = callRdr.IsDBNull(imeiIndex) ? string.Empty : callRdr.GetString(imeiIndex);
                                                var imsi = callRdr.IsDBNull(imsiIndex) ? string.Empty : callRdr.GetString(imsiIndex);
                                                var url = callRdr.IsDBNull(urlIndex) ? string.Empty : callRdr.GetString(urlIndex);

                                                var caller = callRdr.IsDBNull(callerIndex) ? string.Empty : callRdr.GetString(callerIndex);
                                                var callee = callRdr.IsDBNull(calleeIndex) ? string.Empty : callRdr.GetString(calleeIndex);

                                                var tempstartTime = callRdr.GetDateTime(startTimeIndex);
                                                var tempendTime = callRdr.GetDateTime(endTimeIndex);
                                                var conversationDuration = callRdr.GetString(conversationDurationIndex);

                                                var docid = callRdr.GetString(0);
                                                var doctype = callRdr.GetString(1);
                                                var callType = callRdr.GetString(7);

                                                var tempItem = tempListExportElasticObject.Where(m => m.elasticId == docid).FirstOrDefault();
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

                                                tempItem.call_startTime = tempstartTime.AddHours(7);
                                                tempItem.call_endTime = tempendTime.AddHours(7);
                                                tempItem.call_conversationDuration = conversationDuration;
                                                tempItem.listCGI = new List<Call_Location_Object>();

                                                tempListExportInterceptInfo.Add(tempItem);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        var listSMSAndCallItem = tempListExportInterceptInfo.Where(m => m.document_type == "6").ToList();
                        if (listSMSAndCallItem.Count > 0)
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
                                            var tempValue = callLocationRdr.GetString(3);
                                            var tempType = callLocationRdr.GetString(2);
                                            var tempCall = callLocationRdr.GetString(5);
                                            var cgi = new Call_Location_Object
                                            {
                                                call = tempCall,
                                                id = id,
                                                timestamp = tempTimeStamp.AddHours(7),
                                                value = tempValue,
                                                type = tempType
                                            };
                                            var tempItem = tempListExportInterceptInfo.Where(m => m.elasticId == tempCall).FirstOrDefault();
                                            tempItem.listCGI.Add(cgi);
                                        }
                                        //tempListExportObject.Add(tempObj);
                                    }
                                }
                            }
                        }

                        //Phần truy vấn BSA
                        string BSAConnectionString = helper.getBSAConnectionString();
                        using (MySqlConnection connection2 = new MySqlConnection(BSAConnectionString))
                        {
                            using (MySqlCommand cmd3 = helper.getCellTowerByCellId(connection2, tempListExportInterceptInfo))
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
                                            foreach (var itemFinal in tempListExportInterceptInfo)
                                            {
                                                if (itemFinal.document_type == "19")
                                                {
                                                    if (itemFinal.celltower_cellid == cellid)
                                                    {
                                                        itemFinal.celltower_address = address;
                                                        itemFinal.celltower_latlong = latlong;
                                                    }
                                                }
                                                else
                                                {
                                                    var tempListCGI = itemFinal.listCGI.Where(m => m.value == cellid).ToList();
                                                    if (tempListCGI.Count() > 0)
                                                    {
                                                        tempListCGI[0].address = address;
                                                        tempListCGI[0].latlon = latlong;
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    var finalExportList = tempListExportInterceptInfo;
                    if(finalExportList.Count() > 0)
                    {
                        WriteFile(finalExportList, startTimeWrite, interceptNameObject.CaseName, interceptNameObject.InterceptName);
                    }
                    
                }
                    
            }
            catch (Exception ex)
            {
                txtMultiLog.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    txtMultiLog.Text += Environment.NewLine + "Error when export Intercept Name: " + interceptNameObject.InterceptName + ", InterceptId: " + interceptNameObject.InterceptId + "-" + ex.Message;
                });
            }
        }

        private async Task ExecuteCaseNameAsync(ExportTarget item,string startTime, string endTime,string startTimeWrite)
        {
            var tempListInterceptName = new List<ExportObject>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = helper.getSingleIntellegoCaseAndId(connection, item))
                    {
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                var tempObj = new ExportObject
                                {
                                    CaseName = rdr.GetString(0),
                                    InterceptName = rdr.GetString(1),
                                    InterceptId = rdr.GetString(2)
                                };
                                tempListInterceptName.Add(tempObj);
                            }
                        }
                    }
                }
                List<Task> tasks = new List<Task>();
                foreach (var interceptNameObject in tempListInterceptName)
                {
                    tasks.Add(ProcessIntercept(interceptNameObject,startTime, endTime,startTimeWrite));
                    //ExecuteInterceptName(interceptNameObject, startTime, endTime, startTimeWrite);
                }
                await Task.WhenAll(tasks);
                txtMultiLog.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    txtMultiLog.Text += Environment.NewLine + "Successfully exported " + tempListInterceptName.Count() + " Intercept name";
                });
            }
            catch (Exception ex)
            {
                txtMultiLog.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    txtMultiLog.Text += Environment.NewLine + "Unhandled error when export casename: " + item.TargetName + "-" + ex.Message;
                });

            }

        }

        private void exportSingleData()
        {
            //listTarget = db.ExportTargets.ToList();
            var selectedText = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Value;
            var selectedKey = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Key;
            var tempTarget = new ExportTarget { TargetId = selectedKey, TargetName = selectedText };
            listTarget.Add(tempTarget);
            //var currentTime = new DateTime(2022, 09, 15, 07, 00, 00);
            var startTime = (startTimeDate.Value.Date + startTimeTime.Value.TimeOfDay).AddHours(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endTime = (endTimeDate.Value.Date + endTimeTime.Value.TimeOfDay).AddHours(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");

            //var startTi = startTimeDate.Value.Date + startTimeTime.Value.TimeOfDay;
            //var endTime = endTimeDate.Value.Date + endTimeTime.Value.TimeOfDay;

            var logExportTime = startTime + " - " + endTime;
            var startTimeWrite = new DateTime(startTimeDate.Value.Year, startTimeDate.Value.Month, startTimeDate.Value.Day, startTimeTime.Value.Hour, 0, 0).ToString("yyyy-MM-dd-HH-mm");
            //var tempListExportObject = new List<ExportObject>();
            
            foreach (var item in listTarget)
            {
                //ExecuteCaseName(item, startTime, endTime, startTimeWrite);
                ExecuteCaseNameAsync(item, startTime, endTime, startTimeWrite);
            }
            //await Task.WhenAll(tasks);
        }

        private void exportData()
        {
            listTarget = db.ExportTargets.ToList();
            var tempListExportObject = new List<ExportObject>();
            var currentTime = new DateTime(2022,09,15,07,00,00);
            var startTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0).AddHours(3).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var logExportTime = startTime + " - " + endTime;
            var startTimeWrite = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0).ToString("yyyy-MM-dd-HH-mm");
            var tempListExportObject2 = new List<ExportObject>();
            try
            {
                //Phần truy vấn thông tin list InterceptId&InterceptName by CaseName
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand cmd = helper.getIntellegoCaseAndId(connection, listTarget))
                        {
                            using (MySqlDataReader rdr = cmd.ExecuteReader())
                            {
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
                            }
                        }
                    }
                }
                catch(Exception ex1) {
                    var message = "Exception when retrive list InterceptId&InterceptName By CaseName: " + ex1.Message;
                    txtMultiLog.Text += Environment.NewLine + message;
                    helper.InsertLogToDB(message, StatusInfo.Failed.ToString(),logExportTime,"",DateTime.Now,"","");
                }
                /****Phần truy vấn Elastic****/
                var nodes = new Uri[]
                        {
                            new Uri("http://localhost:9200/"),
                        };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming();
                //var elasticClient = new ElasticClient(connectionSettings);
                var elasticClient = new ElasticClient(connectionSettings.DefaultIndex("intellego"));
                
                foreach (var interceptItem in tempListExportObject)
                {
                    try
                    {
                        var tempQuery = helper.getElasticQuery(interceptItem.InterceptId, startTime, endTime);
                        var searchResult = elasticClient.LowLevel.Search<SearchResponse<ElasticObject>>(tempQuery);
                        var kkk = searchResult.Total;
                        foreach (var item in searchResult.Hits)
                        {
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
                    catch(Exception ex)
                    {
                        var message = "Exception when retrive elasticId: " + interceptItem.InterceptId + " " + ex.Message;
                        txtMultiLog.Text += Environment.NewLine + message;
                        helper.InsertLogToDB(message, StatusInfo.Failed.ToString(),logExportTime,interceptItem.CaseName,DateTime.Now,interceptItem.InterceptId, interceptItem.InterceptName);
                    }
                    
                }

                /****Phần truy vấn MySQL theo type****/
                var tempListExportObjectMySQL2 = new List<ExportObject>();
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        //Phần lấy thông tin Location Update
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

                        //Phần lấy thông tin SMS
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
                                catch (Exception ex) { helper.InsertLogToDB(ex.Message, StatusInfo.Failed.ToString(),logExportTime,item.CaseName,DateTime.Now,item.InterceptId,item.InterceptName); }
                            }
                        }
                        //Phần lấy thông tin Call
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
                                                    int directionIndex = callRdr.GetOrdinal("direction");
                                                    int urlIndex = callRdr.GetOrdinal("url");
                                                    //int metadataIndex = tempRdr.GetOrdinal("metadata");
                                                    int imeiIndex = callRdr.GetOrdinal("imei");
                                                    int imsiIndex = callRdr.GetOrdinal("imsi");

                                                    int callerIndex = callRdr.GetOrdinal("caller");
                                                    int calleeIndex = callRdr.GetOrdinal("callee");

                                                    int startTimeIndex = callRdr.GetOrdinal("startTime");
                                                    int endTimeIndex = callRdr.GetOrdinal("endTime");

                                                    int conversationDurationIndex = callRdr.GetOrdinal("conversationDuration");

                                                    var calldirection = callRdr.IsDBNull(directionIndex) ? string.Empty : callRdr.GetString(directionIndex);
                                                    var imei = callRdr.IsDBNull(imeiIndex) ? string.Empty : callRdr.GetString(imeiIndex);
                                                    var imsi = callRdr.IsDBNull(imsiIndex) ? string.Empty : callRdr.GetString(imsiIndex);
                                                    var url = callRdr.IsDBNull(urlIndex) ? string.Empty : callRdr.GetString(urlIndex);

                                                    var caller = callRdr.IsDBNull(callerIndex) ? string.Empty : callRdr.GetString(callerIndex);
                                                    var callee = callRdr.IsDBNull(calleeIndex) ? string.Empty : callRdr.GetString(calleeIndex);

                                                    var tempstartTime = callRdr.GetDateTime(startTimeIndex);
                                                    var tempendTime =  callRdr.GetDateTime(endTimeIndex);
                                                    var conversationDuration = callRdr.GetString(conversationDurationIndex);

                                                    var docid = callRdr.GetString(0);
                                                    var doctype = callRdr.GetString(1);
                                                    var callType = callRdr.GetString(7);

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

                                                    tempItem.call_startTime = tempstartTime;
                                                    tempItem.call_endTime = tempendTime;
                                                    tempItem.call_conversationDuration = conversationDuration;
                                                    tempItem.listCGI = new List<Call_Location_Object>();

                                                    tempListExportObjectMySQL2.Add(tempItem);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex) { helper.InsertLogToDB(ex.Message, StatusInfo.Failed.ToString(), logExportTime, item.CaseName, DateTime.Now, item.InterceptId, item.InterceptName); }
                            }
                        }

                        /****Phần truy vấn Call_location cho SMS và Call****/
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
                                            var tempValue = callLocationRdr.GetString(3);
                                            var tempType = callLocationRdr.GetString(2);
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
                catch (Exception ex)
                {
                    var message = "Exception when retrive InterceptInfo by type, " + ex.Message;
                    txtMultiLog.Text += Environment.NewLine + message;
                    helper.InsertLogToDB(message, StatusInfo.Failed.ToString(), logExportTime, "", DateTime.Now, "", "");
                }

                /****Phần truy vấn MySQL BSA****/
                var finalExportList = tempListExportObjectMySQL2;
                var listLocationObj = new List<ExportObject>();
                try
                {
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
                                                if (item.celltower_cellid == cellid)
                                                {
                                                    item.celltower_address = address;
                                                    item.celltower_latlong = latlong;
                                                }
                                            }
                                            else
                                            {
                                                var tempListCGI = item.listCGI.Where(m => m.value == cellid).ToList();
                                                if (tempListCGI.Count() > 0)
                                                {
                                                    var tempItemCGI = tempListCGI[0];
                                                    tempItemCGI.address = address;
                                                    tempItemCGI.latlon = latlong;
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    var message = "Exception when retrive BSA info, " + ex.Message;
                    txtMultiLog.Text += Environment.NewLine + message;
                    helper.InsertLogToDB(message, StatusInfo.Failed.ToString(), logExportTime, "", DateTime.Now, "", "");
                }
                

                /****Phần ghi dữ liệu ra file****/
                var finalListByEventDate = finalExportList.OrderBy(m=>m.eventDate).ToList();
                //WriteFile(finalListByEventDate,startTimeWrite);
            }
            /****Kết thúc phần truy vấn MySQL BSA****/
            catch (Exception ex)
            {
                var message = "Unhandled error: " + ex.Message;
                txtMultiLog.Text += Environment.NewLine + message;
                helper.InsertLogToDB(message,StatusInfo.Failed.ToString(),logExportTime,"",DateTime.Now,"","");
                //connection.Close();
            }
        }

        private void WriteFile(List<ExportObject> listExport, string startTime,string casename, string interceptname)
        {
            ///var listItemExport = listExport.Where(m => m.CaseName == item).OrderByDescending(m=>m.eventDate).ToList();

            var convertedInterceptName = "";
            if(interceptname.Substring(0,2) == "84")
            {
                convertedInterceptName = interceptname.Remove(0,2);
                convertedInterceptName = convertedInterceptName.Insert(0, "0");
            }
                string initialData = "[";
                var destinationPath = StaticKey.EXPORT_FOLDER + @"\" + casename + "-" + startTime + @"\" + convertedInterceptName;
                var hi2FullPath = destinationPath + @"\HI2_"+ casename + "_" + interceptname +".json";
                Directory.CreateDirectory(destinationPath);
                foreach (var itemExport in listExport)
                {
                    if (itemExport.document_type == "19")
                    {
                        initialData += getLocationJsonString(itemExport) + ",";
                    }
                    else
                    {
                        if (itemExport.call_type == "4")
                        {
                            initialData += getSMSJsonString(itemExport) + ",";
                        }
                        else
                        {
                            initialData += getCallJsonString(itemExport, destinationPath) + ",";
                        }

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
                System.IO.File.WriteAllText(hi2FullPath, initialData);
        }


        private void btnStop_Click(object sender, EventArgs e)
        {
            //List<int> cont = new List<int>();
            //cont.Add(1);
            //cont.Add(2);
            //cont.Add(3);
            //cont.Add(4);
            //cont.Add(5);
            //cont.Add(6);
            //cont.Add(7);
            //cont.Add(8);
            //cont.Add(9);
            //cont.Add(10);
            //cont.Add(11);
            //cont.Add(12);

            //List<Task> task = new List<Task>();
            //foreach(var abc in cont)
            //{
            //    task.Add(ProcessTask(abc));
            //}
            //await Task.WhenAll(task);
            //var ms = results.ToList();


            //var startTigmt = startTi.AddHours(-7);
            //var endTimegmt = endTime.AddHours(-7);
            var sting1 = "1352565,52652355,1169464";
            var sting2 = "1352565,52652355,1169464,";
            var sm = sting1.TrimEnd(',');
            var sm2 = sting2.TrimEnd(',');
            var dr = "";
        }

        private Task ProcessTask(int id)
        {
            return Task.Run(() => {
                Thread.Sleep(3000);
                //Task.Delay(5000);
                txtMultiLog.Invoke((MethodInvoker)delegate
                {
                    txtMultiLog.Text += Environment.NewLine + id.ToString() + "-" + Thread.CurrentThread.ManagedThreadId;
                });
                //return id;
            });
        }
        private Task ProcessIntercept(ExportObject interceptNameObject,string startTime,string endTime,string startTimeWrite)
        {
            return Task.Run(() =>
            {
                ExecuteInterceptName(interceptNameObject, startTime, endTime, startTimeWrite);
            });
        }

        //private Task ProcessTaskForCaseName(ExportTarget item, string startTime, string endTime, string startTimeWrite)
        //{
        //    return Task.Run(() =>
        //    {
                
        //    });
            
        //}
        private string getSMSJsonString(ExportObject exportObject)
        {
            var data = "";
                var decodedMsg = utility.getDecodedSMSFromUrl(exportObject.call_product_url);
                var caller = exportObject.call_caller == null ? "" : exportObject.call_caller;
                var callee = exportObject.call_callee == null ? "" : exportObject.call_callee;
                string cgi = "", Address = "", timeStamp = "", dateStamp = "", lat = "", lon = "";
                

                var listDic = new string[][] {
                new string[] { "Date:", exportObject.eventDate.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.eventDate.ToString("HH.mm.ss")},
                new string[] { "CalledPartyNumber:",  callee} ,
                new string[] { "CallingPartyNumber:", caller} ,
                new string[] { "IMEI:", exportObject.call_participant_imei == null? "" : exportObject.call_participant_imei },
                new string[] { "IMSI:", exportObject.call_participant_imsi == null? "" : exportObject.call_participant_imsi},
                new string[] { "Monitored Target:", exportObject.InterceptName},
                new string[] { "SMS-transfer-status:", ""},
                new string[] { "Decoded SMS:", decodedMsg},
                new string[] { "Decoded Number:", callee}
                
            };
            var tempDic = listDic.ToList();

            if (exportObject.listCGI.Count() > 0)
            {
                var tempcgiObj = exportObject.listCGI[0];
                cgi = tempcgiObj.value;
                Address = tempcgiObj.address;
                timeStamp = tempcgiObj.timestamp.ToString("HH.mm.ss");
                dateStamp = tempcgiObj.timestamp.ToString("dd/MM/yyyy");
                lat = utility.getLatLonFromString(exportObject.celltower_latlong).lat;
                lon = utility.getLatLonFromString(exportObject.celltower_latlong).lon;
                tempDic.Add(new string[] { "Date:", dateStamp });
                tempDic.Add(new string[] { "Time:", timeStamp });
                tempDic.Add(new string[] { "CGI:", cgi });
                tempDic.Add(new string[] { "Address:", Address });
                tempDic.Add(new string[] { "City:", "" });
                tempDic.Add(new string[] { "Latitude:", lat });
                tempDic.Add(new string[] { "Longitude:", lon });
            }
                var export = new ExportLocation
                {
                    
                    orclId = exportObject.elasticId,
                    phone = exportObject.InterceptName,
                    system = "LMC",
                    targetId = exportObject.InterceptId,
                    liuId = exportObject.elasticId,
                    type = "SMS",
                    sequence = "",
                    displayDate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                    length = "0",
                    network = utility.getNetworkByCGI(exportObject.celltower_cellid),
                    direction = exportObject.call_direction == "1" ? "IN" : "OUT",
                    normCin = callee,
                    lastUpdate = exportObject.eventDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    iriRecords = tempDic.ToArray(),
                    hi3File = "",
                    fullPath = ""
                };
                string json = JsonConvert.SerializeObject(export);
                data += json;
                return data;
            
        }

        private string getCallJsonString(ExportObject exportObject, string destinationPath)
        {
            var data = "";
           
                var caller = exportObject.call_caller == null ? "" : exportObject.call_caller;
                var callee = exportObject.call_callee == null ? "" : exportObject.call_callee;

                var url = exportObject.call_product_url;
                var subUrl =  url.Replace("file://var/intellego/", "").Replace("/", @"\");
                var absoluteUrl = StaticKey.MAIN_URL_FOLDER + subUrl.Trim();

                var fileName = utility.getHI3FilenameFromUrl(exportObject.call_product_url);
                var destinationFullUrl = destinationPath + @"\" + fileName;
                if(absoluteUrl.IndexOf(".wav") > -1)
                {
                    File.Copy(absoluteUrl, destinationFullUrl, true);
                }
                var listDic = new string[][] {
                new string[] { "Date:", exportObject.call_startTime.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.call_startTime.ToString("HH.mm.ss")},
                new string[] { "Date:", exportObject.call_endTime.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.call_endTime.ToString("HH.mm.ss")},
                new string[] { "CalledPartyNumber:",  callee} ,
                new string[] { "CallingPartyNumber:", caller} ,
                new string[] { "IMEI:", exportObject.call_participant_imei == null? "" : exportObject.call_participant_imei },
                new string[] { "IMSI:", exportObject.call_participant_imsi == null? "" : exportObject.call_participant_imsi},
                new string[] { "Monitored Target:", exportObject.InterceptName}
                
                };
                var tempDic = listDic.ToList();

                foreach(var item in exportObject.listCGI)
                {
                    //var tempcgiObj = exportObject.listCGI[0];
                    tempDic.Add(new string[] { "Date:", item.timestamp.ToString("dd/MM/yyyy") });
                    tempDic.Add(new string[] { "Time:", item.timestamp.ToString("HH.mm.ss") });
                    tempDic.Add(new string[] {"CGI:",item.value});
                    tempDic.Add(new string[] { "Address:", item.address });
                    tempDic.Add(new string[] { "City:", item.address });
                    tempDic.Add(new string[] { "Latitude:", utility.getLatLonFromString(exportObject.celltower_latlong).lat });
                    tempDic.Add(new string[] { "Longitude:", utility.getLatLonFromString(exportObject.celltower_latlong).lon });
                }

                var export = new ExportLocation
                {
                    orclId = exportObject.elasticId,
                    phone = exportObject.InterceptName,
                    system = "LMC",
                    targetId = exportObject.InterceptId,
                    liuId = exportObject.elasticId,
                    type = "VOICE",
                    sequence = "",
                    displayDate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                    length = exportObject.call_conversationDuration,
                    network = utility.getNetworkByCGI(exportObject.celltower_cellid),
                    direction = exportObject.call_direction == "1" ? "IN" : "OUT",
                    normCin = callee,
                    lastUpdate = exportObject.eventDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    iriRecords = tempDic.ToArray(),
                    hi3File = fileName,
                    fullPath = ""
                };
                string json = JsonConvert.SerializeObject(export);
                data += json;
                return data;
           
        }

        private string getLocationJsonString(ExportObject exportObject)
        {
            var data = "";
            
                //var latlon = "['a','d']";
                var listDic = new string[][] {
                new string[] { "Date:", exportObject.eventDate.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.eventDate.ToString("HH.mm.ss")},
                new string[] { "CGI:", exportObject.celltower_cellid} ,
                new string[] { "Address:", exportObject.celltower_address == null? "" : exportObject.celltower_address },
                new string[] { "City:", ""},
                new string[] { "Latitude:", utility.getLatLonFromString(exportObject.celltower_latlong).lat},
                new string[] { "Longitude:", utility.getLatLonFromString(exportObject.celltower_latlong).lon},
            };
                var export = new ExportLocation
                {
                    orclId = exportObject.elasticId,
                    phone = exportObject.InterceptName,
                    system = "LMC",
                    targetId = exportObject.InterceptId,
                    liuId = exportObject.elasticId,
                    type = "CELL",
                    sequence = null,
                    displayDate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                    length = null,
                    network = utility.getNetworkByCGI(exportObject.celltower_cellid),
                    direction = null,
                    normCin = null,
                    lastUpdate = exportObject.eventDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    iriRecords = listDic,
                    hi3File = "",
                    fullPath = ""
                };
                string json = JsonConvert.SerializeObject(export);
                data += json;
                return data ;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            
        }

        private void Main_Load(object sender, EventArgs e)
        {
            timer1.Interval = 120000; // specify interval time as you want
            timer1.Tick += new EventHandler(timer1_Tick);
            connectionString = helper.getConnectionString();

            var listObj = new List<CaseObject>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = helper.getListCaseName(connection))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var tempObj = new CaseObject
                            {
                                id = rdr.GetString(0),
                                name = rdr.GetString(1),
                            };
                            listObj.Add(tempObj);
                        }
                    }
                }
            }
            Dictionary<string, string> test = new Dictionary<string, string>();
            foreach (var item in listObj)
            {
                test.Add(item.id, item.name);
            }
            comboBox1.DataSource = new BindingSource(test, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

        }
    }
}


