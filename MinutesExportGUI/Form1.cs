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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MinutesExportGUI
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        private List<HotNumber> listNumber = new List<HotNumber>();
        private DBHelper helper = new DBHelper();
        private MainHelper mainHelper = new MainHelper();
        //private SQLServerHelper sqlserverHelper = new SQLServerHelper();
        private ExportHistoryEntities exportEntity = new ExportHistoryEntities();
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

            string[] lines = File.ReadAllLines("configs.txt");
            

            if(lines.Count() > 0)
            {
                //test
               
                string exportCase = lines[1];

                var tempExportTarget = new ExportTarget { Active = true, TargetId = "1", TargetName = exportCase };

                var tempListInterceptName = mainHelper.GetListIntercept2MinsName(exportCase);

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
                catch
                {
                    txtLog.Text += Environment.NewLine + "Cannot write log file";
                }
            }
            else
            {
                txtLog.Text += Environment.NewLine + "Error!!! Check config file";
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
            }
        }

        private void Write2MinsFile(List<ExportObject> listExport, string startTime, string casename, string interceptname)
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
            var destinationPath = exportPath + @"\AP_" + casename + "_2MINS_" + startTime;
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
                            InsertHI3ToRetrieve(absoluteUrl, destinationFullUrl);

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

        private void InsertHI3ToRetrieve(string source, string destination)
        {
            var log = "";
            try
            {
                string[] lines = File.ReadAllLines("HI3CopyConfig.txt");
                var tempDestination = lines[0];

                string path = tempDestination + @"\2mins" + DateTime.Now.ToString("-ddMMyyyy-HHmmss") + ".txt";

                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine(source);
                tw.WriteLine(destination);
                tw.Close();

                log = Environment.NewLine + "Added " + source + " to HI3Retrieve.";
            }
            catch (Exception ex) {
                log = Environment.NewLine + "Cannot add " + source + " to HI3Retrieve!!!" + ex.Message;
            }
            finally
            {
                txtLog.Invoke(new Action(() =>
                {
                    txtLog.Text += log;
                }));
                listLog.Add(new Log { dateLog = DateTime.Now, log = log });
            }
            
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
