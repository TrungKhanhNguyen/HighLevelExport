using ConnectionHelper.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionHelper.Helper
{
    public class DBHelper
    {
        
        public string getConnectionString()
        {
            string connStr = string.Empty;
            connStr = String.Format("server={0};port={1};user id={2}; password={3}; database={4}; SslMode={5}",
                StaticKey.SERVER_IP, StaticKey.PORT, StaticKey.USER_NAME, StaticKey.PASSWORD, StaticKey.DATABASE_NAME, "None");
            return connStr;
        }

        public string getBSAConnectionString()
        {
            string connStr = string.Empty;
            connStr = String.Format("server={0};port={1};user id={2}; password={3}; database={4}; SslMode={5}",
                StaticKey.BSA_SERVER_IP, StaticKey.PORT, StaticKey.BSA_USER_NAME, StaticKey.BSA_PASSWORD, StaticKey.BSA_DATABASE_NAME, "None");
            return connStr;
        }

        
        public MySqlCommand getListCaseName(MySqlConnection connection)
        {
            string sql = "SELECT * FROM intellego.case";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getAllIntelleoIntercept(MySqlConnection connection)
        {
            string sql = "select name, id from intercept;";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getSingleIntellegoCaseAndId(MySqlConnection connection, string casename)
        {
            string sql = "select b.name as CaseName, a.name as InterceptName, a.id as InterceptId from intellego.intercept a, intellego.case b, intellego.case_intercept c where b.name = '" + casename + "' and b.id = c.case  and c.intercept = a.id order by a.id";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getSingleIntellegoCaseAndId2Mins(MySqlConnection connection, string casename)
        {
            string sql = "select b.name as CaseName, a.name as InterceptName, a.id as InterceptId, a.description as Description from intellego.intercept a, intellego.case b, intellego.case_intercept c where b.name = '" + casename + "' and b.id = c.case  and c.intercept = a.id order by a.id";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getLocationUpdateInterceptInfo(MySqlConnection connection, List<ExportObject> listExport)
        {
            string listItem = "";
            foreach (var item in listExport)
            {
                listItem += item.elasticId;
                var itemIndex = listExport.IndexOf(item);
                if (itemIndex != listExport.Count() - 1)
                {
                    listItem += ",";
                }
            }
            string sql = "";
            if (String.IsNullOrEmpty(listItem))
            {
                sql = "select * from intellego_events.document where id = 0";
            }
            else
            {
                sql = "select * from intellego_events.document where id in ( " + listItem + ") and type = 19;";
            }
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getLostHI2Info(MySqlConnection connection, List<ExportObject> listExport)
        {
            string listItem = "";
            foreach (var item in listExport)
            {
                listItem += item.elasticId;
                var itemIndex = listExport.IndexOf(item);
                if (itemIndex != listExport.Count() - 1)
                {
                    listItem += ",";
                }
            }
            string sql = "";
            if (String.IsNullOrEmpty(listItem))
            {
                sql = "select * from intellego_events.document where id = 0";
            }
            else
            {
                sql = "select a.id, a.type, b.*, c.* from intellego_events.document a, intellego_events.call b, intellego_events.call_product c where a.id in ("+ listItem +") and a.type=6 and b.type in (1, 2) and a.id=b.id and a.id=c.call ;";
            }
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }


        public MySqlCommand getSMSInterceptInfo(MySqlConnection connection, ExportObject tempObj)
        {
            var sql = "select a.id ,a.type, b.*, c.*, d.* from intellego_events.document a, intellego_events.call b, intellego_events.call_product c, intellego_events.call_participant d where a.id = " + tempObj.elasticId + " and a.type = 6 and b.type = 4 and a.id = b.id and a.id = c.call and b.id = d.call and d.imsi != '' ;";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getCallInterceptInfo(MySqlConnection connection, ExportObject tempObj)
        {
            var sql = "select a.id, a.type, b.*, c.*, d.* from intellego_events.document a, intellego_events.call b, intellego_events.call_product c, intellego_events.call_participant d where a.id = " + tempObj.elasticId + " and a.type = 6 and b.type in (1, 2) and a.id = b.id and a.id = c.call and a.id=d.call and d.imsi != '' ;";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getCallLocationInfo(MySqlConnection connection, List<ExportObject> listExport)
        {
            string listItem = "";
            foreach (var item in listExport)
            {

                listItem += item.elasticId;
                var itemIndex = listExport.IndexOf(item);
                listItem += ",";

            }
            string sql = "";
            if (string.IsNullOrEmpty(listItem))
            {
                sql = "SELECT * FROM intellego_events.call_location where id < 1;";
            }
            else
            {
                sql = "SELECT * FROM intellego_events.call_location where call_location.call in ( " + listItem.TrimEnd(',') + ");";
            }

            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }
        public MySqlCommand getSingleCellTowerByCellId(MySqlConnection connection, ExportObject tempExport)
        {
            string listItem = "";
            foreach(var item in tempExport.listCGI)
            {
                listItem += item.value;
                var cgiIndex = tempExport.listCGI.IndexOf(item);
                if (cgiIndex != tempExport.listCGI.Count() - 1)
                {
                    listItem += ",";
                }
            }
            string sql = "";
            if (String.IsNullOrEmpty(listItem))
            {
                sql = "SELECT cellid,address,latlong FROM slpdb_controller.celltower where celltower.type = -1";
            }
            else
            {
                sql = "SELECT cellid,address,latlong FROM slpdb_controller.celltower where cellid in (" + listItem + ") and celltower.type = 'CGI'";
            }
            var indexOfCloseCharacter = sql.LastIndexOf(')');
            var indexOfLastComma = sql.LastIndexOf(',');
            if (indexOfCloseCharacter > -1)
            {
                if ((indexOfLastComma + 1) == indexOfCloseCharacter)
                {
                    sql = sql.Remove(indexOfCloseCharacter - 1, 1);
                }
            }
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getCellTowerByCellId(MySqlConnection connection, List<ExportObject> listExport)
        {
            string listItem = "";
            foreach (var item in listExport)
            {
                if (item.document_type == "19")
                {
                    if (!String.IsNullOrEmpty(item.celltower_cellid))
                    {
                        listItem += item.celltower_cellid;

                        var itemIndex = listExport.IndexOf(item);
                        if (itemIndex != listExport.Count() - 1)
                        {
                            listItem += ",";
                        }
                    }
                }
                else
                {
                    foreach (var itemCGI in item.listCGI)
                    {
                        if (!String.IsNullOrEmpty(itemCGI.value))
                        {
                            listItem += itemCGI.value;
                            var cgiIndex = item.listCGI.IndexOf(itemCGI);
                            if (cgiIndex != item.listCGI.Count() - 1)
                            {
                                listItem += ",";
                            }
                            else
                            {
                                var itemIndex = listExport.IndexOf(item);
                                if (itemIndex != listExport.Count() - 1)
                                {
                                    listItem += ",";
                                }
                            }
                        }

                    }
                }

            }
            string sql = "";
            if (String.IsNullOrEmpty(listItem))
            {
                sql = "SELECT cellid,address,latlong FROM slpdb_controller.celltower where celltower.type = -1";
            }
            else
            {
                sql = "SELECT cellid,address,latlong FROM slpdb_controller.celltower where cellid in (" + listItem + ") and celltower.type = 'CGI'";
            }
            var indexOfCloseCharacter = sql.LastIndexOf(')');
            var indexOfLastComma = sql.LastIndexOf(',');
            if (indexOfCloseCharacter > -1)
            {
                if ((indexOfLastComma + 1) == indexOfCloseCharacter)
                {
                    sql = sql.Remove(indexOfCloseCharacter - 1, 1);
                }
            }
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getListBackupIntercept(MySqlConnection connection)
        {
            var sql = "select intercept.id, intercept.name, dateCreated, expiration_date from intercept where intercept.suspendedTime!='';";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public string getElasticQuery(string interceptid, string fromDate, string endDate)
        {
            var query = @"{
            ""size"": 1000000,
            ""query"": {
                ""bool"": {
                    ""must"": [
                        {
                        ""bool"": {
                            ""must"": [
                                {
                                ""match"": {
                                    ""intercept"": ""intercepid""
                                }}]}},
   {""bool"" : {""must"" : [{""match"":{""relevance"":{""query"":""1""}}}]}},
                        {
                        ""bool"": {
                            ""must"": [
                                {
                                ""range"": {
                                    ""eventDate"": {
                                        ""from"": ""fromDate"",
                                        ""to"": ""endDate""
                                    }}}]}}]}},
            ""docvalue_fields"": [
                {
                        ""field"": ""eventDate"",
                    ""format"": ""date_time""
                }]}";
            var returnQuery = query.Replace("intercepid", interceptid).Replace("fromDate", fromDate).Replace("endDate", endDate);
            return returnQuery;
        }
    }
}
