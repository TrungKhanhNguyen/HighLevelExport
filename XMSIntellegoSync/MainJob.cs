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
            //var currentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 00, 00);
            var currentTime = new DateTime(DateTime.Now.Year, 12, 28, 15, 00, 00);

            var startTime = (currentTime).AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss");
            var endTime = (currentTime).ToString("yyyy-MM-dd HH:mm:ss");

            //syncNewData(startTime, endTime);

            syncUpdatedData(startTime, endTime);
        }
        private void syncNewData(string startTime, string endTime)
        {
            try
            {
                string connectionString = helper.getIntellgoConnectionString();
                string xcpconnectionString = helper.getXCDBConnectionString();
                //var fromDate = DateTime.Now;
                var listObject = new List<NewSyncObject>();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = helper.getInterceptInfo(connection, startTime, endTime))
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

                    if (listObject.Count() > 0)
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
                                                var tempDate = stopdatetime.Replace('T', ' ').Replace('Z', ' ');
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

                        var listDiaPhuong = db.ExportTargets.Where(m => m.Active == true).ToList();

                        foreach (var item in listObject)
                        {
                            string sql = "Update intellego.intercept SET intercept.expiration_date = '" + item.STOPDATETIME + "' , intercept.description = '" + item.NAME + "', intercept.interceptType = 'CC' where intercept.name = '" + item.InterceptName +"';";
                            var cmd = new MySqlCommand(sql, connection);
                            cmd.ExecuteNonQuery();

                            var provinceName = getProvinceNameFromFullName(item.NAME);
                            var tempDiaphuong = listDiaPhuong.Where(m => m.TargetName == provinceName).FirstOrDefault();
                            if (tempDiaphuong != null)
                            {
                                var caseid = tempDiaphuong.TargetId;
                                UpdateProvinceData(connection, item.InterceptId, caseid, provinceName);
                            }
                        }
                        //helper.UpdateData(listObject);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Synced "+listObject.Count()+" NEW DATA " + startTime + " to " + endTime );
                    }
                }
                //return listObject;
            }
            catch (Exception ex) {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when sync NEW DATA "+ startTime + " to " + endTime + " " + ex.Message);
            }
            
        }


        private void syncUpdatedData(string startTime, string endTime)
        {
            try
            {
                string connectionString = helper.getIntellgoConnectionString();
                string xcpconnectionString = helper.getXCDBConnectionString();
                //var fromDate = DateTime.Now;
                var listObject = new List<NewSyncObject>();
                using (MySqlConnection xcpconnection = new MySqlConnection(xcpconnectionString))
                {
                    xcpconnection.Open();
                    using (MySqlCommand cmd = helper.getListUpdatedIntercept(xcpconnection, startTime, endTime))
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
                }
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    //lấy thông tin InterceptId của từng Intercept
                    using (MySqlCommand command = helper.getListInterceptIdByName(connection, listObject))
                    {
                        using (MySqlDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                var tempId = rdr.GetString(0);
                                var tempName = rdr.GetString(1);
                                var tempObj = listObject.Where(m => m.CASEID == tempName).FirstOrDefault();
                                tempObj.InterceptId = tempId;
                            }
                        }
                    }

                    var listDiaPhuong = db.ExportTargets.Where(m => m.Active == true).ToList();
                    var listActiveObject = listObject.Where(m => m.STATE == "ACTIVE").ToList();
                    foreach (var item in listActiveObject)
                    {
                        try
                        {
                            var cmd = helper.updateActiveRecordOnXcipio(connection, item.NAME, item.OPTIONVALUE, item.CASEID);
                            cmd.ExecuteNonQuery();

                            var provinceName = getProvinceNameFromFullName(item.NAME);

                            var tempDiaphuong = listDiaPhuong.Where(m => m.TargetName == provinceName).FirstOrDefault();
                            if (tempDiaphuong != null)
                            {
                                var interceptid = item.InterceptId;
                                var caseid = tempDiaphuong.TargetId;
                                UpdateProvinceData(connection, interceptid, caseid, provinceName);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when sync UPDATE DATA " + startTime + " to " + endTime + " , Active intercept: " + item.CASEID + ex.Message);
                        }

                    }

                    var listInactiveObject = listObject.Where(m => m.STATE == "INACTIVE").ToList();
                    foreach (var item in listInactiveObject)
                    {
                        try
                        {
                            var provinceName = getProvinceNameFromFullName(item.NAME);
                            var tempDiaphuong = listDiaPhuong.Where(m => m.TargetName == provinceName).FirstOrDefault();
                            if (tempDiaphuong != null)
                            {
                                var cmd = helper.updateInterceptTrashTime(connection, item.InterceptId);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                var cmd = helper.updateInterceptSuspendedTime(connection, item.InterceptId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when sync UPDATE DATA " + startTime + " to " + endTime + " , Inactive intercept: " + item.CASEID + ex.Message);
                        }

                    }
                }
                
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when sync UPDATE DATA " + startTime + " to " + endTime + " " + ex.Message);
            }
            
        }

        private void UpdateProvinceData(MySqlConnection connection, string interceptid, string caseid, string provinceName)
        {
            var cmdUpdate = helper.updateInterceptAndCaseIntercept(connection, interceptid, caseid, provinceName);
            cmdUpdate.ExecuteNonQuery();
        }

        private string getProvinceNameFromFullName(string name)
        {
            var provinceIndex = name.IndexOf('_');
            var provinceName = name.Substring(0, provinceIndex);
            return provinceName;
        }
        
    }
}
