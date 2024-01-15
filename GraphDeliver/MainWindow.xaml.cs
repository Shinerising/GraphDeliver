using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GraphDeliver
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Status status;

        public MainWindow()
        {
            status = new Status();

            DataContext = status;

            InitializeComponent();

            CheckAutoRun();
        }

        private void CheckAutoRun()
        {
            if (AppSettings.IsAutoRun)
            {
                status.DelayOpen(1000, AppSettings.WaitTime, AppSettings.RetryCount);
            }
        }

        #region ShowLog
        private void ShowSpecialLog()
        {
            string data = "nZW7btswFIb3PgU1aaGpZqXsPEHmLh20F548dJELMFURJE6MBIWDBr0EBWqgQ5aiCNAOlgLkUUQEHfkK/UmKust2S0gWRR6e75yfh7IUQv7jRSJKGRvl4up1Lq7xGI1UuiaUqPS3ypbP9vKhb0omWKg2l9y1kPvmGZUj4zGX4kKlQiapFMc7nBN3T6VYPZ3ccZlkCJBXTSY35qlDNq8ZHBMzpDabvWLHpV23G1x1xl6o7CzA02vYpftJhIVBsUZl77TE9mWOQKMmJV1HkedBn2EM6bs1wsUWHkVwrHtT8nh/OLECmXeVvvdnKjs5qOexbyokNnnM3SKVfaQThAoYIhYqO/cjmx8mn359eaUNVLboEXgPWn763eP4cawFXJ1Lcd0OaoqtWarsVmUfHu8x4If/wdP6cVMLIbyhT0DW4z6JcEqc8MmdPL6sFnkd0laU26ii6W7oYUEcEbW5JXrvC9u3a5TbkTsrxfIe2gCurArqk8rW6uI1yobO8tUNzhUu4mPg0BzjBydtD3F7OQYdHvcMx3xlyJjK5CsqRg/ha2H2ywqbDvJqwE7V01z8bJtrnh6nAASacIFBSBmbQJBfFfkbsg+1A+3bCM5jJA8k4wGMsLVM2zL+0h6NBXl+gM50uHJq0G6elX2zuqdBAAY3TFjNZLLJV1eAItHPjrWdOSjwQJ585kPE2HSDoqIZUQ/f0Gf6oC6Nxktd1CikZal4v85dsSvVhyIIsJ0M+p6VfH0xEzVjhcUnhpjIn9WP+a78h/h11f0anVhEccJ0n8EB9r6YMLky5nSMt50m2xhr4+u50zY7cDYoc2LXVxZ6Ym6DslbzHfw6vPmJqjrASpEUNgEJbW3X4mptsc1+F5mzRrWXuErtQB/i+jxjrpxD/V8mxSkpyzxJnVu6C83+Ag==";
            byte[] buffer = Convert.FromBase64String(data);
            string text = "";

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (MemoryStream inputStream = new MemoryStream(buffer))
                {
                    using (DeflateStream deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        try
                        {
                            deflateStream.CopyTo(outputStream);
                            text = Encoding.UTF8.GetString(outputStream.ToArray());
                        }
                        catch
                        {
                            return;
                        }
                    }
                }
            }

            foreach (string log in text.Split('\n'))
            {
                status.AddLog(log);
            }
        }
        #endregion

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = true;

            if (status.IsPortOpen)
            {
                status.ClosePort();
            }
            else
            {
                status.OpenPort();
            }

            Task.Factory.StartNew(() => {
                Thread.Sleep(3000);
                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    button.IsEnabled = true;
                }));
            });
        }

        private void Button_Option_Click(object sender, RoutedEventArgs e)
        {
            new ConfigWindow(this).ShowDialog();
        }
        private void Button_ClearRecord_Click(object sender, RoutedEventArgs e)
        {
            status.ClearRecord();
        }
        private void Button_ClearLog_Click(object sender, RoutedEventArgs e)
        {
            status.ClearLog();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            string password = AppSettings.ExitPassword;
            bool? result = PasswordWindow.Show(this, "请输入退出密码", password, "密码确认", false);
            if (result != true)
            {
                e.Cancel = true;
                return;
            }

            status.Dispose();
        }


        private void OpenHelpFile()
        {
            string helpFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.html");

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(helpFile);
                    process.Start();
                }
            }
            catch
            {

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                return;
            }
            if (e.Key == Key.F1)
            {
                OpenHelpFile();
            }
            else if (Keyboard.IsKeyDown(Key.M) && Keyboard.IsKeyDown(Key.I) && Keyboard.IsKeyDown(Key.K) && Keyboard.IsKeyDown(Key.U))
            {
                ShowSpecialLog();
            }
        }
        private void Label_Com_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string command = "/K mode COM" + AppSettings.ComPort;
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo("cmd.exe", command) { WorkingDirectory = "/" };
                    process.Start();
                }
            }
            catch
            {

            }
        }
    }
}