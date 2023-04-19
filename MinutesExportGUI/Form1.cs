using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MinutesExportGUI
{
    public partial class Form1 : Form
    {
        private Timer timer1 = new Timer();
        private List<HotNumber> listNumber = new List<HotNumber>();
        private DBHelper helper = new DBHelper();
        private MainHelper mainHelper = new MainHelper();
        private SQLServerHelper sqlserverHelper = new SQLServerHelper();
        private Utility utility = new Utility();
        private List<Log> listLog = new List<Log>();
        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 120000;
            timer1.Tick += new EventHandler(timer_Tick);
            timer1.Start();

        }
        private async void timer_Tick(object sender, EventArgs e)
        {
            listLog = new List<Log>();
            txtLog.Text += Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "Start 2 minutes export session";
            //Call method
            listNumber = sqlserverHelper.GetAllHotNumber();

            var currentTarget = listNumber[0];

            var tempExportTarget = new ExportTarget { Active = true, TargetId = currentTarget.InterceptId, TargetName = currentTarget.CaseName };

            var tempListInterceptName = mainHelper.GetListIntercept2MinsName(tempExportTarget);

            var currentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 00);
            var startTime = (currentTime).AddHours(-7).AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endTime = (currentTime).AddHours(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var startTimeWrite = currentTime.ToString("yyyy-MM-dd HH-mm");

            List<Task> tasks = new List<Task>();

            foreach (var item in tempListInterceptName)
            {
                var tempTarget = new ExportObject { InterceptId = item.InterceptId, InterceptName = item.InterceptName, CaseName = item.CaseName };
                tasks.Add(ProcessIntercept(tempTarget, startTime, endTime, startTimeWrite));
            }
            await Task.WhenAll(tasks);

            try
            {
                var fileDirectory = @"C:\Logs\2Mins\";
                string path = fileDirectory + DateTime.Now.ToString("yyyyMMddHH");
                Directory.CreateDirectory(path);
                var fullPath = path + @"\2mins.txt";
                var listItem = listLog.OrderBy(m => m.dateLog).ToList();
                using (StreamWriter sw = (File.Exists(fullPath)) ? File.AppendText(fullPath) : File.CreateText(fullPath))
                {
                    foreach (var line in listItem)
                    {
                        sw.WriteLine(line.log);
                    }
                }
            }
            catch {
                txtLog.Text += Environment.NewLine + "Cannot write log file";
            }
            //txtLog.Text += Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "Stop 2 minutes export session";

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
                var finalExportList = mainHelper.ExecuteInterceptName(interceptNameObject, startTime, endTime, startTimeWrite, ReExportType.Minute.ToString());
                if (finalExportList.Count() > 0)
                {
                    Write2MinsFile(finalExportList, startTimeWrite, interceptNameObject.CaseName, interceptNameObject.InterceptName);
                    string log = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "[DONE] Exported Intercept name " + interceptNameObject.InterceptName;
                    DateTime dateLog = DateTime.Now;
                    txtLog.Invoke(new Action(() =>
                    {
                        txtLog.Text += Environment.NewLine + log;
                    }));
                    listLog.Add(new Log { dateLog = dateLog, log = log });
                    //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "[DONE] Exported Intercept name " + interceptNameObject.InterceptName);
                }

            }
            catch (Exception ex)
            {
                string log = DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when export Intercept name " + interceptNameObject.InterceptName + " " + ex.Message;
                DateTime dateLog = DateTime.Now;

                txtLog.Invoke(new Action(() =>
                {
                    txtLog.Text += Environment.NewLine + log;
                }));
                listLog.Add(new Log { dateLog = dateLog, log = log });
                //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm: ") + "[FAILED] Error when export Intercept name " + interceptNameObject.InterceptName + " " + ex.Message);
                //sqlserverHelper.InsertLogToDB("Error " + ex.Message, DateTime.Now, interceptNameObject.CaseName, ErrorType.MinuteError.ToString(), interceptNameObject.InterceptId, interceptNameObject.InterceptName);
            }
        }

        private void Write2MinsFile(List<ExportObject> listExport, string startTime, string casename, string interceptname)
        {
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
            var destinationPath = StaticKey.EXPORT_2MINS_FOLDER + @"\AP_" + casename + "_2MINS_" + convertedInterceptName + "_" + startTime;
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
                            sqlserverHelper.InsertHI3ToRetrieve(absoluteUrl, destinationFullUrl);

                        }
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

        private void button1_Click(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
            //txtLog.Refresh();
        }
    }
}
