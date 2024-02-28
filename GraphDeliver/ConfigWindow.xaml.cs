using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GraphDeliver
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        /// <summary>
        /// 启动运行注册表位置
        /// </summary>
        private static readonly RegistryKey regPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        public int PortID { get; set; } = 1;
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public int StopBits { get; set; } = 1;
        public string Parity { get; set; } = "NONE";
        public int SendInterval { get; set; } = 1000;
        public int IdleCount { get; set; } = 5;
        public string HostAddressA { get; set; } = "127.0.0.1:1000";
        public string HostAddressB { get; set; } = "127.0.0.1:1001";
        public bool IsAutoRun { get; set; }
        public bool IsAutoStart
        {
            get
            {
                return regPath.GetValue(Process.GetCurrentProcess().ProcessName) != null;
            }
            set
            {
                if (value)
                {
                    regPath.SetValue(Process.GetCurrentProcess().ProcessName, Process.GetCurrentProcess().MainModule.FileName);
                }
                else
                {
                    regPath.DeleteValue(Process.GetCurrentProcess().ProcessName, false);
                }
            }
        }
        public ConfigWindow(Window window)
        {
            Owner = window;

            DataContext = this;

            try
            {
                PortID = int.Parse(ConfigurationManager.AppSettings["com"]);
                BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]);
                DataBits = int.Parse(ConfigurationManager.AppSettings["databits"]);
                StopBits = int.Parse(ConfigurationManager.AppSettings["stopbits"]);
                Parity = ConfigurationManager.AppSettings["parity"].ToUpper();
                SendInterval = int.Parse(ConfigurationManager.AppSettings["interval"]);
                IdleCount = int.Parse(ConfigurationManager.AppSettings["idlecount"]);
                HostAddressA = ConfigurationManager.AppSettings["host_a"];
                HostAddressB = ConfigurationManager.AppSettings["host_b"];

                IsAutoRun = ConfigurationManager.AppSettings["autorun"].ToUpper() == "TRUE";
            }
            catch
            {

            }

            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);

            SaveAllConfig();
        }
        private void SaveAllConfig()
        {
            SaveConfig(new Dictionary<string, object>()
            {
                { "com", PortID },
                { "baudrate", BaudRate },
                { "databits", DataBits },
                { "stopbits", StopBits },
                { "parity", Parity },
                { "interval", SendInterval },
                { "idlecount", IdleCount },
                { "host_a", HostAddressA },
                { "host_b", HostAddressB },
                { "autorun", IsAutoRun },
            });
        }

        private void SaveConfig(Dictionary<string, object> dict)
        {
            try
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                foreach (var pair in dict)
                {
                    object param = pair.Value;
                    string key = pair.Key;
                    if (param != null)
                    {
                        if (configuration.AppSettings.Settings.AllKeys.Contains(key))
                        {
                            configuration.AppSettings.Settings[key].Value = param.ToString();
                        }
                        else
                        {

                            configuration.AppSettings.Settings.Add(key, param.ToString());
                        }
                    }
                    else
                    {
                        configuration.AppSettings.Settings.Remove(key);
                    }
                }
                configuration.Save(ConfigurationSaveMode.Minimal, true);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch
            {

            }
        }
    }
}