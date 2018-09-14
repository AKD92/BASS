

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




using Quartz;
using System;
using System.Net;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using BASS.Utilities;


namespace BASS.SystemServices.CurrencyService.CurrencyRateUpdateImpl
{
    public class CurrencyRateUpdateJob : IJob
    {
        private int MaxRetryCount;
        private string BaseURL;
        private NameValueCollection NameValue;
        private string SuccessFormat, ErrorFormat, ApiRequestFailFormat, ApiErrorFormat;

        public CurrencyRateUpdateJob()
        {
            BaseURL = ConfigurationManager.AppSettings["CurrencyApiBaseUrl"];
            this.MaxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaximumRetryCount"]);

            NameValue = new NameValueCollection();
            NameValue["access_key"] = ConfigurationManager.AppSettings["CurrencyApiAccessKey"];
            NameValue["currencies"] = ConfigurationManager.AppSettings["ForeignCurrency"];
            NameValue["source"] = ConfigurationManager.AppSettings["BaseCurrency"];

            SuccessFormat = "Operation successful.\r\nLive Currency (JSON):\r\n{0}\r\nSuccessful Insertion Count:\r\n{1}\r\nRate Exchange Timestamp (UTC):\r\n{2}\r\nCurrent Timestamp (UTC):\r\n{3}\r\nNext Schedule (UTC):\r\n{4}\r\n";
            ErrorFormat = "Error while executing service:\r\n{0}\r\nCurrent Timestamp (UTC):\r\n{1}\r\nNext Schedule (UTC):\r\n{2}\r\n";
            ApiRequestFailFormat = "Web request failed while obtaining live currency data.\r\nMessage: {0}\r\nAttempting for Retry No. {1} out of {2}\r\n";
            ApiErrorFormat = "Currency API server returned error.\r\nError Code: {0}\r\nError Info: {1}\r\nRate Exchange Timestamp (UTC):\r\n{2}\r\nCurrent Timestamp (UTC):\r\n{3}\r\nNext Schedule (UTC):\r\n{4}\r\n";
        }

        public DateTime UnixTimeStampToUTC(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

        public void Execute(IJobExecutionContext context)
        {
            WebClient Client = null;
            int InsertCount = 0;
            string JsonResponse, Message;
            List<SqlDataRecord> CurrencyTable;
            EventLog Log = (EventLog) context.MergedJobDataMap["ServiceLog"];
            string LogName = ConfigurationManager.AppSettings["ServiceLog"];
            string SourceName = ConfigurationManager.AppSettings["CurrencyUpdateSource"];
            string NextFire = context.NextFireTimeUtc.HasValue ? context.NextFireTimeUtc.Value.DateTime.ToString("F") : "Unavailable";
            int RetryCount = 0;
            DateTime OpTime, CurrentTime;

            try
            {
                CLAYER_API_REQUEST:
                try
                {
                    Client = new WebClient();
                    Client.QueryString = NameValue;
                    JsonResponse = Client.DownloadString(BaseURL);
                }
                catch (Exception ex)
                {
                    if (RetryCount < this.MaxRetryCount)
                    {
                        Thread.Sleep(1000);
                        RetryCount += 1;
                        Message = string.Format(ApiRequestFailFormat, Utility.DeepestExceptionMessage(ex), RetryCount, this.MaxRetryCount);
                        Log.WriteEntry(Message, EventLogEntryType.Warning);
                        goto CLAYER_API_REQUEST;
                    }
                    else
                    {
                        throw ex;
                    }
                }
                
                CurrencyModel LiveCurrency = JsonConvert.DeserializeObject<CurrencyModel>(JsonResponse);
                OpTime = this.UnixTimeStampToUTC(LiveCurrency.timestamp);
                CurrentTime = DateTime.UtcNow;
                if (LiveCurrency.error == null)
                {
                    CurrencyTable = this.BuildSqlRecordList(LiveCurrency);
                    InsertCount = this.WriteDatabase(CurrencyTable);
                    Message = string.Format(SuccessFormat, JsonResponse, InsertCount, OpTime.ToString("F"), CurrentTime.ToString("F"), NextFire);
                    Log.WriteEntry(Message, EventLogEntryType.Information);
                }
                else
                {
                    Message = string.Format(ApiErrorFormat, LiveCurrency.error.code, LiveCurrency.error.info, OpTime.ToString("F"), CurrentTime.ToString("F"), NextFire);
                    Log.WriteEntry(Message, EventLogEntryType.Error);
                }
            }
            catch (Exception ex)
            {
                Message = string.Format(ErrorFormat, Utility.DeepestExceptionMessage(ex), DateTime.UtcNow.ToString("F"), NextFire);
                Log.WriteEntry(Message, EventLogEntryType.Error);
            }
        }

        private List<SqlDataRecord> BuildSqlRecordList(CurrencyModel DataModel)
        {
            string CurrencyCode;
            decimal CurrencyRate;
            PropertyInfo[] PublicProps;
            Type ModelType;
            SqlDataRecord Record;
            SqlMetaData[] Columns;
            SqlMetaData C_Code, C_Rate;
            List<SqlDataRecord> CurrencyTable = new List<SqlDataRecord>();
            C_Code = new SqlMetaData("Code", SqlDbType.NVarChar, 5);
            C_Rate = new SqlMetaData("Rate", SqlDbType.Decimal, 18, 2);
            Columns = new SqlMetaData[] { C_Code, C_Rate };

            ModelType = typeof(CurrencyRateModel);
            PublicProps = ModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (PropertyInfo Property in PublicProps)
            {
                CurrencyCode = Property.Name.ToString();
                CurrencyRate = Convert.ToDecimal(Property.GetValue(DataModel.quotes).ToString());
                Record = new SqlDataRecord(Columns);
                Record.SetValue(Record.GetOrdinal("Code"), CurrencyCode.Substring(3));
                Record.SetValue(Record.GetOrdinal("Rate"), Math.Round(CurrencyRate, 2));
                CurrencyTable.Add(Record);
            }
            Record = new SqlDataRecord(Columns);
            Record.SetValue(Record.GetOrdinal("Code"), DataModel.source);
            Record.SetValue(Record.GetOrdinal("Rate"), 1.00M);
            CurrencyTable.Add(Record);
            return CurrencyTable;
        }


        private int WriteDatabase(List<SqlDataRecord> CurrencyTable)
        {
            DbCommand cmd = null;
            SqlConnection _dbConnection;
            SqlParameter CurrencyTVP, CalcDate;
            int EntryCount = 0;
            _dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CentralConnection"].ConnectionString);

            try
            {
                _dbConnection.Open();
                cmd = new SqlCommand("SP_InsertCurrencyRates_TVP", _dbConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                CurrencyTVP = new SqlParameter("@CURRENCY_TVP", SqlDbType.Structured);
                CurrencyTVP.Direction = ParameterDirection.Input;
                CurrencyTVP.TypeName = "TYPE_CurrencyRate";
                CurrencyTVP.Value = CurrencyTable;
                cmd.Parameters.Add(CurrencyTVP);
                CalcDate = new SqlParameter("@DOMAIN_DATE", SqlDbType.DateTime);
                CalcDate.Direction = ParameterDirection.Input;
                CalcDate.Value = DateTime.UtcNow;
                cmd.Parameters.Add(CalcDate);
                EntryCount = cmd.ExecuteNonQuery();
            }
            finally
            {
                if (_dbConnection.State == ConnectionState.Open)
                    _dbConnection.Close();
            }
            return EntryCount;
        }
    }
}
