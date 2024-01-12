using System.Configuration;

namespace GraphDeliver
{
    public static class AppSettings
    {
        public static string HostAddressA => ConfigurationManager.AppSettings["host_a"] ?? "127.0.0.1:1000";
        public static string HostAddressB => ConfigurationManager.AppSettings["host_b"] ?? "127.0.0.1:1001";
        public static string ComPort => $"COM{ConfigurationManager.AppSettings["comport"] ?? "1"}";
        public static int BaudRate => int.TryParse(ConfigurationManager.AppSettings["baudrate"], out int baud) ? baud : 9600;
        public static int DataBits => int.TryParse(ConfigurationManager.AppSettings["databits"], out int data) ? data : 8;
        public static int StopBits => int.TryParse(ConfigurationManager.AppSettings["stopbits"], out int stop) ? stop : 1;
        public static string Parity => ConfigurationManager.AppSettings["parity"] ?? "NONE";
        public static int SendInterval => int.TryParse(ConfigurationManager.AppSettings["interval"], out int interval) ? interval : 1;
        public static bool IsAutoRun => ConfigurationManager.AppSettings["autorun"]?.ToUpper() == "TRUE";
        public static int WaitTime => int.TryParse(ConfigurationManager.AppSettings["waittime"], out int wait) ? wait : 10000;
        public static int RetryCount => int.TryParse(ConfigurationManager.AppSettings["retrycount"], out int retry) ? retry : 5;
    }
}