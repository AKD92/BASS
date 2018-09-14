using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using BASS.Utilities;

namespace BASS.SystemServices.EmailAlertService.OfferEmailImpl
{
    internal class DataAccess
    {

        public static EmailTemplateModel GetEmailTemplateByTemplateCode(string TempCode, string CultureId)
        {
            SqlCommand cmd;
            EmailTemplateModel Model = null;
            SqlDataReader Reader = null;
            SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CentralConnection"].ConnectionString);

            try
            {
                dbConnection.Open();
                cmd = new SqlCommand("SP_GetEmailTemplateBy_TemplateCode", dbConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TemplateCode", TempCode);
                cmd.Parameters.AddWithValue("@CultureId", CultureId);
                Reader = cmd.ExecuteReader();
                if (Reader.Read() == true)
                {
                    Model = new EmailTemplateModel();
                    Model.TemplateCode = TempCode;
                    Model.Name = Reader["Name"].ToString();
                    Model.Subject = Reader["Subject"].ToString();
                    Model.Body = Reader["Body"].ToString();
                }
            }
            finally
            {
                if (Reader != null)
                    Reader.Close();
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }
            return Model;
        }


        public static TranslatorPaymentQueryModel GetTranslatorPaymentAmount(TranslatorPaymentQueryModel QueryModel)
        {
            SqlCommand cmd;
            SqlParameter ReturnParameter, PaymentParameter, PaymentRateParameter;
            SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CentralConnection"].ConnectionString);

            try
            {
                dbConnection.Open();
                cmd = new SqlCommand("SP_GetTranslatorPayment", dbConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                ReturnParameter = cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
                ReturnParameter.Direction = ParameterDirection.ReturnValue;
                PaymentParameter = cmd.Parameters.Add("@TranslatorPayment", SqlDbType.Decimal);
                PaymentParameter.Precision = 18;
                PaymentParameter.Scale = 2;
                PaymentParameter.Direction = ParameterDirection.Output;
                PaymentRateParameter = cmd.Parameters.Add("@PaymentRate", SqlDbType.Decimal);
                PaymentRateParameter.Precision = 18;
                PaymentRateParameter.Scale = 2;
                PaymentRateParameter.Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@OrderNo", QueryModel.GetOrderNo());
                cmd.Parameters.AddWithValue("@TranslatorID", QueryModel.GetTranslatorID());
                cmd.ExecuteNonQuery();
                QueryModel.ReturnValue = Convert.ToInt32(ReturnParameter.Value.ToString());
                if (QueryModel.ReturnValue == 0)
                {
                    QueryModel.TranslatorPayment = Convert.ToDecimal(PaymentParameter.Value.ToString());
                    QueryModel.PaymentRate = Convert.ToDecimal(PaymentRateParameter.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                QueryModel.ReturnValue = -1;
                Utility.SetErrorLog(null, "SP_GetTranslatorPayment", ex.Message);
                throw ex;
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }
            return QueryModel;
        }


        public static int DirectCurrencyConversion(long SourceCurrency, long DestCurrency, DateTime DomainDate, decimal InputAmount, out decimal ConvertedAmount)
        {
            SqlCommand cmd;
            SqlParameter AmountParam, ReturnParam;
            SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CentralConnection"].ConnectionString);
            int Result = 0;

            try
            {
                dbConnection.Open();
                cmd = new SqlCommand("SP_DirectCurrencyConversion", dbConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SourceCurrency", SourceCurrency);
                cmd.Parameters.AddWithValue("@DestinationCurrency", DestCurrency);
                cmd.Parameters.AddWithValue("@DomainDate", DomainDate);
                cmd.Parameters.AddWithValue("@InputAmount", InputAmount);
                AmountParam = cmd.Parameters.Add("@ConvertedAmount", SqlDbType.Decimal);
                AmountParam.Precision = 18;
                AmountParam.Scale = 2;
                AmountParam.Direction = ParameterDirection.Output;
                ReturnParam = cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
                ReturnParam.Direction = ParameterDirection.ReturnValue;
                cmd.ExecuteNonQuery();
                Result = Convert.ToInt32(ReturnParam.Value.ToString());
                if (Result == 0)
                {
                    ConvertedAmount = Convert.ToDecimal(AmountParam.Value.ToString());
                }
                else
                {
                    ConvertedAmount = 0.0M;
                }
            }
            catch (Exception ex)
            {
                ConvertedAmount = 0.0M;
                throw ex;
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }
            return Result;
        }

        public static OrderWebModel GetOrderDetailsById(OrderFilter filter)
        {
            SqlCommand cmd;
            SqlDataReader dataReader;
            List<OrderWebModel> _orders = new List<OrderWebModel>();
            SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CentralConnection"].ConnectionString);

            try
            {
                dbConnection.Open();
                cmd = new SqlCommand("SP_GetOrderDetailsBy_Id", dbConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CultureId", filter.cultureId);
                cmd.Parameters.AddWithValue("@ApplicationId", filter.ApplicationId);
                cmd.Parameters.AddWithValue("@TranslationType", filter.translationType);
                cmd.Parameters.AddWithValue("@OrderId", filter.orderId);
                cmd.Parameters.AddWithValue("@OrderNo", filter.orderNo);
                cmd.Parameters.AddWithValue("@SourceLangID", filter.srcLangId);
                cmd.Parameters.AddWithValue("@TargetLangID", filter.trgLangId);
                cmd.Parameters.AddWithValue("@SpecialFieldID", filter.specialFieldId);
                cmd.Parameters.AddWithValue("@ClientID", filter.clientId);
                cmd.Parameters.AddWithValue("@TranslatorID", filter.translatorId);
                cmd.Parameters.AddWithValue("@OrderStatus", filter.orderStatus);
                cmd.Parameters.AddWithValue("@StartDate", filter.startDate);
                cmd.Parameters.AddWithValue("@EndDate", filter.endDate);
                dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    OrderWebModel order = new OrderWebModel();
                    order.ID = Guid.Parse(dataReader["ID"].ToString());
                    order.DeliveryPlanID = Convert.ToInt64(dataReader["DeliveryPlanID"].ToString());
                    order.ClientID = Guid.Parse(dataReader["ClientId"].ToString());
                    order.ClientNo = Convert.ToInt64(dataReader["ClientNo"].ToString());

                    order.ApplicationName = dataReader["ApplicationName"].ToString();
                    order.AssignedTranslatorID = dataReader["AssignedTranslatorID"] == DBNull.Value ? (Guid?)null : Guid.Parse(dataReader["AssignedTranslatorID"].ToString());
                    order.CommentToTranslator = dataReader["CommentToTranslator"].ToString();
                    order.DeliveryComment = dataReader["DeliveryComment"].ToString();
                    order.CompanyNotes = dataReader["CompanyNotes"].ToString();
                    order.CommentToBcause = dataReader["CommentToBcause"].ToString();
                    order.CurrencyCode = dataReader["CurrencyCode"].ToString();
                    order.CurrencySymbol = dataReader["CurrencySymbol"].ToString();
                    order.CurrencyName = dataReader["CurrencyName"].ToString();
                    order.CompletionDate = dataReader["CompletionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dataReader["CompletionDate"]);
                    order.CurrencyID = Convert.ToInt64(dataReader["CurrencyID"].ToString());
                    order.DeliveryDate = dataReader["DeliveryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dataReader["DeliveryDate"]);
                    order.DeliveryLevelName = dataReader["DeliveryLevelName"].ToString();
                    order.StartDate = dataReader["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dataReader["StartDate"]);
                    order.EndDate = dataReader["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dataReader["EndDate"]);
                    order.RequestDate = dataReader["RequestDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dataReader["RequestDate"]);
                    order.OrderStatus = dataReader["OrderStatus"] == DBNull.Value ? (int?)null : Convert.ToInt32(dataReader["OrderStatus"]);
                    order.DeliveryPlan = dataReader["DeliveryPlan"].ToString();
                    order.DeliveryTime = dataReader["DeliveryTime"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dataReader["DeliveryTime"].ToString());
                    order.DeliveryType = dataReader["DeliveryType"] == DBNull.Value ? (int?)null : Convert.ToInt32(dataReader["DeliveryType"].ToString());
                    order.InvoiceNo = dataReader["InvoiceNo"].ToString();
                    order.TranslationType = (TranslationType)Convert.ToInt32(dataReader["TranslationType"].ToString());
                    order.TranslationTypeName = order.TranslationType.ToString();
                    order.PaymentAmount = Convert.ToDecimal(dataReader["PaymentAmount"].ToString());
                    order.TranslatorFee = dataReader["TranslatorFee"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dataReader["TranslatorFee"].ToString()); ;
                    order.EstimatedPrice = Convert.ToDecimal(dataReader["EstimatedPrice"].ToString());
                    order.UnitPrice = Convert.ToDecimal(dataReader["UnitPrice"].ToString());
                    order.Discount = Convert.ToDecimal(dataReader["Discount"].ToString());
                    order.PriceAfterDiscount = Convert.ToDecimal(dataReader["PriceAfterDiscount"].ToString());
                    order.ConsumptionTax = Convert.ToDecimal(dataReader["ConsumptionTax"].ToString());
                    order.OrderDate = dataReader["OrderDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dataReader["OrderDate"]);
                    order.OrderId = Convert.ToInt64(dataReader["OrderID"].ToString());
                    order.OrderNo = dataReader["OrderNo"].ToString();
                    order.TranslatorNo = dataReader["TranslatorNo"] == DBNull.Value ? (long?)null : Convert.ToInt64(dataReader["TranslatorNo"].ToString());
                    order.OrderStatusName = dataReader["OrderStatusName"].ToString();
                    order.TranslatorName = dataReader["FirstName"].ToString() + " " + dataReader["MiddleName"].ToString() + " " + dataReader["LastName"].ToString();
                    order.MenuScript = dataReader["MenuScript"].ToString();
                    order.WordCount = Convert.ToInt64(dataReader["WordCount"].ToString());
                    order.CountType = dataReader["CountType"] == DBNull.Value ? (int?)null : Convert.ToInt32(dataReader["CountType"].ToString());
                    order.CharacterCount = Convert.ToInt64(dataReader["CharacterCount"].ToString());
                    order.TargetLanguageID = Guid.Parse(dataReader["TargetLanguageID"].ToString());
                    order.SourceLanguageID = Guid.Parse(dataReader["SourceLanguageID"].ToString());
                    order.TranslationFieldID = Guid.Parse(dataReader["TranslationFieldID"].ToString());
                    order.TargetLanguage = dataReader["TargetLanguage"].ToString();
                    order.SourceLanguage = dataReader["SourceLanguage"].ToString();
                    order.EvaluationScore = Convert.ToDecimal(dataReader["EvaluationScore"].ToString());
                    order.EvaluationComment = dataReader["EvaluationComment"].ToString();
                    order.TranslationFieldName = dataReader["TranslationFieldName"].ToString();
                    order.SubSpecialityFieldID = dataReader["SubSpecialityFieldID"] == DBNull.Value ? (Guid?)null : Guid.Parse(dataReader["SubSpecialityFieldID"].ToString());
                    order.SubSpecialityFieldName = dataReader["SubSpecialityFieldName"].ToString();
                    order.PaymentDate = dataReader["PaymentDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dataReader["PaymentDate"].ToString());
                    _orders.Add(order);
                }
                dataReader.Close();
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }
            return _orders.FirstOrDefault();
        }
    }
}
