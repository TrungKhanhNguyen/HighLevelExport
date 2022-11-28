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
            //var currentTime = new DateTime(DateTime.Now.Year, 9, 17, 02, 00, 00);

            var startTime = (currentTime).AddHours(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endTime = (currentTime).AddHours(-6).ToString("yyyy-MM-ddTHH:mm:ssZ");

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
                    if(interceptNameObject.CaseName == "test_export")
                        tasks.Add(ProcessIntercept(interceptNameObject, startTime, endTime, startTimeWrite));
                }
                await Task.WhenAll(tasks);
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ")+"DONE Exported " + tempListInterceptName.Count() + " intercept from case " + item.TargetName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "FAILED Unhandled error when export casename: " + item.TargetName);
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
                }
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "DONE Exported Intercept name " + interceptNameObject.InterceptName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "FAILED Error when export Intercept name " + interceptNameObject.InterceptName);
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
                    initialData += getLocationJsonString(itemExport) + ",";
                }
                else
                {
                    if (itemExport.call_type == "4")
                        initialData += getSMSJsonString(itemExport) + ",";
                    else
                        initialData += getCallJsonString(itemExport, destinationPath) + ",";
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

        private string getSMSJsonString(ExportObject exportObject)
        {
            var data = "";
            var decodedMsg = utility.getDecodedSMSFromUrl(exportObject.call_product_url);
            var caller = exportObject.call_caller == null ? "" : exportObject.call_caller;
            var callee = exportObject.call_callee == null ? "" : exportObject.call_callee;
            string cgi = "", Address = "", timeStamp = "", dateStamp = "", lat = "", lon = "";

            var tempCalledPartyNumber = exportObject.call_direction == "1" ? callee : caller;
            var tempCallingPartyNumber = exportObject.call_direction == "1" ? caller : callee;
            var tempNormCin = exportObject.call_direction == "1" ? caller : callee;

            var listDic = new string[][] {
                new string[] { "Date:", exportObject.eventDate.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.eventDate.ToString("HH.mm.ss")},
                new string[] { "CalledPartyNumber:", tempCalledPartyNumber } ,
                new string[] { "CallingPartyNumber:", tempCallingPartyNumber } ,
                new string[] { "IMEI:", exportObject.call_participant_imei == null? "" : exportObject.call_participant_imei },
                new string[] { "IMSI:", exportObject.call_participant_imsi == null? "" : exportObject.call_participant_imsi},
                new string[] { "Monitored Target:", exportObject.InterceptName},
                new string[] { "SMS-transfer-status:", ""},
                new string[] { "Decoded SMS:", decodedMsg},
                new string[] { "Decoded Number:", tempNormCin }
            };
            var tempDic = listDic.ToList();

            var tempInterceptName = exportObject.InterceptName;
            if (tempInterceptName.Substring(0, 2) == "84")
            {
                tempInterceptName = exportObject.InterceptName.Remove(0, 2);
                tempInterceptName = tempInterceptName.Insert(0, "0");
            }

            if (exportObject.listCGI.Count() > 0)
            {
                var tempcgiObj = exportObject.listCGI[0];
                cgi = tempcgiObj.value;
                Address = tempcgiObj.address;
                timeStamp = tempcgiObj.timestamp.ToString("HH.mm.ss");
                dateStamp = tempcgiObj.timestamp.ToString("dd/MM/yyyy");
                lat = utility.getLatLonFromString(exportObject.celltower_latlong).lat;
                lon = utility.getLatLonFromString(exportObject.celltower_latlong).lon;
                tempDic.Add(new string[] { "Date:", dateStamp });
                tempDic.Add(new string[] { "Time:", timeStamp });
                tempDic.Add(new string[] { "CGI:", cgi });
                tempDic.Add(new string[] { "Address:", Address });
                tempDic.Add(new string[] { "City:", "" });
                tempDic.Add(new string[] { "Latitude:", lat });
                tempDic.Add(new string[] { "Longitude:", lon });
            }

            var export = new ExportLocation
            {

                orclId = exportObject.elasticId,
                phone = tempInterceptName,
                mito = "LMC",
                targetId = exportObject.InterceptId,
                liuId = exportObject.elasticId,
                type = "SMS",
                sequence = "",
                displayDate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                length = "0",
                network = utility.getNetworkByIMSI(exportObject.call_participant_imsi),
                direction = exportObject.call_direction == "1" ? "IN" : "OUT",
                normCin = tempNormCin,
                lastUpdate = exportObject.eventDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                iriRecords = tempDic.ToArray(),
                hi3File = "",
                fullPath = ""
            };
            string json = JsonConvert.SerializeObject(export);
            data += json;
            return data;

        }

        private string getCallJsonString(ExportObject exportObject, string destinationPath)
        {
            var data = "";
            var caller = exportObject.call_caller == null ? "" : exportObject.call_caller;
            var callee = exportObject.call_callee == null ? "" : exportObject.call_callee;

            var url = exportObject.call_product_url;
            var subUrl = url.Replace("file://var/intellego/", "").Replace("/", @"\");
            var absoluteUrl = StaticKey.MAIN_URL_FOLDER + subUrl.Trim();

            var fileName = utility.getHI3FilenameFromUrl(exportObject.call_product_url);
            var destinationFullUrl = destinationPath + @"\" + fileName;
            if (absoluteUrl.IndexOf(".wav") > -1)
            {
                if (!File.Exists(destinationFullUrl))
                    File.Copy(absoluteUrl, destinationFullUrl, false);
            }

            var tempCalledPartyNumber = exportObject.call_direction == "1" ? callee : caller;
            var tempCallingPartyNumber = exportObject.call_direction == "1" ? caller : callee;
            var tempNormCin = exportObject.call_direction == "1" ? caller : callee;

            var listDic = new string[][] {
                new string[] { "Date:", exportObject.call_startTime.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.call_startTime.ToString("HH.mm.ss")},
                new string[] { "Date:", exportObject.call_endTime.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.call_endTime.ToString("HH.mm.ss")},
                new string[] { "CalledPartyNumber:", tempCalledPartyNumber } ,
                new string[] { "CallingPartyNumber:", tempCallingPartyNumber } ,
                new string[] { "IMEI:", exportObject.call_participant_imei == null? "" : exportObject.call_participant_imei },
                new string[] { "IMSI:", exportObject.call_participant_imsi == null? "" : exportObject.call_participant_imsi},
                new string[] { "Monitored Target:", exportObject.InterceptName}

                };
            var tempDic = listDic.ToList();

            foreach (var item in exportObject.listCGI)
            {
                //var tempcgiObj = exportObject.listCGI[0];
                tempDic.Add(new string[] { "Date:", item.timestamp.ToString("dd/MM/yyyy") });
                tempDic.Add(new string[] { "Time:", item.timestamp.ToString("HH.mm.ss") });
                tempDic.Add(new string[] { "CGI:", item.value });
                tempDic.Add(new string[] { "Address:", item.address });
                tempDic.Add(new string[] { "City:", item.address });
                tempDic.Add(new string[] { "Latitude:", utility.getLatLonFromString(exportObject.celltower_latlong).lat });
                tempDic.Add(new string[] { "Longitude:", utility.getLatLonFromString(exportObject.celltower_latlong).lon });
            }
            var tempInterceptName = exportObject.InterceptName;
            if (tempInterceptName.Substring(0, 2) == "84")
            {
                tempInterceptName = exportObject.InterceptName.Remove(0, 2);
                tempInterceptName = tempInterceptName.Insert(0, "0");
            }
            var export = new ExportLocation
            {
                orclId = exportObject.elasticId,
                phone = tempInterceptName,
                mito = "LMC",
                targetId = exportObject.InterceptId,
                liuId = exportObject.elasticId,
                type = "VOICE",
                sequence = "",
                displayDate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                length = exportObject.call_conversationDuration,
                network = utility.getNetworkByIMSI(exportObject.call_participant_imsi),
                direction = exportObject.call_direction == "1" ? "IN" : "OUT",
                normCin = tempNormCin,
                lastUpdate = exportObject.eventDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                iriRecords = tempDic.ToArray(),
                hi3File = fileName,
                fullPath = ""
            };
            string json = JsonConvert.SerializeObject(export);
            data += json;
            return data;

        }

        private string getLocationJsonString(ExportObject exportObject)
        {
            var data = "";
            //var latlon = "['a','d']";
            var listDic = new string[][] {
                new string[] { "Date:", exportObject.eventDate.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.eventDate.ToString("HH.mm.ss")},
                new string[] { "CGI:", exportObject.celltower_cellid} ,
                new string[] { "Address:", exportObject.celltower_address == null? "" : exportObject.celltower_address },
                new string[] { "City:", ""},
                new string[] { "Latitude:", utility.getLatLonFromString(exportObject.celltower_latlong).lat},
                new string[] { "Longitude:", utility.getLatLonFromString(exportObject.celltower_latlong).lon},
            };
            var tempInterceptName = exportObject.InterceptName;
            if (tempInterceptName.Substring(0, 2) == "84")
            {
                tempInterceptName = exportObject.InterceptName.Remove(0, 2);
                tempInterceptName = tempInterceptName.Insert(0, "0");
            }
            var export = new ExportLocation
            {
                orclId = exportObject.elasticId,
                phone = tempInterceptName,
                mito = "LMC",
                targetId = exportObject.InterceptId,
                liuId = exportObject.elasticId,
                type = "CELL",
                sequence = null,
                displayDate = exportObject.eventDate.ToString("dd/MM/yyyy HH.mm.ss"),
                length = null,
                network = utility.getNetworkByCGI(exportObject.celltower_cellid),
                direction = null,
                normCin = null,
                lastUpdate = exportObject.eventDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                iriRecords = listDic,
                hi3File = "",
                fullPath = ""
            };
            string json = JsonConvert.SerializeObject(export);
            data += json;
            return data;
        }
    }
}
