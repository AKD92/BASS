

/* ***********************************************************************************************************************
 * 
 * Synopsis:
 * This is the implementation of TransPro Offer Email Job
 * Part of TransPro Automated System Services
 *          
 * Library Used:
 *          Quartz.NET Job Scheduler
 * 
 * Creation Date    :   01-March-2018
 * Programmed By    :   Ashis Kr. Das
 * 
 * **********************************************************************************************************************/




using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.SqlServer.Server;
using Quartz;
using BASS.Utilities;
using System.Diagnostics;

namespace BASS.SystemServices.EmailAlertService.OfferEmailImpl
{

    [DisallowConcurrentExecution]
    public class TranslatorOfferEmailJob : IJob
    {

        public static IDictionary<string, dynamic> ApplicationData;
        private static IDictionary<string, string> Section3Data;
        private static IDictionary<string, string> CommentLightData;
        private static IDictionary<string, string> CommentBusinessData;
        private static IDictionary<string, string> CommentExpertData;
        
        private SqlMetaData[] Col_NewLogEntry;
        private SqlMetaData[] Col_OldLogEntry;
        private List<SqlDataRecord> NewLogList;
        private List<SqlDataRecord> ModifyLogList;

        private StringBuilder Log;

        public TranslatorOfferEmailJob()
        {
            SqlMetaData C_ID, C_OID, C_TID, C_ST, C_MSG;
            C_ID = new SqlMetaData("ID", SqlDbType.UniqueIdentifier);
            C_OID = new SqlMetaData("OrderID", SqlDbType.UniqueIdentifier);
            C_TID = new SqlMetaData("TranslatorID", SqlDbType.UniqueIdentifier);
            C_ST = new SqlMetaData("Status", SqlDbType.VarChar, 50);
            C_MSG = new SqlMetaData("ErrorMessage", SqlDbType.NVarChar, -1);
            Col_NewLogEntry = new SqlMetaData[] { C_OID, C_TID, C_ST, C_MSG };
            Col_OldLogEntry = new SqlMetaData[] { C_ID, C_ST, C_MSG };
            NewLogList = new List<SqlDataRecord>();
            ModifyLogList = new List<SqlDataRecord>();
            Log = new StringBuilder();
        }

        static TranslatorOfferEmailJob()
        {
            ApplicationData = new Dictionary<string, dynamic>();
            Section3Data = new Dictionary<string, string>();
            CommentLightData = new Dictionary<string, string>();
            CommentBusinessData = new Dictionary<string, string>();
            CommentExpertData = new Dictionary<string, string>();
            Section3Data["en"] = "b-cause inc.\nCloud-based translation website trans-Pro.\nhttps://www.trans-pro.com.au/";
            CommentLightData["en"] = @"Documents which does not required special knowledge or skill for translation, will be ordered as Light level translation. 
We recommend you to start working from translation orders in Light level, get some evaluation and trust from our client, then try for the Business and Expert level translation works.";
            CommentBusinessData["en"] = @"In Business level translation, some special knowledge and skill will be required for translation. 
We request you to communicate with client and meet their requirment for translation.";
            CommentExpertData["en"] = @"Special knowledge and experience is required for Expert level transration. We should provide qualified, perfect translation to our customer. (We are giving this offer only for the certified professional translators.)";

            ApplicationData["Transpro_AU"] = new { ApplicationID = 2, CultureID = "en", ProcedureName = "SP_TransProOfferEmail_AU_GREEDY" };
            ApplicationData["Transpro_JP"] = new { ApplicationID = 4, CultureID = "jp", ProcedureName = string.Empty };
        }


        public void Execute(IJobExecutionContext ExecutionContext)
        {
            try
            {
                this.SendOfferEmail(ApplicationData["Transpro_AU"], ExecutionContext);
            }
            catch (Exception ex)
            {
                string SourceName = ConfigurationManager.AppSettings["OfferEmailSource"];
                string Message = Utility.DeepestExceptionMessage(ex);
                string FullMessage = string.Format("ROOT LEVEL ERROR OCCURED.\n{0}", Message);
                EventLog ServiceLog = (EventLog)ExecutionContext.MergedJobDataMap["ServiceLog"];
                ServiceLog.WriteEntry(FullMessage, EventLogEntryType.Error);
            }
            Log.Clear();
        }

