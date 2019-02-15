using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace lab1ksisNetScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            string hostname = Dns.GetHostName();
            IPHostEntry host = Dns.GetHostEntry(hostname);
            NetworkInterface[] niList = NetworkInterface.GetAllNetworkInterfaces();

            //printing device info
            Console.WriteLine($"{hostname}: ");
            foreach (NetworkInterface ni in niList)
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    Console.WriteLine($"     Name {ni.Name} |" +
                                      $"     Mac {ni.GetPhysicalAddress()}");
                }
            }


            Ping ping = new Ping();
            //scanning each subnet
            int hostcount = host.AddressList.Count();
            for (int i = hostcount / 2; i < hostcount; i++)
            {
                string subnet = host.AddressList[i].ToString();
                subnet = subnet.Remove(subnet.LastIndexOf('.') + 1);

                PingNet(subnet);
            }
            Console.ReadKey();
        }

        static CountdownEvent countdown;
        static int upCount;
        static object lockObj = new object();

        static void PingNet(string ipBase)
        {
            upCount = 0;
            countdown = new CountdownEvent(1);
            Stopwatch sw = new Stopwatch();

            //start measuaring elapsed time for that subnet
            sw.Start();

            Console.WriteLine("Scanning " + ipBase + " : ");
            for (int i = 1; i < 255; i++)
            {
                string ipStr = ipBase + i.ToString();
                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                countdown.AddCount();
                p.SendAsync(ipStr, 1000, ipStr);
            }
            countdown.Signal();
            countdown.Wait();
            sw.Stop();
            //TimeSpan span = new TimeSpan(sw.ElapsedTicks);
            Console.WriteLine("Took {0} milliseconds. {1} hosts active.", sw.ElapsedMilliseconds, upCount);
        }

        static void p_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string name;
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                    name = hostEntry.HostName;
                }
                catch (SocketException ex)
                {
                    name = "?";
                }
                Console.WriteLine("     {0} ({1}) {2} is up: ({3} ms)", ip, name, arp(ip),e.Reply.RoundtripTime);
                lock (lockObj)
                {
                    upCount++;
                }
            }
            else if (e.Reply == null)
            {
                Console.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);
            }
            countdown.Signal();
        }

        public static string arp(string ipAddress)
        {
            string macAddress = string.Empty;
            Process pProcess = new Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(substrings[3].Length - 2)
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }
            else
            {
                return "";
            }
        }
    }
}
