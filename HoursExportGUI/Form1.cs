using Quartz.Impl;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HoursExportGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            //scheduler.Start();
            //IJobDetail job = JobBuilder.Create<MainJob>().Build();
            //ITrigger trigger = TriggerBuilder.Create()
            // .StartAt(DateTime.Now)
            //   .WithCronSchedule("10 5 0/1 * * ?")
            //   //.WithCronSchedule("20 0/2 * * * ?")
            //   .WithPriority(1)
            //   .Build();
            //scheduler.ScheduleJob(job, trigger);
        }
    }
}
