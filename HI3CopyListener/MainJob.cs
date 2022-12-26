using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HI3CopyListener
{
    public class MainJob : IJob
    {
        private SQLServerHelper sqlServerHelper = new SQLServerHelper();
        public void RetrieveData()
        {
            try
            {
                var listData = sqlServerHelper.GetListHI3Retrieve();
                var listDelete = new List<HI3Retrieve>();
                if(listData.Count() > 0)
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm ") + "[INFO] Starting new copy session... ");
                foreach (var item in listData)
                {
                    try
                    {
                        var lastindex = item.DestinationPath.LastIndexOf(@"\");
                        var destinationFolder = item.DestinationPath.Substring(0, lastindex);
                        Directory.CreateDirectory(destinationFolder);

                        File.Copy(item.SourcePath, item.DestinationPath, true);
                        listDelete.Add(item);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm ") + "[DONE] Copy from " + item.SourcePath + " to " + item.DestinationPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm ") + "[ERROR] Failed when copy " + item.SourcePath + " " + ex.Message);
                    }
                }
                if(listDelete.Count() > 0)
                {
                    sqlServerHelper.DeleteHI3Range(listDelete);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "[ERROR] Unhandled error " + ex.Message);
            }
        }

        void IJob.Execute(IJobExecutionContext context)
        {
            //throw new NotImplementedException();
            RetrieveData();
        }
    }
}
