using ConnectionHelper.Models;
using Elasticsearch.Net;
using MySql.Data.MySqlClient;
using Nest;
using Org.BouncyCastle.Asn1.X500;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionHelper.Helper
{
    public class MainHelper
    {
        private DBHelper helper = new DBHelper();
        public SQLServerHelper sqlServerHelper = new SQLServerHelper();

        public List<CaseObject> GetListCaseObject()
        {
            string connectionString = helper.getConnectionString();
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
            return listObj;
        }

        public List<ExportObject> GetListIntercept2MinsName(string casename)
        {
            var tempList = new List<ExportObject>();
            string connectionString = helper.getConnectionString();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = helper.getSingleIntellegoCaseAndId2Mins(connection, casename))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var tempObj = new ExportObject
                            {
                                CaseName = rdr.GetString(3),
                                InterceptName = rdr.GetString(1),
                                InterceptId = rdr.GetString(2),
                            };
                            tempList.Add(tempObj);
                        }
                    }
                }
            }
            return tempList;
        }

        //Lấy danh sách các Intercept theo Case name
        public List<ExportObject> GetListInterceptName(string casename)
        {
            var tempList = new List<ExportObject>();
            string connectionString = helper.getConnectionString();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = helper.getSingleIntellegoCaseAndId(connection, casename))
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
                            tempList.Add(tempObj);
                        }
                    }
                }
            }
            return tempList;
        }

        public void UpdateTrashedTime(string id)
        {
            var conn = helper.getConnectionString();
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                connection.Open();
                var sql = "UPDATE intercept SET intercept.trashedTime=current_timestamp(), intercept.suspendedTime= null where intercept.id= '" + id + "';";
                var cmd = new MySqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
            }
        }

        public List<ExportObject> GetListBackupObject()
        {
            var tempList = new List<ExportObject>();
            string connectionString = helper.getConnectionString();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = helper.getListBackupIntercept(connection))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            try
                            {
                                var tempObj = new ExportObject
                                {
                                    CaseName = "",
                                    InterceptName = rdr.GetString(1),
                                    InterceptId = rdr.GetString(0),
                                    dateCreated = rdr.GetDateTime(2),
                                    expiration_date = rdr.GetDateTime(3)
                                };
                                tempList.Add(tempObj);
                            }
                            catch { }

                        }
                    }
                }
            }
            return tempList;
        }

        //public List<ExportObject> ExecuteReExportObject(CallReExport item)
        //{
        //    string connectionString = helper.getConnectionString();
        //    var itemElastic = new ExportObject
        //    {
        //        CaseName = item.Casename,
        //        InterceptId = item.InterceptId,
        //        InterceptName = item.InterceptName,
        //        eventDate = Convert.ToDateTime(item.EventDate),
        //        elasticId = item.ElasticId
        //    };
        //    var newListItem = new List<ExportObject>();
        //    using (MySqlConnection connection = new MySqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        using (MySqlCommand callCmd = helper.getCallInterceptInfo(connection, itemElastic))
        //        {
        //            using (MySqlDataReader callRdr = callCmd.ExecuteReader())
        //            {
        //                while (callRdr.Read())
        //                {
        //                    if (callRdr.HasRows)
        //                    {
        //                        int startTimeIndex = callRdr.GetOrdinal("startTime");
        //                        int endTimeIndex = callRdr.GetOrdinal("endTime");

        //                        if (!callRdr.IsDBNull(endTimeIndex))
        //                        {
        //                            int directionIndex = callRdr.GetOrdinal("direction");
        //                            int urlIndex = callRdr.GetOrdinal("url");
        //                            //int metadataIndex = tempRdr.GetOrdinal("metadata");
        //                            int imeiIndex = callRdr.GetOrdinal("imei");
        //                            int imsiIndex = callRdr.GetOrdinal("imsi");

        //                            int callerIndex = callRdr.GetOrdinal("caller");
        //                            int calleeIndex = callRdr.GetOrdinal("callee");

        //                            int conversationDurationIndex = callRdr.GetOrdinal("conversationDuration");

        //                            var calldirection = callRdr.IsDBNull(directionIndex) ? string.Empty : callRdr.GetString(directionIndex);
        //                            var imei = callRdr.IsDBNull(imeiIndex) ? string.Empty : callRdr.GetString(imeiIndex);
        //                            var imsi = callRdr.IsDBNull(imsiIndex) ? string.Empty : callRdr.GetString(imsiIndex);
        //                            var url = callRdr.IsDBNull(urlIndex) ? string.Empty : callRdr.GetString(urlIndex);

        //                            var caller = callRdr.IsDBNull(callerIndex) ? string.Empty : callRdr.GetString(callerIndex);
        //                            var callee = callRdr.IsDBNull(calleeIndex) ? string.Empty : callRdr.GetString(calleeIndex);

        //                            var tempstartTime = callRdr.GetDateTime(startTimeIndex);
        //                            var conversationDuration = callRdr.IsDBNull(conversationDurationIndex) ? string.Empty : callRdr.GetString(conversationDurationIndex);

        //                            var docid = callRdr.GetString(0);
        //                            var doctype = callRdr.GetString(1);
        //                            var callType = callRdr.GetString(6);

        //                            //var tempItem = tempListExportElasticObject.Where(m => m.elasticId == docid).FirstOrDefault();
        //                            itemElastic.document_type = doctype;
        //                            //tempItem.document_metadata = metadata;
        //                            itemElastic.document_id = docid;
        //                            itemElastic.call_direction = calldirection;
        //                            itemElastic.call_participant_imei = imei;
        //                            itemElastic.call_participant_imsi = imsi;
        //                            itemElastic.call_product_url = url;
        //                            itemElastic.call_type = callType;
        //                            itemElastic.call_caller = caller;
        //                            itemElastic.call_callee = callee;

        //                            itemElastic.call_startTime = tempstartTime.AddHours(7);

        //                            itemElastic.call_conversationDuration = conversationDuration;
        //                            itemElastic.listCGI = new List<Call_Location_Object>();

        //                            itemElastic.call_endTime = callRdr.GetDateTime(endTimeIndex).AddHours(7);

        //                            newListItem.Add(itemElastic);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if(newListItem.Count() > 0)
        //        {
        //            //Location
        //            using (MySqlCommand cmdCallLocation = helper.getCallLocationInfo(connection, newListItem))
        //            {
        //                using (MySqlDataReader callLocationRdr = cmdCallLocation.ExecuteReader())
        //                {
        //                    while (callLocationRdr.Read())
        //                    {
        //                        if (callLocationRdr.HasRows)
        //                        {
        //                            var id = callLocationRdr.GetString(0);
        //                            var tempTimeStamp = callLocationRdr.GetDateTime(1);
        //                            var tempValue = callLocationRdr.GetString(3);
        //                            var tempType = callLocationRdr.GetString(2);
        //                            var tempCall = callLocationRdr.GetString(5);
        //                            var cgi = new Call_Location_Object
        //                            {
        //                                call = tempCall,
        //                                id = id,
        //                                timestamp = tempTimeStamp.AddHours(7),
        //                                value = tempValue,
        //                                type = tempType
        //                            };
        //                            var tempItem = newListItem.Where(m => m.elasticId == tempCall).FirstOrDefault();
        //                            tempItem.listCGI.Add(cgi);
        //                        }
        //                        //tempListExportObject.Add(tempObj);
        //                    }
        //                }
        //            }

        //            //BSA
        //            string BSAConnectionString = helper.getBSAConnectionString();
        //            using (MySqlConnection connection2 = new MySqlConnection(BSAConnectionString))
        //            {
        //                using (MySqlCommand cmd3 = helper.getCellTowerByCellId(connection2, newListItem))
        //                {
        //                    connection2.Open();
        //                    using (MySqlDataReader reader = cmd3.ExecuteReader())
        //                    {
        //                        while (reader.Read())
        //                        {
        //                            if (reader.HasRows)
        //                            {
        //                                var cellid = reader.GetString(0);
        //                                var address = reader.GetString(1);
        //                                var latlong = reader.GetString(2);
        //                                foreach (var itemFinal in newListItem)
        //                                {
        //                                    if (itemFinal.document_type == "19")
        //                                    {
        //                                        if (itemFinal.celltower_cellid == cellid)
        //                                        {
        //                                            itemFinal.celltower_address = address;
        //                                            itemFinal.celltower_latlong = latlong;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        var tempListCGI = itemFinal.listCGI.Where(m => m.value == cellid).ToList();
        //                                        if (tempListCGI.Count() > 0)
        //                                        {
        //                                            tempListCGI[0].address = address;
        //                                            tempListCGI[0].latlon = latlong;
        //                                        }
        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        return newListItem;
        //    }
            
        //}

        //Export thông tin theo từng Intercept
        public List<ExportObject> ExecuteInterceptName(ExportObject interceptNameObject, string startTime, string endTime,string writeTime, string type)
        {
                string connectionString = helper.getConnectionString();
                var finalList = new List<ExportObject>();
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
                    //if (itemHit.Id == "128484161")
                    //{
                        var tempObj = new ExportObject
                        {
                            CaseName = interceptNameObject.CaseName,
                            InterceptName = interceptNameObject.InterceptName,
                            InterceptId = interceptNameObject.InterceptId,
                            elasticId = itemHit.Id,
                            eventDate = eventDate.AddHours(7)
                        };
                        tempListExportElasticObject.Add(tempObj);
                    //}

            }

                if (tempListExportElasticObject.Count() > 0)
                {
                    
                    /****Phần truy vấn MySQL theo type****/
                    var listId = new List<string>();

                    var tempListLocation = new List<ExportObject>();
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
                                    tempListLocation.Add(tempItem);
                                    listId.Add(docid);
                                }
                                //tempListExportObject.Add(tempObj);
                            }
                        }
                    }

                    var tempListSMS = new List<ExportObject>();
                    //Phần lấy thông tin SMS
                    foreach (var itemElastic in tempListExportElasticObject)
                    {
                        if (listId.IndexOf(itemElastic.elasticId) < 0)
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
                                            var callType = tempRdr.GetString(6);

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
                                            tempListSMS.Add(tempItem);
                                            listId.Add(tempItem.elasticId);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var tempListCall = new List<ExportObject>();
                    //Phần lấy thông tin Call
                    foreach (var itemElastic in tempListExportElasticObject)
                    {
                        if (listId.IndexOf(itemElastic.elasticId) < 0)
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
                                            var conversationDuration = callRdr.IsDBNull(conversationDurationIndex) ? string.Empty : callRdr.GetString(conversationDurationIndex);

                                            

                                            var docid = callRdr.GetString(0);
                                            var doctype = callRdr.GetString(1);
                                            var callType = callRdr.GetString(6);

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
                                            
                                            tempItem.call_conversationDuration = conversationDuration;
                                            tempItem.listCGI = new List<Call_Location_Object>();

                                            //var tempendTimeString = callRdr.GetDateTime(endTimeIndex);
                                            if(!callRdr.IsDBNull(endTimeIndex))
                                            {
                                                tempItem.call_endTime = callRdr.GetDateTime(endTimeIndex).AddHours(7);
                                            }
                                            else
                                            {
                                                //if(type != ReExportType.Backup.ToString())
                                                //    sqlServerHelper.InsertToReExport(tempItem.CaseName, tempItem.InterceptName, tempItem.InterceptId, tempItem.elasticId, tempItem.eventDate, type, writeTime);
                                            }

                                            tempListExportInterceptInfo.Add(tempItem);

                                            tempListCall.Add(tempItem);

                                            listId.Add(tempItem.elasticId);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var listLostHI2 = tempListExportElasticObject.Except(tempListExportInterceptInfo).ToList();
                    using (MySqlCommand cmdLost = helper.getLostHI2Info(connection, listLostHI2))
                    {
                        using (MySqlDataReader readerLost = cmdLost.ExecuteReader())
                        {
                            while (readerLost.Read())
                            {
                                if (readerLost.HasRows)
                                {
                                    int directionIndex = readerLost.GetOrdinal("direction");
                                    int urlIndex = readerLost.GetOrdinal("url");
                                    //int metadataIndex = tempRdr.GetOrdinal("metadata");

                                    //int imeiIndex = readerLost.GetOrdinal("imei");

                                    int imsiIndex = 0;
                                    int imeiIndex = 0;
                                    try
                                    {
                                        imsiIndex = readerLost.GetOrdinal("imsi");
                                        imeiIndex = readerLost.GetOrdinal("imei");
                                    }
                                    catch
                                    {

                                    }
                                    

                                    int callerIndex = readerLost.GetOrdinal("caller");
                                    int calleeIndex = readerLost.GetOrdinal("callee");

                                    int startTimeIndex = readerLost.GetOrdinal("startTime");
                                    int endTimeIndex = readerLost.GetOrdinal("endTime");

                                    int conversationDurationIndex = readerLost.GetOrdinal("conversationDuration");

                                    var calldirection = readerLost.IsDBNull(directionIndex) ? string.Empty : readerLost.GetString(directionIndex);
                                    var imei = (imeiIndex == 0) ? string.Empty : readerLost.GetString(imeiIndex);
                                    var imsi = (imsiIndex == 0 )? string.Empty : readerLost.GetString(imsiIndex);
                                    var url = readerLost.IsDBNull(urlIndex) ? string.Empty : readerLost.GetString(urlIndex);

                                    var caller = readerLost.IsDBNull(callerIndex) ? string.Empty : readerLost.GetString(callerIndex);
                                    var callee = readerLost.IsDBNull(calleeIndex) ? string.Empty : readerLost.GetString(calleeIndex);

                                    var tempstartTime = readerLost.GetDateTime(startTimeIndex);
                                    var conversationDuration = readerLost.IsDBNull(conversationDurationIndex) ? string.Empty : readerLost.GetString(conversationDurationIndex);



                                    var docid = readerLost.GetString(0);
                                    var doctype = readerLost.GetString(1);
                                    var callType = readerLost.GetString(6);

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

                                    tempItem.call_conversationDuration = conversationDuration;
                                    tempItem.listCGI = new List<Call_Location_Object>();

                                    //var tempendTimeString = callRdr.GetDateTime(endTimeIndex);
                                    if (!readerLost.IsDBNull(endTimeIndex))
                                    {
                                        tempItem.call_endTime = readerLost.GetDateTime(endTimeIndex).AddHours(7);
                                    }
                                    else
                                    {
                                        //if (type != ReExportType.Backup.ToString())
                                        //    sqlServerHelper.InsertToReExport(tempItem.CaseName, tempItem.InterceptName, tempItem.InterceptId, tempItem.elasticId, tempItem.eventDate, type, writeTime);
                                    }

                                    tempListExportInterceptInfo.Add(tempItem);

                                    tempListCall.Add(tempItem);

                                    listId.Add(tempItem.elasticId);
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

                    if(type != ReExportType.Backup.ToString())
                    {
                        //Phần truy vấn BSA
                        try
                        {
                            string BSAConnectionString = helper.getBSAConnectionString();
                            using (MySqlConnection connection2 = new MySqlConnection(BSAConnectionString))
                            {
                                connection2.Open();
                                if (type == ReExportType.Backup.ToString())
                                {
                                    foreach (var itemExport in tempListExportInterceptInfo)
                                    {
                                        using (MySqlCommand cmd4 = helper.getSingleCellTowerByCellId(connection2, itemExport))
                                        {
                                            using (MySqlDataReader reader = cmd4.ExecuteReader())
                                            {
                                                while (reader.Read())
                                                {
                                                    if (reader.HasRows)
                                                    {
                                                        var cellid = reader.GetString(0);
                                                        var address = reader.GetString(1);
                                                        var latlong = reader.GetString(2);

                                                        if (itemExport.document_type == "19")
                                                        {
                                                            if (itemExport.celltower_cellid == cellid)
                                                            {
                                                                itemExport.celltower_address = address;
                                                                itemExport.celltower_latlong = latlong;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var tempListCGI = itemExport.listCGI.Where(m => m.value == cellid).ToList();
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
                                else
                                {
                                    using (MySqlCommand cmd3 = helper.getCellTowerByCellId(connection2, tempListExportInterceptInfo))
                                    {
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
                        }
                        catch { }
                    }
                    
                }
                finalList = tempListExportInterceptInfo.OrderBy(m => m.eventDate).ToList();
            }
            return finalList;
            //}
            //else return new List<ExportObject>();
        }
    }
}
