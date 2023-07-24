using ConnectionHelper.Helper;
using ConnectionHelper.Models;
//using ConnectionHelper.Models;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoursExportListener
{
    public class MainJob : IJob
    {
        private List<ExportTarget> listTarget = new List<ExportTarget>();
        private DBHelper helper = new DBHelper();
        private MainHelper mainHelper = new MainHelper();
        private Utility utility = new Utility();
        //private SQLServerHelper sqlserverHelper = new SQLServerHelper();
        private ExportHistoryEntities exportHistory = new ExportHistoryEntities();
        private List<Log> listLog = new List<Log>();
        public void Execute(IJobExecutionContext context)
        {
            exportSingleData();
        }

        public void exportSingleData()
        {
            listLog = new List<Log>();
            

            var currentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 10, 00, 00);

            var startTime = (currentTime).AddHours(-8).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endTime = (currentTime).AddHours(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var startTimeWrite = currentTime.ToString("yyyy-MM-dd HH-mm");
            //List<Task> taskCase = new List<Task>();
            string[] lines = File.ReadAllLines("configs.txt");
            if (lines.Count() > 1)
            {
                for(int i=1; i<lines.Count(); i++)
                {
                    var tempItem = new ExportTarget { Active = true, TargetName = lines[i], Id = i };
                    listTarget.Add(tempItem);
                }

                foreach (var item in listTarget)
                {
                    //taskCase.Add(ExecuteCaseNameAsync(item, startTime, endTime, startTimeWrite));
                    // if(item.TargetName == "DIENBIEN")
                    //{
                    ExecuteCaseNameAsync(item, startTime, endTime, startTimeWrite);
                    //}

                }
                //await Task.WhenAll(taskCase);

                try
                {
                    var fileDirectory = @"C:\Logs\1Hour\";
                    string path = fileDirectory + DateTime.Now.ToString("yyyyMMddHH");
                    Directory.CreateDirectory(path);
                    var fullPath = path + @"\1hour.txt";
                    var listItem = listLog.OrderBy(m => m.dateLog).ToList();
                    using (StreamWriter sw = (File.Exists(fullPath)) ? File.AppendText(fullPath) : File.CreateText(fullPath))
                    {
                        foreach (var line in listItem)
                        {
                            sw.WriteLine(line.log);
                        }
                    }
                }
                catch
                {
                    AddLog(Environment.NewLine + "Cannot write log file");
                }
            }
            else
            {
                AddLog(Environment.NewLine + "Error!!! Check config file");
            }

            
            
            //Console.ReadLine();
        }

        private void ExecuteCaseNameAsync(ExportTarget item, string startTime, string endTime, string startTimeWrite)
        {
            try
            {
                var tempListInterceptName = mainHelper.GetListInterceptName(item.TargetName);
                List<Task> tasks = new List<Task>();
                foreach (var interceptNameObject in tempListInterceptName)
                {
                    //if(interceptNameObject.InterceptName == "84778542863")
                    //{
                    //tasks.Add(ProcessIntercept(interceptNameObject, startTime, endTime, startTimeWrite));
                    ExecuteInterceptName(interceptNameObject, startTime, endTime, startTimeWrite);
                }
                //await Task.WhenAll(tasks);
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Exported " + tempListInterceptName.Count() + " intercept from case " + item.TargetName);
            }
            catch (Exception ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Unhandled error when export casename: " + item.TargetName + ", error: " +ex.Message);
            }
        }

        private Task ProcessIntercept(ExportObject interceptNameObject, string startTime, string endTime, string startTimeWrite)
        {
            return Task.Run(() =>
            {
                ExecuteInterceptName(interceptNameObject, startTime, endTime, startTimeWrite);
            });
        }

        private void AddLog(string log)
        {
            Console.WriteLine(log);
            listLog.Add(new Log { dateLog = DateTime.Now, log = log });
        }

        private void ExecuteInterceptName(ExportObject interceptNameObject, string startTime, string endTime, string startTimeWrite)
        {
            try
            {
                var finalExportList = mainHelper.ExecuteInterceptName(interceptNameObject, startTime, endTime, startTimeWrite,ReExportType.Hour.ToString());
                if (finalExportList.Count() > 0)
                {
                    WriteFile(finalExportList, startTimeWrite, interceptNameObject.CaseName, interceptNameObject.InterceptName);
                    AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Exported Intercept name " + interceptNameObject.InterceptName);
                }
            }
            catch (Exception ex)
            {
                AddLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when export Intercept name " + interceptNameObject.InterceptName + " " + ex.Message);
                //sqlserverHelper.InsertLogToDB("Error " + ex.Message, DateTime.Now, interceptNameObject.CaseName,ErrorType.HourError.ToString(), interceptNameObject.InterceptId, interceptNameObject.InterceptName);
            }
        }

        private void InsertHI3ToRetrieve(string source, string destination)
        {
            var hi3 = new HI3Retrieve { DestinationPath = destination, SourcePath = source };
            exportHistory.HI3Retrieve.Add(hi3);
            exportHistory.SaveChanges();
        }

        private void WriteFile(List<ExportObject> listExport, string startTime, string casename, string interceptname)
        {
            string[] lines = File.ReadAllLines("configs.txt");
            string exportPath = lines[0];

            var convertedInterceptName = "";
            if (interceptname.Substring(0, 2) == "84")
            {
                convertedInterceptName = interceptname.Remove(0, 2);
                convertedInterceptName = convertedInterceptName.Insert(0, "0");
            }
            else
            {
                convertedInterceptName = interceptname;
            }
            string initialData = "[";
            var destinationPath = exportPath + @"\" + "AP_" + casename + "_All_" + startTime + @"\AP_" + casename + "_" + convertedInterceptName + "_" + startTime;
            var hi2FullPath = destinationPath + @"\HI2_" + casename + "_" + interceptname + ".json";
            Directory.CreateDirectory(destinationPath);
            foreach (var itemExport in listExport)
            {
                if (itemExport.document_type == "19")
                {
                    initialData += utility.getLocationJsonString(itemExport) + ",";
                }
                else
                {
                    if (itemExport.call_type == "4")
                        initialData += utility.getSMSJsonString(itemExport) + ",";
                    else
                    {
                        try
                        {
                            initialData += utility.getCallJsonString(itemExport, destinationPath) + ",";
                        }
                        catch (Exception ex)
                        {
                            var url = itemExport.call_product_url;
                            var subUrl = url.Replace("file://var/intellego/", "").Replace("/", @"\");
                            var absoluteUrl = StaticKey.MAIN_URL_FOLDER + subUrl.Trim();

                            var fileName = utility.getHI3FilenameFromUrl(itemExport.call_product_url);
                            var destinationFullUrl = destinationPath + @"\" + fileName;

                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when copy file, " + ex.Message);
                            //sqlserverHelper.InsertHI3ToRetrieve(absoluteUrl, destinationFullUrl);

                        }
                    }
                        //initialData += utility.getCallJsonString(itemExport, destinationPath) + ",";
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

        
    }
}
