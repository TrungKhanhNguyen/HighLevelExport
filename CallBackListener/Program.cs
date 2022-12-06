using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallBackListener
{
    internal class Program
    {
        //private DBHelper helper = new DBHelper();
        //private MainHelper mainHelper = new MainHelper();
        //private SQLServerHelper sqlserverHelper = new SQLServerHelper();
        //private Utility utility = new Utility();
        static void Main(string[] args)
        {
            IHubProxy hub;

            var url = StaticKey.SIGNALR_IP;
            var Connection = new HubConnection(url, useDefaultUrl: false);
            hub = Connection.CreateHubProxy("ServiceStatusHub");
            Connection.Start().Wait();

            Console.WriteLine("Waiting for new export command...");

            hub.On<string>("acknowledgeMessage", (message) =>
            {
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm: ") +  "[INFO] Received callback export - " + message);
                ExportData(message);
                Console.WriteLine("Waiting for new export command...");
            });
            //Console.WriteLine("Waiting for new export command...");
            Console.ReadKey();
        }

        private static void ExportData(string message)
        {
            try
            {
                var arrayData = message.Split(';');
                if (arrayData.Count() > 0)
                {
                    var casename = arrayData[0];
                    var interceptid = arrayData[1];
                    var interceptname = arrayData[2];
                    var beginvalue = arrayData[3];
                    var endvalue = arrayData[4];

                    var beginDate = DateTime.ParseExact(beginvalue, "dd-MM-yyyy HH:mm",
                                           System.Globalization.CultureInfo.InvariantCulture);

                    var endDate = DateTime.ParseExact(endvalue, "dd-MM-yyyy HH:mm",
                                           System.Globalization.CultureInfo.InvariantCulture);

                    var tempBegin = beginDate.AddHours(-7).ToString("yyyy-MM-ddTHH:mm:00Z");
                    var tempEnd = endDate.AddHours(-7).ToString("yyyy-MM-ddTHH:mm:00Z");

                    var startTimeWrite = beginDate.ToString("yyyy-MM-dd HH-mm");
                    var tempTarget = new ExportObject { InterceptId = interceptid, InterceptName = interceptname, CaseName = casename };
                    ExecuteInterceptName(tempTarget, tempBegin, tempEnd, startTimeWrite);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm: ") + "[ERROR] - " + ex.Message);
            }
            
        }
        private static void ExecuteInterceptName(ExportObject interceptNameObject, string startTime, string endTime, string startTimeWrite)
        {
            try
            {
                MainHelper mainHelper = new MainHelper();
                var finalExportList = mainHelper.ExecuteInterceptName(interceptNameObject, startTime, endTime, startTimeWrite);
                if (finalExportList.Count() > 0)
                {
                    WriteCallBackFile(finalExportList, startTimeWrite, interceptNameObject.CaseName, interceptNameObject.InterceptName);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Exported Intercept name " + interceptNameObject.InterceptName);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when export Intercept name " + interceptNameObject.InterceptName + " " + ex.Message);
                SQLServerHelper sqlserverHelper = new SQLServerHelper();
                sqlserverHelper.InsertLogToDB("Error " + ex.Message, DateTime.Now, interceptNameObject.CaseName, ErrorType.CallbackError.ToString(), interceptNameObject.InterceptId, interceptNameObject.InterceptName);
            }
        }

        private static void WriteCallBackFile(List<ExportObject> listExport, string startTime, string casename, string interceptname)
        {
            Utility utility = new Utility();
            var convertedInterceptName = "";
            if (interceptname.Substring(0, 2) == "84")
            {
                convertedInterceptName = interceptname.Remove(0, 2);
                convertedInterceptName = convertedInterceptName.Insert(0, "0");
            }
            string initialData = "[";
            var destinationPath = StaticKey.EXPORT_MANUAL_FOLDER + @"\AP_" + casename + "_Callback_" + convertedInterceptName + "_" + startTime;
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
