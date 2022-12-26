using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallReExportListener
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\r\n");
            Console.WriteLine("=======================================================================================================");
            Console.WriteLine("**********************************Start Re-Export call data every 5 hour********************************");
            Console.WriteLine("=======================================================================================================");
            Console.WriteLine("\r\n");
            //Console.WriteLine("Start simple job");

            Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info }; Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info };

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail job = JobBuilder.Create<MainJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
             .StartAt(DateTime.Now)
               //.WithCronSchedule("20 5 0/1 * * ?")
               .WithCronSchedule("20 0/3 * * * ?")
               .WithPriority(1)
               .Build();
            scheduler.ScheduleJob(job, trigger);
        }
    }
}
