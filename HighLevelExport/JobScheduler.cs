using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighLevelExport
{
    public  class JobScheduler
    {
        public async void Start()
        {
            //ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            //IScheduler scheduler = await schedulerFactory.GetScheduler();
            //await scheduler.Start();

            //IJobDetail job = JobBuilder.Create<helloJob>().Build();

            //ITrigger trigger = TriggerBuilder.Create()
            //    .WithIdentity("IDGJob", "IDG")
            //    .StartAt(DateTime.Now)
            //    .WithCronSchedule("10 0/5 * * * ?")
            //    .WithPriority(1)
            //    .Build();

            //await scheduler.ScheduleJob(job, trigger);

        }
    }
}
