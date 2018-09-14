
using System;
using System.ServiceProcess;
using System.Configuration;
using Quartz;
using Quartz.Impl;
using System.Diagnostics;
using System.Threading;
using BASS.SystemServices.EmailAlertService.OfferEmailImpl;

namespace BASS.SystemServices.EmailAlertService
{
    public class OfferEmailService : ServiceBase
    {

        private IScheduler Scheduler;
        private string AutoOrderEmail_CronExpr;
        private string LogName, SourceName;
        private static string FRMT_INIT;
        
        public EventLog ServiceLog { get; }

        static OfferEmailService()
        {
            FRMT_INIT = "Service started successfully.\nCurrent time (UTC): {0}.\nNext execution time (UTC): {1}.";
        }

        public OfferEmailService(string ServiceName) : base()
        {
            this.ServiceName = ServiceName;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
            this.AutoLog = false;
            this.ExitCode = 0;
            AutoOrderEmail_CronExpr = "0 */10 * ? * * *";    //"0 */10 * ? * * *";
            Scheduler = null;
            LogName = ConfigurationManager.AppSettings["ServiceLog"];
            SourceName = ConfigurationManager.AppSettings["OfferEmailSource"];
            if (EventLog.SourceExists(SourceName) == false)
            {
                EventSourceCreationData Data = new EventSourceCreationData(SourceName, LogName);
                EventLog.CreateEventSource(Data);
            }
            ServiceLog = new EventLog();
            ServiceLog.Source = SourceName;
        }

        protected override void OnStart(string[] args)
        {
            string DateNow, DateNext;
            base.OnStart(args);
            
            if (Scheduler == null)
            {
                Scheduler = StdSchedulerFactory.GetDefaultScheduler();
            }
            Scheduler.Start();
            
            TriggerBuilder Builder = TriggerBuilder.Create();
            Builder = Builder.WithCronSchedule(AutoOrderEmail_CronExpr, (CronSchBuilder) => {
                CronSchBuilder.InTimeZone(TimeZoneInfo.Utc);
                CronSchBuilder.WithMisfireHandlingInstructionFireAndProceed();
            });
            ITrigger AutoEmailTrigger = Builder.Build();
            IJobDetail AutoOrderEmailJob = JobBuilder.Create(typeof(TranslatorOfferEmailJob)).Build();
            AutoOrderEmailJob.JobDataMap["ServiceLog"] = this.ServiceLog;
            Scheduler.ScheduleJob(AutoOrderEmailJob, AutoEmailTrigger);

            DateNow = DateTime.UtcNow.ToString("F");
            DateNext = AutoEmailTrigger.GetNextFireTimeUtc().Value.DateTime.ToString("F");
            ServiceLog.WriteEntry(string.Format(FRMT_INIT, DateNow, DateNext), EventLogEntryType.Information);
            
            if (this.ServiceLog.MaximumKilobytes < 51200)
            {
                this.ServiceLog.MaximumKilobytes = 51200;
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
    }
}
