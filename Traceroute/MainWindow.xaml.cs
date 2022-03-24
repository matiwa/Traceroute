using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Traceroute
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            outlist.Items.Clear();
            string hostname = addresslist.Text;
            int timeout = 1000;
            int max_ttl = 30;
            int current_ttl = 0;
            const int bufferSize = 32;
            Stopwatch s1 = new Stopwatch();
            Stopwatch s2 = new Stopwatch();
            byte[] buffer = new byte[bufferSize];
            new Random().NextBytes(buffer);
            Ping pinger = new Ping();
            Task.Factory.StartNew(() =>
            {
                WriteListBox($"Started ICMP Trace route on {hostname}");
                for(int ttl = 1; ttl <= max_ttl; ttl++)
                {
                    current_ttl++;
                    s1.Start();
                    s2.Start();
                    PingOptions options = new PingOptions(ttl, true);
                    PingReply reply = null;
                    try
                    {
                        reply = pinger.Send(hostname, timeout, buffer, options);
                    }
                    catch
                    {
                        WriteListBox("Error");
                        break;
                    }
                    if (reply != null)
                    {
                        if (reply.Status == IPStatus.TtlExpired)
                        {
                            WriteListBox($"[{ttl}] - Route: {reply.Address} - Time: {s1.ElapsedMilliseconds} ms - " +
                                $"Total Time: {s2.ElapsedMilliseconds} ms");
                            continue;
                        }
                        if (reply.Status == IPStatus.TimedOut)
                        {
                            WriteListBox($"Timeout on {hostname}. Continuing.");
                            continue;
                        }
                        if (reply.Status == IPStatus.Success)
                        {
                            WriteListBox($"Successful Traceroute to {hostname} in {s1.ElapsedMilliseconds} ms - " +
                                $"Total Time: {s2.ElapsedMilliseconds} ms");
                            s1.Stop();
                            s2.Stop();
                        }
                    }
                }
            });
        }

        private void WriteListBox(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                outlist.Items.Add(text);
            }));
        }

        private void BExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
