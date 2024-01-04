using System.Configuration;

namespace GraphDeliver
{
    public static class AppSettings
    {
        public static string HostAddressA => ConfigurationManager.AppSettings["host_a"] ?? "127.0.0.1:1000";
        public static string HostAddressB => ConfigurationManager.AppSettings["host_b"] ?? "127.0.0.1:1001";
        public static bool IsAutoRun => ConfigurationManager.AppSettings["autorun"]?.ToUpper() == "TRUE";
        public static int WaitTime => int.TryParse(ConfigurationManager.AppSettings["waittime"], out int wait) ? wait : 10000;
        public static int RetryCount => int.TryParse(ConfigurationManager.AppSettings["retrycount"], out int retry) ? retry : 5;
    }
}