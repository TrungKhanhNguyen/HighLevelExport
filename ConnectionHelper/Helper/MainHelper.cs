using ConnectionHelper.Models;
using Elasticsearch.Net;
using MySql.Data.MySqlClient;
using Nest;
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

        //Lấy danh sách tất cả các case
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
                                //brief = rdr.GetString(2),
                                //owner = rdr.GetString(3),
                                //dateCreated = rdr.GetString(4),
                                //dateUpdated = rdr.GetString(5),
                                //sensitivity = rdr.GetString(6),
                                //priority    = rdr.GetString(7),
                                //status = rdr.GetString(8),
                                //group = rdr.GetString(9),
                                //trashedTime = rdr.GetString(10),
                            };
                            listObj.Add(tempObj);
                        }
                    }
                }
            }
            return listObj;
        }

        //Lấy danh sách các Intercept theo Case name
        public List<ExportObject> GetListInterceptName(ExportTarget item)
        {
            var tempList = new List<ExportObject>();
            string connectionString = helper.getConnectionString();
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
                            tempList.Add(tempObj);
                        }
                    }
                }
            }
            return tempList;
        }

        //Export thông tin theo từng Intercept
        public List<ExportObject> ExecuteInterceptName(ExportObject interceptNameObject, string startTime, string endTime, string startTimeWrite)
        {
            //if (interceptNameObject.InterceptId != "894")
            //{
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
                                            var tempendTime = callRdr.GetDateTime(endTimeIndex);
                                            var conversationDuration = callRdr.GetString(conversationDurationIndex);

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
                                            tempItem.call_endTime = tempendTime.AddHours(7);
                                            tempItem.call_conversationDuration = conversationDuration;
                                            tempItem.listCGI = new List<Call_Location_Object>();

                                            tempListExportInterceptInfo.Add(tempItem);

                                            tempListCall.Add(tempItem);

                                            listId.Add(tempItem.elasticId);
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
                finalList = tempListExportInterceptInfo.OrderBy(m => m.eventDate).ToList();
            }
            return finalList;
            //}
            //else return new List<ExportObject>();
        }
    }
}
