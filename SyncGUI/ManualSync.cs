using K4os.Compression.LZ4.Internal;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncGUI
{
    public partial class ManualSync : Form
    {
        private static Helper helper = new Helper();
        private static ExportHistoryEntities db = new ExportHistoryEntities();
        public ManualSync()
        {
            InitializeComponent();
        }

        private void ManualSync_Load(object sender, EventArgs e)
        {
            dpFromDate.Value = dpToDate.Value = dpToTime.Value = DateTime.Now;
            dpFromTime.Value = DateTime.Now.AddHours(-1);
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            var fromdate = dpFromDate.Value;
            var fromtime = dpFromTime.Value;
            var todate = dpToDate.Value;
            var totime = dpToTime.Value;
            var tempStartTime = new DateTime(fromdate.Year, fromdate.Month, fromdate.Day, fromtime.Hour, fromtime.Minute, 0);
            var tempEndTime = new DateTime(todate.Year, todate.Month, todate.Day, totime.Hour, totime.Minute, 0);

            var tempStartTime7 = tempStartTime.AddHours(-7);
            var tempEndTime7 = tempEndTime.AddHours(-7);

            syncNewData(tempStartTime7.ToString("yyyy-MM-dd HH:mm:ss"), tempEndTime7.ToString("yyyy-MM-dd HH:mm:ss"));
            syncUpdatedData(tempStartTime.ToString("yyyy-MM-dd HH:mm:ss"), tempEndTime.ToString("yyyy-MM-dd HH:mm:ss"));

        }

        private void syncNewData(string startTime, string endTime)
        {
            try
            {
                string connectionString = helper.getIntellgoConnectionString();
                string xcpconnectionString = helper.getXCDBConnectionString();

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
                                        //var name = rdr.GetString(1);
                                        var optionvalue = rdr.GetString(1);
                                        var stopdatetime = rdr.GetString(2);
                                        var mod_ts = rdr.GetString(3);
                                        var state = rdr.GetString(4);

                                        var tempItem = listObject.Where(m => m.InterceptName == caseid).FirstOrDefault();
                                        if (tempItem != null)
                                        {
                                            try
                                            {
                                                var tempDate = stopdatetime.Replace('T', ' ').Replace('Z', ' ');
                                                tempItem.STOPDATETIME = tempDate;
                                                tempItem.OPTIONVALUE = optionvalue;
                                                //tempItem.NAME = name;
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
                            string sql = "Update intellego.intercept SET intercept.expiration_date = '" + item.STOPDATETIME + "' , intercept.description = '" + item.OPTIONVALUE + "', intercept.interceptType = 'CC' where intercept.name = '" + item.InterceptName + "';";
                            var cmd = new MySqlCommand(sql, connection);
                            cmd.ExecuteNonQuery();

                            var provinceName = getProvinceNameFromFullName(item.OPTIONVALUE);
                            var tempDiaphuong = listDiaPhuong.Where(m => m.TargetName == provinceName).FirstOrDefault();
                            if (tempDiaphuong != null)
                            {
                                var caseid = tempDiaphuong.TargetId;
                                UpdateProvinceData(connection, item.InterceptId, caseid, provinceName);
                            }
                        }
                        var log = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + "[DONE] Synced " + listObject.Count() + " NEW DATA " + startTime + " to " + endTime;
                        AddToLog(log);
                    }
                }
                //return listObject;
            }
            catch (Exception ex)
            {
                string log = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + "[FAILED] Error when sync NEW DATA " + startTime + " to " + endTime + " " + ex.Message;
                AddToLog(log);
            }

        }

        private void AddToLog(string log)
        {
            txtLog.Invoke(new Action(() =>
            {
                txtLog.Text += Environment.NewLine + log;
            }));
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
                                //var name = rdr.GetString(1);
                                var optionvalue = rdr.GetString(1);
                                var stopdatetime = rdr.GetString(2);
                                var mod_ts = rdr.GetString(3);
                                var state = rdr.GetString(4);
                                var tempDate = stopdatetime.Replace('T', ' ').Replace('Z', ' ');
                                var tempObj = new NewSyncObject
                                {
                                    CASEID = caseid,
                                    //NAME = name,
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
                            var cmd = helper.updateActiveRecordOnXcipio(connection, item.STOPDATETIME, item.OPTIONVALUE, item.CASEID);
                            cmd.ExecuteNonQuery();

                            var provinceName = getProvinceNameFromFullName(item.OPTIONVALUE);

                            var tempDiaphuong = listDiaPhuong.Where(m => m.TargetName == provinceName).FirstOrDefault();
                            if (tempDiaphuong != null)
                            {
                                var interceptid = item.InterceptId;
                                var caseid = tempDiaphuong.TargetId;
                                UpdateProvinceData(connection, interceptid, caseid, provinceName);
                                //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Synced ACTIVE data " + item.CASEID);

                                var log = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + "[DONE] Synced ACTIVE data " + item.CASEID;
                                AddToLog(log);
                            }
                        }
                        catch (Exception ex)
                        {
                            var log = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + "[FAILED] Error when sync UPDATE data " + startTime + " to " + endTime + " , Active intercept: " + item.CASEID + " " + ex.Message;
                            AddToLog(log);
                        }
                    }

                    var listInactiveObject = listObject.Where(m => m.STATE == "INACTIVE").ToList();
                    foreach (var item in listInactiveObject)
                    {
                        try
                        {
                            if (!String.IsNullOrEmpty(item.OPTIONVALUE))
                            {
                                var provinceName = getProvinceNameFromFullName(item.OPTIONVALUE);
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
                            else
                            {
                                var cmd = helper.updateInterceptSuspendedTime(connection, item.InterceptId);
                                cmd.ExecuteNonQuery();
                            }
                            AddToLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + "[DONE] Synced INACTIVE data " + item.CASEID);
                        }
                        catch (Exception ex)
                        {
                            AddToLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + "[FAILED] Error when sync UPDATE data " + startTime + " to " + endTime + " , Inactive intercept: " + item.CASEID + " " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddToLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + "[FAILED] Error when sync UPDATE DATA " + startTime + " to " + endTime + " " + ex.Message);
            }

        }

        private static void UpdateProvinceData(MySqlConnection connection, string interceptid, string caseid, string provinceName)
        {
            var cmdUpdate = helper.updateInterceptAndCaseIntercept(connection, interceptid, caseid, provinceName);
            cmdUpdate.ExecuteNonQuery();
        }

        private static string getProvinceNameFromFullName(string name)
        {
            var provinceIndex = name.IndexOf('_');
            var provinceName = name.Substring(0, provinceIndex);
            return provinceName;
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }
}
