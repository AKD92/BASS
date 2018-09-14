

/* ***********************************************************************************************************************
 * 
 * Synopsis:
 * Trans-Pro Live Currency Update service is implemented.
 * Using Quartz Job Scheduler library.
 * CRON scheduling method is used.
 * 
 * Creation Date    :   10-July-2018
 * Programmed By    :   Ashis Kr. Das
 * 
 * **********************************************************************************************************************/




using System;
using System.ServiceProcess;
using System.Configuration;
using Quartz;
using Quartz.Impl;
using System.Diagnostics;
using System.Threading;
using BASS.SystemServices.CurrencyService.CurrencyRateUpdateImpl;


namespace BASS.SystemServices.CurrencyService
{
    public class CurrencyRateUpdateService : ServiceBase
    {

        private IScheduler Scheduler;
        private string CronMonthly, CronTest;
        private string LogName, SourceName;
        private static string FRMT_INIT;

        public EventLog ServiceLog { get; }

        static CurrencyRateUpdateService()
        {
            FRMT_INIT = "Service started successfully.\nCurrent time (UTC): {0}.\nNext execution time (UTC): {1}.";
        }

        public CurrencyRateUpdateService(string ServiceName) : base()
        {
            this.ServiceName = ServiceName;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
            this.AutoLog = false;
            this.ExitCode = 0;
            Scheduler = null;
            CronMonthly = "0 0 0 1 * ? *";
            CronTest = "0 * 0 ? * * *";
            LogName = ConfigurationManager.AppSettings["ServiceLog"];
            SourceName = ConfigurationManager.AppSettings["CurrencyUpdateSource"];
            if (EventLog.SourceExists(SourceName) == false)
            {
                EventSourceCreationData Data = new EventSourceCreationData(SourceName, LogName);
                EventLog.CreateEventSource(Data);
            }
            this.ServiceLog = new EventLog();
            this.ServiceLog.Source = SourceName;
        }

        protected override void OnStart(string[] args)
        {
            ITrigger CurrencyTrigger;
            IJobDetail CurrencyJobDetail;
            TriggerBuilder TriggerSpec;
            string DateNow, DateNext;

            base.OnStart(args);
            if (Scheduler == null)
            {
                Scheduler = StdSchedulerFactory.GetDefaultScheduler();
            }
            Scheduler.Start();

            TriggerSpec = TriggerBuilder.Create();
            TriggerSpec = TriggerSpec.WithCronSchedule(CronMonthly, (CronSchBuilder) => {
                CronSchBuilder.InTimeZone(TimeZoneInfo.Utc);
                CronSchBuilder.WithMisfireHandlingInstructionFireAndProceed();
            });
            CurrencyTrigger = TriggerSpec.Build();
            CurrencyJobDetail = JobBuilder.Create(typeof(CurrencyRateUpdateJob)).Build();
            CurrencyJobDetail.JobDataMap["ServiceLog"] = this.ServiceLog;
            Scheduler.ScheduleJob(CurrencyJobDetail, CurrencyTrigger);
            DateNow = DateTime.UtcNow.ToString("F");
            DateNext = CurrencyTrigger.GetNextFireTimeUtc().HasValue ? CurrencyTrigger.GetNextFireTimeUtc().Value.DateTime.ToString("F") : "Unavailable";
            ServiceLog.WriteEntry(string.Format(FRMT_INIT, DateNow, DateNext), EventLogEntryType.Information);
            
            if (ServiceLog.MaximumKilobytes < 51200)
            {
                ServiceLog.MaximumKilobytes = 51200;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            Scheduler.PauseAll();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
            Scheduler.ResumeAll();
        }

        protected override void OnStop()
        {
            base.OnStop();
            Scheduler.Shutdown(true);
            Scheduler = null;
            ServiceLog.WriteEntry("Service stopped successfully.", EventLogEntryType.Information);
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            Scheduler.Shutdown(false);
            Scheduler = null;
            ServiceLog.WriteEntry("Service stopped successfully. The system is shutting down.", EventLogEntryType.Information);
        }


        private void ConfigureScheduleBuilder(DailyTimeIntervalScheduleBuilder ScheduleBuilder)
        {
            //ScheduleBuilder = ScheduleBuilder.InTimeZone(TimeZoneInfo.Utc);
            //ScheduleBuilder = ScheduleBuilder.StartingDailyAt(new TimeOfDay(0, 0, 5));
            //ScheduleBuilder = ScheduleBuilder.WithInterval(1, IntervalUnit.Month);

            DateTimeOffset Df = DateTimeOffset.Now.AddSeconds(5.0);
            ScheduleBuilder = ScheduleBuilder.InTimeZone(TimeZoneInfo.Local);
            ScheduleBuilder = ScheduleBuilder.StartingDailyAt(new TimeOfDay(Df.Hour, Df.Minute, Df.Second));
            ScheduleBuilder = ScheduleBuilder.WithInterval(20, IntervalUnit.Second);
        }
    }
}