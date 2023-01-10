using MySql.Data.MySqlClient;
using Quartz.Impl;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace XMSIntellegoSync
{
    internal class Program
    {
        private static Helper helper = new Helper();
        private static ExportHistoryEntities db = new ExportHistoryEntities();
        static void Main(string[] args)
        {

            //Console.WriteLine("\r\n");
            //Console.WriteLine("=======================================================================================================");
            //Console.WriteLine("**********************************Start sync XMS-INTELLEGO every 1 hour********************************");
            //Console.WriteLine("=======================================================================================================");
            //Console.WriteLine("\r\n");
            ////Console.WriteLine("Start simple job");

            //Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info }; Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info };

            //IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            //scheduler.Start();
            //IJobDetail job = JobBuilder.Create<MainJob>().Build();
            //ITrigger trigger = TriggerBuilder.Create()
            // .StartAt(DateTime.Now)
            //   //.WithCronSchedule("1 5 0/1 * * ?")
            //   .WithCronSchedule("10 0/15 * * * ?")
            //   .WithPriority(1)
            //   .Build();
            //scheduler.ScheduleJob(job, trigger);

            //Console.ReadLine();




            Console.WriteLine("\r\n");
            Console.WriteLine("=======================================================================================================");
            Console.WriteLine("***************************************Sync XMS-INTELLEGO manual***************************************");
            Console.WriteLine("=======================================================================================================");
            Console.WriteLine("\r\n");
            //Console.WriteLine("Start simple job");
            var currentVal = true;
            while (currentVal)
            {
                Console.WriteLine("Enter start date (format yyyy-MM-dd HH:mm:ss)");
                // Create a string variable and get user input from the keyboard and store it in the variable
                string startDate = Console.ReadLine();
                Console.WriteLine("\r\n");
                // Print the value of the variable (userName), which will display the input value
                Console.WriteLine("Enter end date (format yyyy-MM-dd HH:mm:ss)");
                string endDate = Console.ReadLine();
                Console.WriteLine("\r\n");
                syncNewData(startDate, endDate);
                syncUpdatedData(startDate, endDate);
                Console.WriteLine("\r\n");
                Console.WriteLine("DO YOU WANT TO MANUAL SYNC AGAIN? (y/n)");
                string syncagain = Console.ReadLine();
                if (syncagain == "n")
                    currentVal = false;
            }
            Console.WriteLine("Press Enter to Quit");
            Console.ReadLine();
        }


        private static void syncNewData(string tempstartTime, string tempendTime)
        {
            var temp1 = DateTime.ParseExact(tempstartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).AddHours(-7);
            var temp2 = DateTime.ParseExact(tempendTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).AddHours(-7);
            var startTime = temp1.ToString("yyyy-MM-dd HH:mm:ss");
            var endTime = temp2.ToString("yyyy-MM-dd HH:mm:ss");
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
                        //helper.UpdateData(listObject);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Synced " + listObject.Count() + " NEW DATA " + startTime + " to " + endTime);
                    }
                }
                //return listObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when sync NEW DATA " + startTime + " to " + endTime + " " + ex.Message);
            }

        }


        private static void syncUpdatedData(string startTime, string endTime)
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
                        //if(item.CASEID == "84819403526")
                        //{
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
                                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Synced ACTIVE data " + item.CASEID);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when sync UPDATE data " + startTime + " to " + endTime + " , Active intercept: " + item.CASEID + " " + ex.Message);
                        }



                    }

                    var listInactiveObject = listObject.Where(m => m.STATE == "INACTIVE").ToList();
                    foreach (var item in listInactiveObject)
                    {
                        try
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
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Synced INACTIVE data " + item.CASEID);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when sync UPDATE data " + startTime + " to " + endTime + " , Inactive intercept: " + item.CASEID + " " + ex.Message);
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when sync UPDATE DATA " + startTime + " to " + endTime + " " + ex.Message);
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


    }
}
