using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallReExportListener
{
    public class MainJob : IJob
    {
        private SQLServerHelper sqlServerhelper = new SQLServerHelper();
        private DBHelper helper = new DBHelper();
        private MainHelper mainHelper = new MainHelper();
        private Utility utility = new Utility();
        public void Execute(IJobExecutionContext context)
        {
            //throw new NotImplementedException();
            ExecuteData();
        }

        public void ExecuteData()
        {
            string connectionString = helper.getConnectionString();
            var listItem = new List<CallReExport>();

            string[] lines = File.ReadAllLines("configs.txt");
            var reexportfolder = lines[1];
            try
            {
                foreach (string file in Directory.EnumerateFiles(reexportfolder, "*.txt"))
                {
                    var contents = File.ReadAllLines(file);
                    var casename = contents[0];
                    var elasticid = contents[1];
                    var eventDate = contents[2];
                    var interceptid = contents[3];
                    var interceptname = contents[4];
                    var type = contents[5];
                    var writeTime = contents[6];
                    var tempObj = new CallReExport
                    {
                        Casename = casename,
                        ElasticId = elasticid,
                        EventDate = Convert.ToDateTime(eventDate),
                        InterceptId = interceptid,InterceptName = interceptname,Type = type, WriteTime = writeTime
                    };
                    GetData(tempObj);

                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] ERROR " + ex.Message);
            }
        }


        private Task ProcessItem(CallReExport item)
        {
            return Task.Run(() =>
            {
                GetData(item);
            });
        }
        private void GetData(CallReExport item)
        {
            try
            {
                var listExport = mainHelper.ExecuteReExportObject(item);
                if (listExport.Count() > 0)
                {
                    WriteFile(listExport, item.Type, item.WriteTime);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[DONE] Re-Exported intercept " + item.InterceptName + " from case " + item.Casename);
                    //sqlServerhelper.DeleteCallReExport(item.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when re-export Intercept name " + item.InterceptName + " " + ex.Message);
            }
        }


        private void WriteFile(List<ExportObject> listExport,string type, string writeTime)
        {
            var convertedInterceptName = "";
            var item = listExport[0];
            if (item.InterceptName.Substring(0, 2) == "84")
            {
                convertedInterceptName = item.InterceptName.Remove(0, 2);
                convertedInterceptName = convertedInterceptName.Insert(0, "0");
            }
            string initialData = "[";
            var destinationPath = "";
            var hi2FullPath = "";

            string[] lines = File.ReadAllLines("configs.txt");
            string exportPath = lines[0];

            if (type == ReExportType.Hour.ToString())
            {
                destinationPath = exportPath + @"\" + "AP_" + item.CaseName + "_All_" + writeTime + @"\AP_" + item.CaseName + "_" + convertedInterceptName;
                hi2FullPath = destinationPath + @"\HI2_" + item.CaseName + "_" + item.InterceptName + ".json";
            }
            else
            {
                destinationPath = exportPath + @"\AP_" + item.CaseName + "_2MINS_" + convertedInterceptName + "_" + writeTime;
                hi2FullPath = destinationPath + @"\HI2_" + item.CaseName + "_" + item.InterceptName + ".json";
            }

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
                        initialData += utility.getCallJsonString(itemExport, destinationPath) + ",";
                    }
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
