using ConnectionHelper.Helper;
using ConnectionHelper.Models;
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
        private SQLServerHelper sqlserverHelper = new SQLServerHelper();
        public void Execute(IJobExecutionContext context)
        {
            //throw new NotImplementedException();
            exportSingleData();
            //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public async void exportSingleData()
        {
            listTarget = sqlserverHelper.GetListExportTarget();

            var currentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 00, 00);
            //var currentTime = new DateTime(DateTime.Now.Year, 11, 28, 11, 00, 00);

            var startTime = (currentTime).AddHours(-8).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endTime = (currentTime).AddHours(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");

            //var startTime = (currentTime).AddHours(0).ToString("yyyy-MM-ddTHH:mm:ssZ");
            //var endTime = (currentTime).AddHours(48).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var startTimeWrite = currentTime.ToString("yyyy-MM-dd HH-mm");
            foreach (var item in listTarget)
            {
                //ExecuteCaseName(item, startTime, endTime, startTimeWrite);
                await ExecuteCaseNameAsync(item, startTime, endTime, startTimeWrite);
            }
            //await Task.WhenAll(tasks);
        }

        private async Task ExecuteCaseNameAsync(ExportTarget item, string startTime, string endTime, string startTimeWrite)
        {
            try
            {
                var tempListInterceptName = mainHelper.GetListInterceptName(item);
                List<Task> tasks = new List<Task>();
                foreach (var interceptNameObject in tempListInterceptName)
                {
                    tasks.Add(ProcessIntercept(interceptNameObject, startTime, endTime, startTimeWrite));
                }
                await Task.WhenAll(tasks);
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ")+"[DONE] Exported " + tempListInterceptName.Count() + " intercept from case " + item.TargetName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Unhandled error when export casename: " + item.TargetName);
            }
        }

        private Task ProcessIntercept(ExportObject interceptNameObject, string startTime, string endTime, string startTimeWrite)
        {
            return Task.Run(() =>
            {
                ExecuteInterceptName(interceptNameObject, startTime, endTime, startTimeWrite);
            });
        }
        private void ExecuteInterceptName(ExportObject interceptNameObject, string startTime, string endTime, string startTimeWrite)
        {
            try
            {
                var finalExportList = mainHelper.ExecuteInterceptName(interceptNameObject, startTime, endTime, startTimeWrite);
                if (finalExportList.Count() > 0)
                {
                    WriteFile(finalExportList, startTimeWrite, interceptNameObject.CaseName, interceptNameObject.InterceptName);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Exported Intercept name " + interceptNameObject.InterceptName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when export Intercept name " + interceptNameObject.InterceptName);
                sqlserverHelper.InsertLogToDB("Error when export Intercept name " + interceptNameObject.InterceptName, DateTime.Now, interceptNameObject.CaseName,ErrorType.HourError.ToString(), interceptNameObject.InterceptId, interceptNameObject.InterceptName);
            }
        }

        private void WriteFile(List<ExportObject> listExport, string startTime, string casename, string interceptname)
        {
            var convertedInterceptName = "";
            if (interceptname.Substring(0, 2) == "84")
            {
                convertedInterceptName = interceptname.Remove(0, 2);
                convertedInterceptName = convertedInterceptName.Insert(0, "0");
            }
            string initialData = "[";
            var destinationPath = StaticKey.EXPORT_FOLDER + @"\" + "AP_" + casename + "_All_" + startTime + @"\AP_" + casename + "_" + convertedInterceptName;
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
                        initialData += utility.getCallJsonString(itemExport, destinationPath) + ",";
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
