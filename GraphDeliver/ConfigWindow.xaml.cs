using Microsoft.Win32;
using System;
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
        private static readonly bool IsWindowsXP = Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 1;
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
                try
                {
                    RegistryKey regPath = IsWindowsXP ? Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", true) : Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (IsWindowsXP)
                    {
                        return regPath.GetValue("Shell") != null && regPath.GetValue("Shell").ToString().Contains(Process.GetCurrentProcess().MainModule.FileName);
                    }
                    return regPath.GetValue(Process.GetCurrentProcess().ProcessName) != null;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            set
            {
                try
                {
                    RegistryKey regPath = IsWindowsXP ? Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", true) : Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (IsWindowsXP)
                    {
                        string text = regPath.GetValue("Shell")?.ToString();
                        if (value)
                        {
                            regPath.SetValue("Shell", text == null ? Process.GetCurrentProcess().MainModule.FileName : string.Join(",", text, Process.GetCurrentProcess().MainModule.FileName));
                        }
                        else
                        {
                            if (text == null)
                            {
                                return;
                            }
                            var frags = text.Split(',').Where(item => !item.Contains(Process.GetCurrentProcess().MainModule.FileName)).ToList();
                            regPath.SetValue("Shell", string.Join(",", frags));
                        }
                    }
                    else
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
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                foreach (var item in dict)
                {
                    if (config.AppSettings.Settings[item.Key] == null)
                    {
                        config.AppSettings.Settings.Add(item.Key, item.Value.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings[item.Key].Value = item.Value.ToString();
                    }
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch
            {

            }
        }
    }
}