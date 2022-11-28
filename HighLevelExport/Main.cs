using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HighLevelExport
{

    //public class SimpleJob : IJob
    //{
    //    public Task Execute(IJobExecutionContext context)
    //    {
    //        //txtMultiLog.Text += Environment.NewLine + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    //        throw new NotImplementedException();
    //    }
    //}

    public partial class Main : Form,IJob
    {
        //private ExportHistoryEntities db = new ExportHistoryEntities();
        private List<ExportTarget> listTarget = new List<ExportTarget>();
        private DBHelper helper = new DBHelper();
        private MainHelper mainHelper = new MainHelper();
        private Utility utility = new Utility();
        private string connectionString = "";
        private System.Windows.Forms.Timer mainTimer = new System.Windows.Forms.Timer();
        public Main()
        {
            InitializeComponent();
        }
        //public Task Execute(IJobExecutionContext context)
        //{
        //    Add();
        //    throw new NotImplementedException();

        //}
        public void Execute(IJobExecutionContext context)
        {
            //throw new NotImplementedException();
            //txtMultiLog.Text += Environment.NewLine + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            //MessageBox.Show("", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            txtMultiLog.Invoke((MethodInvoker)delegate
            {
                // Running on the UI thread
                txtMultiLog.Text += Environment.NewLine + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            });
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            exportSingleData();


            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail job = JobBuilder.Create<Main>().Build();
            ITrigger trigger = TriggerBuilder.Create()
             .StartAt(DateTime.Now)
               .WithCronSchedule("0 0/2 * * * ?")
               .WithPriority(1)
               .Build();
            scheduler.ScheduleJob(job, trigger);

        }

        public void Add()
        {
            txtMultiLog.Text += Environment.NewLine +  " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        public async void exportSingleData()
        {
            //listTarget = db.ExportTargets.ToList();
            var selectedText = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Value;
            var selectedKey = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Key;
            var tempTarget = new ExportTarget { TargetId = selectedKey, TargetName = selectedText };
            listTarget.Add(tempTarget);
            //var currentTime = new DateTime(2022, 09, 15, 07, 00, 00);
            var startTime = (startTimeDate.Value.Date + startTimeTime.Value.TimeOfDay).AddHours(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endTime = (endTimeDate.Value.Date + endTimeTime.Value.TimeOfDay).AddHours(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var logExportTime = startTime + " - " + endTime;
            var startTimeWrite = new DateTime(startTimeDate.Value.Year, startTimeDate.Value.Month, startTimeDate.Value.Day, startTimeTime.Value.Hour, 0, 0).ToString("yyyy-MM-dd-HH-mm");
            foreach (var item in listTarget)
            {
                //ExecuteCaseName(item, startTime, endTime, startTimeWrite);
                await ExecuteCaseNameAsync(item, startTime, endTime, startTimeWrite);
            }
            //await Task.WhenAll(tasks);
        }
        private async Task ExecuteCaseNameAsync(ExportTarget item, string startTime, string endTime, string startTimeWrite)
        {
            var tempListInterceptName = new List<ExportObject>();
            try
            {
                tempListInterceptName = mainHelper.GetListInterceptName(item);
                List<Task> tasks = new List<Task>();
                foreach (var interceptNameObject in tempListInterceptName)
                {
                    tasks.Add(ProcessIntercept(interceptNameObject, startTime, endTime, startTimeWrite));
                }
                await Task.WhenAll(tasks);
                txtMultiLog.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    txtMultiLog.Text += Environment.NewLine + item.TargetName + ": Total " + tempListInterceptName.Count() + " targets is provisioning";
                });
            }
            catch (Exception ex)
            {
                txtMultiLog.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    txtMultiLog.Text += Environment.NewLine + "Unhandled error when export casename: " + item.TargetName + "-" + ex.Message;
                });
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
            }
            catch (Exception ex)
            {
                txtMultiLog.Invoke((MethodInvoker)delegate
                {
                    txtMultiLog.Text += Environment.NewLine + "Error when export Intercept Name: " + interceptNameObject.InterceptName + ", InterceptId: " + interceptNameObject.InterceptId + "-" + ex.Message;
                });
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
                    {
                        //initialData += getCallJsonString(itemExport, destinationPath) + ",";
                        initialData += getSMSJsonString(itemExport) + ",";
                    }
                    else
                    {
                        initialData += getCallJsonString(itemExport, destinationPath) + ",";
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


        private void btnStop_Click(object sender, EventArgs e)
        {
            //List<int> cont = new List<int>();
            //cont.Add(1);
            //cont.Add(2);
            //cont.Add(3);
            //cont.Add(4);
            //cont.Add(5);
            //cont.Add(6);
            //cont.Add(7);
            //cont.Add(8);
            //cont.Add(9);
            //cont.Add(10);
            //cont.Add(11);
            //cont.Add(12);

            //List<Task> task = new List<Task>();
            //foreach(var abc in cont)
            //{
            //    task.Add(ProcessTask(abc));
            //}
            //await Task.WhenAll(task);
            //var ms = results.ToList();


            //var startTigmt = startTi.AddHours(-7);
            //var endTimegmt = endTime.AddHours(-7);
            var sting1 = "1352565,52652355,1169464";
            var sting2 = "1352565,52652355,1169464,";
            var sm = sting1.TrimEnd(',');
            var sm2 = sting2.TrimEnd(',');
            var dr = "";
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
                var subUrl =  url.Replace("file://var/intellego/", "").Replace("/", @"\");
                var absoluteUrl = StaticKey.MAIN_URL_FOLDER + subUrl.Trim();

                var fileName = utility.getHI3FilenameFromUrl(exportObject.call_product_url);
                var destinationFullUrl = destinationPath + @"\" + fileName;
                if(absoluteUrl.IndexOf(".wav") > -1)
                {
                    if (!File.Exists(destinationFullUrl))
                    File.Copy(absoluteUrl, destinationFullUrl, false);
                }
                    
            //{

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

                foreach(var item in exportObject.listCGI)
                {
                    //var tempcgiObj = exportObject.listCGI[0];
                    tempDic.Add(new string[] { "Date:", item.timestamp.ToString("dd/MM/yyyy") });
                    tempDic.Add(new string[] { "Time:", item.timestamp.ToString("HH.mm.ss") });
                    tempDic.Add(new string[] {"CGI:",item.value});
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
                return data ;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            int minutesToNextHour = 60 - DateTime.Now.Minute;
            if(minutesToNextHour >= 53)
            {
                txtMultiLog.Text += Environment.NewLine + "Running at: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            }
            else
            {
                txtMultiLog.Text += Environment.NewLine + "NOT at: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //timer1.Interval = 120000; // specify interval time as you want
            //timer1.Tick += new EventHandler(timer1_Tick);

            mainTimer.Interval = 1000 * 60 * 5;
            mainTimer.Tick += new EventHandler(mainTimer_Tick);
            connectionString = helper.getConnectionString();

            var listObj = new List<CaseObject>();

            //listObj = mainHelper.GetListCaseObject();

            //Dictionary<string, string> test = new Dictionary<string, string>();
            //foreach (var item in listObj)
            //{
            //    test.Add(item.id, item.name);
            //}
            //comboBox1.DataSource = new BindingSource(test, null);
            //comboBox1.DisplayMember = "Value";
            //comboBox1.ValueMember = "Key";
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("You really want to quit?","Confirmation box",MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        
    }
}


