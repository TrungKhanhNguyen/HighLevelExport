using ConnectionHelper.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionHelper.Helper
{
    public class Utility
    {
        private SQLServerHelper sqlHelper = new SQLServerHelper();
        public LatLonObject getLatLonFromString(string latlon)
        {
            var lat = "";
            var lon = "";
            if (!String.IsNullOrEmpty(latlon))
            {
                var indexOfComma = latlon.IndexOf(',');
                var indexOfOpenSquare = latlon.IndexOf('[');
                var indexOfCloseSquare = latlon.IndexOf('[');
                lat = latlon.Substring(indexOfOpenSquare + 1, indexOfComma - indexOfOpenSquare - 1);
                lon = latlon.Substring(indexOfComma + 1, indexOfCloseSquare - indexOfComma - 1);
            }
            var latlonObj = new LatLonObject
            {
                lat = lat,
                lon = lon
            };
            return latlonObj;
        }

        public string getNetworkByIMSI(string imsi)
        {
            var network = "";
            try
            {
                if (!String.IsNullOrEmpty(imsi))
                {
                    var tempNetwork = imsi.Substring(0, 5);
                    switch (tempNetwork)
                    {
                        case "45201": network = "Mobifone"; break;
                        case "45202": network = "Vinaphone"; break;
                        case "45204": network = "Viettel"; break;
                        case "45208": network = "Vinaphone"; break;
                    }
                }
                return network;
            }
            catch { return ""; }

        }



        public string getNetworkByCGI(string cgi)
        {
            var network = "";
            try
            {
                if (!String.IsNullOrEmpty(cgi))
                {
                    var tempNetwork = cgi.Substring(4, 2);
                    switch (tempNetwork)
                    {
                        case "01": network = "Mobifone"; break;
                        case "02": network = "Vinaphone"; break;
                        case "04": network = "Viettel"; break;
                        case "08": network = "Vinaphone"; break;
                    }
                }
                return network;
            }
            catch { return ""; }

        }

        public double getUnixTimeByDate(DateTime date)
        {
            var timeSpan = (date - new DateTime(1970, 1, 1, 0, 0, 0));
            return (timeSpan.TotalSeconds / 1);
        }

        public string getDecodedSMSFromUrl(string url)
        {
            try
            {
                string sms = "";
                var subUrl = url.Replace("file://var/intellego/", "").Replace("/", @"\");
                var absoluteUrl = StaticKey.MAIN_URL_FOLDER + subUrl.Trim();
                string[] lines = System.IO.File.ReadAllLines(absoluteUrl);
                if (lines.Length >= 2)
                {
                    sms = lines[1];
                }
                return sms;
            }
            catch { return ""; }
            
        }
        public string getHI3FilenameFromUrl(string url)
        {
            string filePath = "";
            var subUrl = url.Replace("/", @"\");
            var indexOfLastClose = subUrl.LastIndexOf(@"\");
            filePath = subUrl.Substring(indexOfLastClose + 1);
            return filePath;
        }


        public string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));
            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

        public string getSMSJsonString(ExportObject exportObject)
        {
            var data = "";
            var decodedMsg = getDecodedSMSFromUrl(exportObject.call_product_url);
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
                new string[] { "Decoded SMS: ", decodedMsg},
                new string[] { "Decoded Number: ", tempNormCin }
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
                lat = getLatLonFromString(exportObject.celltower_latlong).lat;
                lon = getLatLonFromString(exportObject.celltower_latlong).lon;
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
                network = getNetworkByIMSI(exportObject.call_participant_imsi),
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

        public string getCallJsonString(ExportObject exportObject, string destinationPath)
        {
            var data = "";
            var caller = exportObject.call_caller == null ? "" : exportObject.call_caller;
            var callee = exportObject.call_callee == null ? "" : exportObject.call_callee;

            var url = exportObject.call_product_url;
            var subUrl = url.Replace("file://var/intellego/", "").Replace("/", @"\");
            var absoluteUrl = StaticKey.MAIN_URL_FOLDER + subUrl.Trim();

            var fileName = getHI3FilenameFromUrl(exportObject.call_product_url);
            var destinationFullUrl = destinationPath + @"\" + fileName;
            if (absoluteUrl.IndexOf(".wav") > -1)
            {
                if (!File.Exists(destinationFullUrl))
                {
                    File.Copy(absoluteUrl, destinationFullUrl, false);
                }
                    
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
                tempDic.Add(new string[] { "Latitude:", getLatLonFromString(exportObject.celltower_latlong).lat });
                tempDic.Add(new string[] { "Longitude:", getLatLonFromString(exportObject.celltower_latlong).lon });
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
                length = exportObject.call_conversationDuration + "000",
                network = getNetworkByIMSI(exportObject.call_participant_imsi),
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

        public string getLocationJsonString(ExportObject exportObject)
        {
            var data = "";
            //var latlon = "['a','d']";
            var listDic = new string[][] {
                new string[] { "Date:", exportObject.eventDate.ToString("dd/MM/yyyy")},
                new string[] { "Time:", exportObject.eventDate.ToString("HH.mm.ss")},
                new string[] { "CGI:", exportObject.celltower_cellid} ,
                new string[] { "Address:", exportObject.celltower_address == null? "" : exportObject.celltower_address },
                new string[] { "City:", ""},
                new string[] { "Latitude:", getLatLonFromString(exportObject.celltower_latlong).lat},
                new string[] { "Longitude:", getLatLonFromString(exportObject.celltower_latlong).lon},
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
                network = getNetworkByCGI(exportObject.celltower_cellid),
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
