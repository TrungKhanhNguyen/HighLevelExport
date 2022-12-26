using MySql.Data.MySqlClient;
using Quartz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMSIntellegoSync
{
    public class MainJob : IJob
    {
        private Helper helper = new Helper();
        private ExportHistoryEntities db = new ExportHistoryEntities();
        public void Execute(IJobExecutionContext context)
        {
            //throw new NotImplementedException();
            SyncData();
        }

        public void SyncData()
        {
            var currentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 00, 00);
            //var currentTime = new DateTime(DateTime.Now.Year, 12, 07, 11, 00, 00);

            var startTime = (currentTime).AddHours(-8).ToString("yyyy-MM-dd HH:mm:ss");
            var endTime = (currentTime).AddHours(-7).ToString("yyyy-MM-dd HH:mm:ss");

            getListSyncObject(startTime, endTime);
        }
        private List<NewSyncObject> getListSyncObject(string startTime, string endTime)
        {
            string connectionString = helper.getIntellgoConnectionString();
            string xcpconnectionString = helper.getXCDBConnectionString();
            //var fromDate = DateTime.Now;
            var listObject = new List<NewSyncObject>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = helper.getInterceptInfo(connection,startTime,endTime))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var tempObj = new NewSyncObject
                            {
                                InterceptId = rdr.GetString(0),
                                InterceptName = rdr.GetString(1),
                            };
                            listObject.Add(tempObj);
                        }
                    }
                }

                if(listObject.Count() > 0)
                {
                    using (MySqlConnection xcpconnection = new MySqlConnection(xcpconnectionString))
                    {
                        xcpconnection.Open();
                        using (MySqlCommand cmd = helper.getListXcipioInfo(xcpconnection, listObject))
                        {
                            using (MySqlDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    var caseid = rdr.GetString(0);
                                    var name = rdr.GetString(1);
                                    var optionvalue = rdr.GetString(2);
                                    var stopdatetime = rdr.GetString(3);
                                    var mod_ts = rdr.GetString(4);
                                    var state = rdr.GetString(5);

                                    var tempItem = listObject.Where(m => m.InterceptName == caseid).FirstOrDefault();
                                    if (tempItem != null)
                                    {
                                        try
                                        {
                                            var tempDate = stopdatetime.Replace('T', ' ').Replace('Z',' '); 
                                            tempItem.STOPDATETIME = tempDate;
                                            tempItem.OPTIONVALUE = optionvalue;
                                            tempItem.NAME = name;
                                            tempItem.MOD_TS = mod_ts;
                                            tempItem.STATE = state;
                                            tempItem.CASEID = caseid;
                                        }
                                        catch { }
                                        
                                    }
                                }
                            }
                        }
                    }

                    foreach (var item in listObject)
                    {
                        string sql = "Update intellego.intercept, target SET intercept.expiration_date = '" + item.STOPDATETIME + "' , intercept.description = '" + item.NAME + "', intercept.interceptType = 'CC', target.servid = '" + item.OPTIONVALUE + "' where intercept.name = '" + item.InterceptName + "' and intercept.id='" + item.InterceptId + "';";
                        var cmd = new MySqlCommand(sql, connection);
                        cmd.ExecuteNonQuery();
                    }
                    //helper.UpdateData(listObject);
                }
            }
            return listObject;
        }


        private void syncUpdatedData(string startTime, string endTime)
        {
            string connectionString = helper.getIntellgoConnectionString();
            string xcpconnectionString = helper.getXCDBConnectionString();
            //var fromDate = DateTime.Now;
            var listObject = new List<NewSyncObject>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = helper.getListUpdatedIntercept(connection, startTime, endTime)) {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var caseid = rdr.GetString(0);
                            var name = rdr.GetString(1);
                            var optionvalue = rdr.GetString(2);
                            var stopdatetime = rdr.GetString(3);
                            var mod_ts = rdr.GetString(4);
                            var state = rdr.GetString(5);
                            var tempDate = stopdatetime.Replace('T', ' ').Replace('Z', ' ');

                            var tempObj = new NewSyncObject
                            {
                                CASEID = caseid,
                                NAME = name,
                               MOD_TS = mod_ts,
                               STATE = state,
                               STOPDATETIME = tempDate,
                               OPTIONVALUE = optionvalue
                            };
                            listObject.Add(tempObj);
                        }
                    }
                }

                var listDiaPhuong = db.ExportTargets.Where(m => m.Active == true).ToList();
                var listActiveObject = listObject.Where(m => m.STATE == "ACTIVE").ToList();
                foreach(var item in listActiveObject)
                {
                    var cmd = helper.updateActiveRecordOnXcipio(connection,item.NAME, item.OPTIONVALUE, item.CASEID);
                    cmd.ExecuteNonQuery();

                    var provinceIndex = item.NAME.IndexOf('_');
                    var provinceName = item.NAME.Substring(0, provinceIndex);



                }
            }
        }
        
    }
}