        public void SendOfferEmail(dynamic ApplicationContext, IJobExecutionContext ExecutionContext)
        {
            bool WriteEventLog = true;
            SqlCommand cmd;
            SqlDataReader DataReader = null;
            List<OfferEmailModel> emailList = new List<OfferEmailModel>();
            OfferEmailModel eModel;
            string LogName = ConfigurationManager.AppSettings["ServiceLog"];
            string SourceName = ConfigurationManager.AppSettings["OfferEmailSource"];
            string ConnectionString = ConfigurationManager.ConnectionStrings["CentralConnection"].ConnectionString;
            EventLog ServiceLog = (EventLog)ExecutionContext.MergedJobDataMap["ServiceLog"];

            string InitMessage = string.Format("Initiating to run offer email algorithm.\nALGORITHM: {0}\nDB: {1}",
                                                    ApplicationContext.ProcedureName, ConnectionString);
            ServiceLog.WriteEntry(InitMessage, EventLogEntryType.Warning);
            SqlConnection dbConnection = new SqlConnection(ConnectionString);

            try
            {
                dbConnection.Open();
                cmd = new SqlCommand(ApplicationContext.ProcedureName, dbConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                DataReader = cmd.ExecuteReader();
                Log.AppendLine("Procedure ran. Reading list of eligible translators.");
                while (DataReader.Read() == true)
                {
                    eModel = new OfferEmailModel();
                    eModel.OrderID = DataReader["OrderID"] == DBNull.Value ? (Guid?)null : Guid.Parse(DataReader["OrderID"].ToString());
                    eModel.OrderNo = DataReader["OrderNo"].ToString();
                    eModel.TranslatorID = DataReader["TranslatorID"] == DBNull.Value ? (Guid?)null : Guid.Parse(DataReader["TranslatorID"].ToString());
                    eModel.EmailTo = DataReader["EmailAddress"].ToString();
                    eModel.TranslationType = Convert.ToInt32(DataReader["TranslationType"] == DBNull.Value ? "0" : DataReader["TranslationType"].ToString());
                    eModel.DeliveryLevelName = DataReader["DeliveryLevelName"].ToString();
                    eModel.OfferLogID = DataReader["OfferLogID"] == DBNull.Value ? (Guid?)null : Guid.Parse(DataReader["OfferLogID"].ToString());
                    eModel.LogCommand = DataReader["LogCommand"].ToString();
                    emailList.Add(eModel);
                    Log.AppendFormat("--> Order: {0} Log: {2} Translator: {1}.", eModel.OrderNo, eModel.EmailTo, eModel.LogCommand);
                    Log.AppendLine();
                }
            }
            catch (Exception ex)
            {
                string message = Utility.DeepestExceptionMessage(ex);
                Utility.SetErrorLog(null, SourceName, message);
                Log.AppendFormat("Error while reading translator information list: {0}", message);
                Log.AppendLine();
                goto END;
            }
            finally
            {
                if (DataReader != null)
                    DataReader.Close();
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }
            if (emailList.Count == 0)
            {
                Log.AppendLine("No eligible translator was found to send offer email.");
                WriteEventLog = false;
                goto END;
            }
            
            StringBuilder Builder = new StringBuilder();
            OrderFilter FilterOrder = new OrderFilter();
            FilterOrder.cultureId = ApplicationContext.CultureID;
            FilterOrder.ApplicationId = ApplicationContext.ApplicationID;

            Log.AppendFormat("Sending email to {0} translator(s).", emailList.Count);
            Log.AppendLine();
            foreach (OfferEmailModel model in emailList)
            {
                bool SendStatus = false;
                string ErrorMessage = null;
                OrderWebModel OrderModel = null;
                FilterOrder.orderNo = model.OrderNo;

                try
                {
                    OrderModel = DataAccess.GetOrderDetailsById(FilterOrder);
                    string Code = null;
                    string Comment = string.Empty;

                    if (model.DeliveryLevelName == "Light")
                    {
                        Code = "04003";
                        Comment = CommentLightData[ApplicationContext.CultureID];
                    }
                    else if (model.DeliveryLevelName == "Expert")
                    {
                        Code = "04012";
                        Comment = CommentExpertData[ApplicationContext.CultureID];
                    }
                    else if (model.DeliveryLevelName == "Business")
                    {
                        Code = "04013";
                        Comment = CommentBusinessData[ApplicationContext.CultureID];
                    }
                    else
                    {
                        continue;
                    }

                    if (model.TranslationType == 2 || model.TranslationType == 5)       // Type is Appointed or Appointed-Native-Check
                    {
                        Code = "04004";
                    }

                    string OrderTitle = string.Format("{0}->{1} {2} {3} {4}", OrderModel.SourceLanguage, OrderModel.TargetLanguage, OrderModel.TranslationTypeName, OrderModel.TranslationFieldName, OrderModel.DeliveryPlan);
                    int CharCount = (int)(OrderModel.CountType == 1 ? OrderModel.WordCount : OrderModel.CharacterCount);
                    string CountType = OrderModel.CountType == 1 ? "words" : "characters";
                    EmailTemplateModel Template = DataAccess.GetEmailTemplateByTemplateCode(Code, ApplicationContext.CultureID);

                    TranslatorPaymentQueryModel TrPaymentModel = new TranslatorPaymentQueryModel();
                    TrPaymentModel.OrderNo = model.OrderNo;
                    TrPaymentModel.TranslatorID = model.TranslatorID.Value;
                    DataAccess.GetTranslatorPaymentAmount(TrPaymentModel);

                    decimal PaymentAmount_AU, PaymentAmount_JP;
                    if (TrPaymentModel.ReturnValue == 0)
                    {
                        DataAccess.DirectCurrencyConversion(OrderModel.CurrencyID.Value, 3, OrderModel.OrderDate.Value, TrPaymentModel.TranslatorPayment, out PaymentAmount_JP);
                        DataAccess.DirectCurrencyConversion(OrderModel.CurrencyID.Value, 7, OrderModel.OrderDate.Value, TrPaymentModel.TranslatorPayment, out PaymentAmount_AU);
                    }
                    else
                    {
                        PaymentAmount_JP = PaymentAmount_AU = 0.0M;
                    }


                    Builder.Clear();
                    Builder.Append(Template.Body);
                    Builder.Replace("[SECTION-1]", "");
                    Builder.Replace("[SECTION-3]", Section3Data[ApplicationContext.CultureID]);
                    Builder.Replace("%txt01%", OrderTitle);
                    Builder.Replace("%txt02%", OrderModel.SourceLanguage);
                    Builder.Replace("%txt03%", string.Format("{0} {1}", CharCount.ToString(), CountType));
                    Builder.Replace("%txt04%", OrderModel.DeliveryPlan);
                    Builder.Replace("%txt07%", OrderModel.OrderNo);
                    Builder.Replace("%txt08%", Comment);
                    Builder.Replace("%txt09%", OrderModel.MenuScript.Substring(0, OrderModel.MenuScript.Length / 3));
                    Builder.Replace("%txt10%", string.Format("{0} AUD ({1} JPY)", PaymentAmount_AU.ToString(), PaymentAmount_JP.ToString()));

                    try
                    {
                        SendStatus = Utility.SendEmail(model.EmailTo, null, null, Template.Subject, Builder.ToString(), null, false);
                        ErrorMessage = null;
                        Log.AppendFormat("--> Order: {0}, Stat: SENT {1}.", OrderModel.OrderNo, model.EmailTo);
                        Log.AppendLine();
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = Utility.DeepestExceptionMessage(ex);
                        Log.AppendFormat("--> Order: {0}, Stat: NOT SENT {1} Error: {2}.", OrderModel.OrderNo, model.EmailTo, ErrorMessage);
                        Log.AppendLine();
                    }
                    if (model.LogCommand == "INSERT")
                    {
                        SqlDataRecord Record = new SqlDataRecord(Col_NewLogEntry);
                        Record.SetValue(Record.GetOrdinal("OrderID"), model.OrderID);
                        Record.SetValue(Record.GetOrdinal("TranslatorID"), model.TranslatorID);
                        Record.SetValue(Record.GetOrdinal("Status"), (SendStatus == true ? "SENT" : "NOT SENT"));
                        if (ErrorMessage == null)
                            Record.SetDBNull(Record.GetOrdinal("ErrorMessage"));
                        else
                            Record.SetValue(Record.GetOrdinal("ErrorMessage"), ErrorMessage);
                        NewLogList.Add(Record);
                    }
                    else if (model.LogCommand == "UPDATE")
                    {
                        SqlDataRecord Record = new SqlDataRecord(Col_OldLogEntry);
                        Record.SetValue(Record.GetOrdinal("ID"), model.OfferLogID);
                        Record.SetValue(Record.GetOrdinal("Status"), SendStatus);
                        if (ErrorMessage == null)
                            Record.SetDBNull(Record.GetOrdinal("ErrorMessage"));
                        else
                            Record.SetValue(Record.GetOrdinal("ErrorMessage"), ErrorMessage);
                        ModifyLogList.Add(Record);
                    }
                }
                catch (Exception ex)
                {
                    string message = Utility.DeepestExceptionMessage(ex);
                    Utility.SetErrorLog(null, SourceName, message);
                    Log.AppendFormat("Error while accessing database: {0}", message);
                    Log.AppendLine();
                }
            }

            Log.AppendLine("Updating offer email log data.");
            try
            {
                dbConnection.Open();
                cmd = new SqlCommand("SP_LogOfferEmailInfo_TVP", dbConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter ParamNewLog = new SqlParameter("@LOG_NEW", SqlDbType.Structured);
                ParamNewLog.Direction = ParameterDirection.Input;
                ParamNewLog.TypeName = "TYPE_NEW_EMAILLOG";
                ParamNewLog.Value = NewLogList.Count == 0 ? null : NewLogList;
                SqlParameter ParamModifyLog = new SqlParameter("@LOG_MODIFY", SqlDbType.Structured);
                ParamModifyLog.Direction = ParameterDirection.Input;
                ParamModifyLog.TypeName = "TYPE_MODIFY_EMAILLOG";
                ParamModifyLog.Value = ModifyLogList.Count == 0 ? null : ModifyLogList;
                cmd.Parameters.Add(ParamNewLog);
                cmd.Parameters.Add(ParamModifyLog);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                string message = Utility.DeepestExceptionMessage(ex);
                Utility.SetErrorLog(null, SourceName, message);
                Log.AppendFormat("Error while updating log data: {0}", message);
                Log.AppendLine();
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }
            
            END:
            NewLogList.Clear();
            ModifyLogList.Clear();
            
            Log.AppendLine("Process finished.");
            if (WriteEventLog == true)
                ServiceLog.WriteEntry(Log.ToString(), EventLogEntryType.Information);
            return;
        }

    }
}
