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

        public void RetrieveData()
        {
            string[] lines = File.ReadAllLines("configs.txt");
            var copyHI3folder = lines[0];
            try
            {
                foreach (string file in Directory.EnumerateFiles(copyHI3folder, "*.txt"))
                {
                    var contents = File.ReadAllLines(file);
                    var tempDestination = contents[1];
                    var tempSource = contents[0];
                    try
                    {
                        var lastindex = tempDestination.LastIndexOf(@"\");
                        var destinationFolder = tempDestination.Substring(0, lastindex);
                        Directory.CreateDirectory(destinationFolder);
                        File.Copy(tempSource, tempDestination, true);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm ") + "[DONE] Copy from " + tempSource + " to " + tempDestination);

                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm ") + "[ERROR] Failed when copy " + tempSource + " " + ex.Message);
                    }
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
