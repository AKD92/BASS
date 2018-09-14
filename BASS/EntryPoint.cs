
using System.ServiceProcess;
using BASS.SystemServices.CurrencyService;
using BASS.SystemServices.EmailAlertService;

namespace BASS
{
    public static class EntryPoint
    {
        public static void Main()
        {
            ServiceBase svc1 = new CurrencyRateUpdateService("BASSCurrencyService");
            ServiceBase svc2 = new OfferEmailService("BASSOfferEmailService");
            ServiceBase[] Services = new ServiceBase[] { svc1, svc2 };
            ServiceBase.Run(Services);
        }
    }
}
