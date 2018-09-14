
using System;

namespace BASS.SystemServices.EmailAlertService.OfferEmailImpl
{
    public enum TranslationType
    {
        Online = 1,
        Appointed = 2,
        NativeCheck = 3,
        TranslatorCoordinator = 4
    }


    public class OfferEmailModel
    {
        public Guid? OrderID { get; set; }
        public string OrderNo { get; set; }
        public Guid? TranslatorID { get; set; }
        public string EmailTo { get; set; }
        public int TranslationType { get; set; }
        public string DeliveryLevelName { get; set; }
        public Guid? OfferLogID { get; set; }
        public string LogCommand { get; set; }
    }

    public class EmailTemplateModel
    {
        public string Name { get; set; }
        public string TemplateCode { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class OrderWebModel
    {
        public Guid ID { get; set; }
        public string ApplicationName { get; set; }
        public long OrderId { get; set; }
        public string OrderNo { get; set; }
        public string InvoiceNo { get; set; }
        public Guid SourceLanguageID { get; set; }
        public Guid TargetLanguageID { get; set; }
        public Guid TranslationFieldID { get; set; }
        public Guid? SubSpecialityFieldID { get; set; }
        public Guid ClientID { get; set; }
        public int TranslationLevelType { get; set; }
        public long ClientNo { get; set; }
        public string ClientName { get; set; }
        public string EmployeeName { get; set; }
        public string AffiliateCompanyName { get; set; }
        public Guid? AssignedTranslatorID { get; set; }
        public long DeliveryPlanID { get; set; }
        public string DeliveryPlan { get; set; }
        public int? DeliveryType { get; set; }
        public decimal? DeliveryTime { get; set; }
        public string DeliveryLevelName { get; set; }
        public long? CurrencyID { get; set; }
        public Guid IntroducerID { get; set; }

        public Guid? BlackListedStaffID { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyName { get; set; }
        public TranslationType TranslationType { get; set; }
        public string TranslationTypeName { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int? OrderStatus { get; set; }
        public string PaymentStatusName { get; set; }
        public string CurrencyCode { get; set; }
        public int? PaymentMethod { get; set; }
        public string PaymentWay { get; set; }
        //public int? PaymentMethodID { get; set; }
        public long WordCount { get; set; }
        public int? CountType { get; set; }
        public long CharacterCount { get; set; }
        public Nullable<decimal> PaymentAmount { get; set; }
        public Nullable<decimal> TranslatorFee { get; set; }
        public decimal EstimatedPrice { get; set; }
        public int? EstimationType { get; set; }
        public string EstimationTypeName { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> PriceAfterDiscount { get; set; }

        public Nullable<decimal> DiscountedPrice { get; set; }

        public Nullable<decimal> EstimatedPriceAfterTax { get; set; }

        public Nullable<decimal> ConsumptionTax { get; set; }
        public Nullable<decimal> EvaluationScore { get; set; }
        public string EvaluationComment { get; set; }
        public long CountFavouriteTranslator { get; set; }
        public long CountBlackTranslator { get; set; }
        //public Nullable<decimal> StaffCharge { get; set; }
        public Nullable<DateTime> RequestDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string CommentToTranslator { get; set; }
        public string DeliveryComment { get; set; }
        public string CompanyNotes { get; set; }
        public string CommentToBcause { get; set; }
        public string ReferenceFileName { get; set; }
        public string ReferenceOriginalFileName { get; set; }
        public string ReferenceDownloadURL { get; set; }
        public long? ReferenceFileSize { get; set; }
        public string MenuScript { get; set; }
        public string TranslatorName { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string OrderStatusName { get; set; }
        public string TranslationFieldName { get; set; }
        public string SubSpecialityFieldName { get; set; }
        public long? TranslatorNo { get; set; }
        public int? CompanyTypeID { get; set; }
        public string CompanyType { get; set; }
        public string WebSiteURL { get; set; }
        public string MobileNo { get; set; }
        public string TelephoneNo { get; set; }
        public string IntroducedBy { get; set; }
        public long? RegistrationID { get; set; }
        public long? MyIdentityNo { get; set; }
        public string Period { get; set; }
        public int? Task { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public int? year { get; set; }
        public int? monthNumber { get; set; }
        public string Contents { get; set; }
        public string DeliveryDateString { get; set; }
        public string OrderDateString { get; set; }
        public string AffiliateCode { get; set; }
        //Translator Evaluation By Customer
        public int? Q1Score { get; set; }
        public int? Q2Score { get; set; }
        public int? Q3Score { get; set; }
        public int? Q4Score { get; set; }
        public decimal? AvgScore { get; set; }
        public string CustomerComment { get; set; }
        //Service Evaluation By Customer
        public string CompanyFeedbackByCustomer { get; set; }
        public int? ServiceEvaluationScoreByCustomer { get; set; }
        //Penalty information for Translator
        public int NumberOfWorks { get; set; }
        public long CustomerEvaluationPoint { get; set; }
        public long PenaltyPoint { get; set; }
        public DateTime? BannedPeriodFrom { get; set; }
        public DateTime? BannedPeriodTo { get; set; }
        public DateTime? ComplainDate { get; set; }
        public string ClaimDetails { get; set; }
        public string DetailsForAction { get; set; }
        public Guid? ClaimSolvedBy { get; set; }
        public Guid? ClaimRecievedBy { get; set; }
        public bool? IsSolved { get; set; }
        public bool? IsClaimed { get; set; }
        public string ClaimStatus { get; set; }
        public DateTime? ClaimSolvingDate { get; set; }
        public string Timetillsettlement { get; set; }
        public string ClaimReceiverName { get; set; }
        public string ClaimSolverName { get; set; }
        public decimal? DeductedPrice { get; set; }
        public bool? IsPostPay { get; set; }
        //For Staff Information.
        public long? StaffNumber { get; set; }
        public long? NoOfDaysLastQuotationAsCompany { get; set; }
        public long? TotalQuotationAsCompany { get; set; }
        public long? RatioOfOrderAsCompany { get; set; }
        public long? AverageQuationPriceAsCompany { get; set; }
        public long? AverageOrderedPriceAsCompany { get; set; }
        public int? StaffNoOfWorks { get; set; }
        public long? StaffPenaltyPoint { get; set; }
        public decimal? StaffEvaluationPointByCustomer { get; set; }
        public DateTime? StaffBannedPeriodFrom { get; set; }
        public DateTime? StaffBannedPeriodTo { get; set; }
        public long? StaffNegetivePoint { get; set; }
        public Guid? StaffID { get; set; }
        public long? StaffPenaltyNumber { get; set; }
        public bool? IsGivenPenalty { get; set; }

    }

    public class OrderFilter
    {
        public string cultureId { get; set; }
        public Guid? partnerId { get; set; }
        public string affiliatename { get; set; }
        public long ApplicationId { get; set; }
        public string orderNo { get; set; }
        public long? clientNo { get; set; }
        public string staffname { get; set; }
        public string clientname { get; set; }
        public long? orderId { get; set; }
        public int? translationType { get; set; }
        public Guid? srcLangId { get; set; }
        public Guid? trgLangId { get; set; }
        public Guid? specialFieldId { get; set; }
        public Guid? clientId { get; set; }
        public Guid? PartnerId { get; set; }
        public string ClientName { get; set; }
        public Guid? translatorId { get; set; }
        public int? orderStatus { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public DateTime firstDateMonth { get; set; }
        public DateTime lastDateMonth { get; set; }
        public int ItemStatusID { get; set; }
        public bool? IsComplaint { get; set; }
        public bool? IsArranging { get; set; }
        public bool? IsProgress { get; set; }
        public bool? IsWaitingForStatus { get; set; }
        public bool? IsEvaluated { get; set; }
        public bool? IsLight { get; set; }
        public bool? IsExpert { get; set; }
        public bool? IsBusiness { get; set; }
        public long? IntroducerNo { get; set; }
        public long? CustomerNo { get; set; }
        public long? TranslatorNo { get; set; }
        public string EstimationNo { get; set; }
        public long? CompanyTypeID { get; set; }
        public string CurrentCulture { get; set; }
        public long? CurrentUserID { get; set; }

        public long? staffnumber { get; set; }
        public string claimDetails { get; set; }

        public string detailsForAction { get; set; }

        public string webOrderTitle { get; set; }

        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public int? orderMinStatus { get; set; }
    }

    public class TranslatorPaymentQueryModel
    {
        public string OrderNo { get; set; }
        public Guid TranslatorID { get; set; }
        public int ReturnValue { get; set; }
        public decimal TranslatorPayment { get; set; }
        public decimal PaymentRate { get; set; }
        public object GetOrderNo()
        {
            if (string.IsNullOrEmpty(OrderNo) == true)
                return DBNull.Value;
            else
                return OrderNo;
        }
        public object GetTranslatorID()
        {
            if (TranslatorID == Guid.Empty)
                return DBNull.Value;
            else
                return TranslatorID;
        }
    }
}
