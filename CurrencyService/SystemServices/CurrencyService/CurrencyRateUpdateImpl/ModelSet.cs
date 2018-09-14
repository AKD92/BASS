
namespace BASS.SystemServices.CurrencyService.CurrencyRateUpdateImpl
{
    public class CurrencyModel
    {
        public bool success { get; set; }
        public CurrencyErrorModel error { get; set; }
        public string terms { get; set; }
        public string privacy { get; set; }
        public long timestamp { get; set; }
        public string source { get; set; }
        public CurrencyRateModel quotes { get; set; }
    }

    public class CurrencyRateModel
    {
        public decimal USDEUR { get; set; }
        public decimal USDJPY { get; set; }
        public decimal USDKRW { get; set; }
        public decimal USDTHB { get; set; }
        public decimal USDAUD { get; set; }
        public decimal USDCNY { get; set; }
        public decimal USDBDT { get; set; }
    }

    public class CurrencyErrorModel
    {
        public int code { get; set; }
        public string info { get; set; }
    }
}
