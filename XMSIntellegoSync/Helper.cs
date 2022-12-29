using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace XMSIntellegoSync
{
    public class Helper
    {
        public string getIntellgoConnectionString()
        {
            string connStr = string.Empty;
            connStr = String.Format("server={0};port={1};user id={2}; password={3}; database={4}; SslMode={5}",
                SyncStaticKey.INTELLEGO_SERVER_IP, SyncStaticKey.INTELLEGO_PORT, SyncStaticKey.INTELLEGO_USER_NAME, SyncStaticKey.INTELLEGO_PASSWORD, SyncStaticKey.INTELLEGO_DATABASE_NAME, "None");
            return connStr;
        }

        public string getXCDBConnectionString()
        {
            string connStr = string.Empty;
            connStr = String.Format("server={0};port={1};user id={2}; password={3}; database={4}; SslMode={5}",
                SyncStaticKey.XCP_SERVER_IP, SyncStaticKey.XCP_PORT, SyncStaticKey.XCP_USER_NAME, SyncStaticKey.XCP_PASSWORD, SyncStaticKey.XCP_DATABASE_NAME, "None");
            return connStr;
        }

        //I.1.Lấy danh sách các intercept được mới trên Intellego
        public MySqlCommand getInterceptInfo(MySqlConnection connection,string fromDate, string toDate)
        {
            string sql = "SELECT intercept.id, intercept.name FROM intellego.intercept where dateCreated between '"+fromDate+"' and '"+toDate+"';";
            //string sql = "SELECT intercept.id, intercept.name FROM intellego.intercept where id = '2621'";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        //I.2.Lấy thông tin của các intercept trong csdl của Xcipio theo danh sách đã lấy ở I.1
        public MySqlCommand getListXcipioInfo(MySqlConnection connection, List<NewSyncObject> listExport)
        {
            string listItem = "";
            foreach (var item in listExport)
            {
                listItem += item.InterceptName;
                var itemIndex = listExport.IndexOf(item);
                if (itemIndex != listExport.Count() - 1)
                {
                    listItem += ",";
                }
            }
            string sql = "";
            if (String.IsNullOrEmpty(listItem))
            {
                sql = "select * from SURVEILLANCE where OBJECT_ID = 0";
            }
            else
            {
                sql = "select a.CASEID, b.NAME, d.OPTIONVALUE, a.STOPDATETIME, a.MOD_TS, a.STATE from SURVEILLANCE a, COGRP b, CO c, TARGETOPTS d where b.GID=c.GID and a.COID=c.COID and d.COID=a.COID and a.CASEID in (" + listItem + ");";
            }
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        //Lấy danh sách các intercept  được thay đổi trên Xcipio trong bảng Surveillance
        public MySqlCommand getListUpdatedIntercept(MySqlConnection connection, string fromDate, string toDate)
        {
            string sql = "select a.CASEID, b.NAME, d.OPTIONVALUE, a.STOPDATETIME, a.MOD_TS, a.STATE from SURVEILLANCE a, COGRP b, CO c, TARGETOPTS d where b.GID=c.GID and a.COID=c.COID and d.COID=a.COID and a.MOD_TS between '"+ fromDate + "' and  '"+toDate+"';";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        //update thông tin trên Xcipio với STATE = ACTIVE
        public MySqlCommand updateActiveRecordOnXcipio(MySqlConnection connection,string name,string optionvalue, string CASEID)
        {
            string sql = "Update intercept SET intercept.expiration_date = current_timestamp(), intercept.description = '"+ name+"', intercept.interceptType = 'CC' where intercept.name = '"+CASEID+"';";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand updateProvinceNameAndCaseIntercept(MySqlConnection connection, string provinceName,string intercept, string id)
        {
            string sql = "";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        //Update thông tin phần địa phương phần 2.1
        public MySqlCommand updateInterceptAndCaseIntercept(MySqlConnection connection,string interceptid, string caseid, string provinceName)
        {
            string sql = "update intercept set provinceName = '"+provinceName+"' where id = '"+interceptid+"'; INSERT INTO case_intercept (intercept, case_intercept.case) SELECT * FROM (SELECT '"+interceptid+"' AS intercept, '"+caseid+"' AS 'case') AS temp WHERE NOT EXISTS (SELECT intercept, case_intercept.case FROM case_intercept WHERE intercept = '"+interceptid+"' AND case_intercept.case = '"+caseid+"') LIMIT 1;";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getListInterceptIdByName(MySqlConnection connection, List<NewSyncObject> listObject)
        {
           
            string listItem = "";
            foreach (var item in listObject)
            {
                listItem += item.CASEID;
                var itemIndex = listObject.IndexOf(item);
                if (itemIndex != listObject.Count() - 1)
                {
                    listItem += ",";
                }
            }
            string sql = "";
            if (String.IsNullOrEmpty(listItem))
            {
                sql = "select * from intellego.intercept where id = 0";
            }
            else
            {
                sql = "select id,name from intellego.intercept where name in ( " + listItem + ");";
            }
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        public MySqlCommand getInterceptIdByName(MySqlConnection connection, string name)
        {
            string sql = "SELECT intercept.id, intercept.name FROM intellego.intercept WHERE intercept.name = '" + name + "'";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        //Update trashtime với các Intercept của Địa phương
        public MySqlCommand updateInterceptTrashTime(MySqlConnection connection,string interceptid)
        {
            string sql = "UPDATE intercept a JOIN case_intercept b ON a.id= '"+interceptid+"' AND a.id=b.intercept AND b.case in (273, 274, 275, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300) SET a.trashedTime=current_timestamp();";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

        //Update suspendedtime với các Intercept KHÔNG của Địa phương
        public MySqlCommand updateInterceptSuspendedTime(MySqlConnection connection, string interceptid)
        {
            string sql = "UPDATE intercept a JOIN case_intercept b ON a.id= '"+interceptid+"' AND a.id=b.intercept AND b.case not in (273, 274, 275, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300) SET a.suspendedTime=current_timestamp();";
            var cmd = new MySqlCommand(sql, connection);
            return cmd;
        }

    }
}
