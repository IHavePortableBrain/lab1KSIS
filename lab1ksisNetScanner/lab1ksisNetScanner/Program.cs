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
            PingReply reply;
            IPAddress ip;
            //scanning each subnet
            for (int i = host.AddressList.Count() / 2; i < host.AddressList.Count(); i++)
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
        const bool resolveNames = true;

        static void PingNet(string ipBase)
        {
            upCount = 0;
            countdown = new CountdownEvent(1);
            Stopwatch sw = new Stopwatch();

            //start measuaring elapsed time for that subnet
            sw.Start();
            Console.WriteLine("Scanning " + ipBase + " : ");
            //string ipBase = "10.22.4.";
            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i.ToString();

                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                countdown.AddCount();
                p.SendAsync(ip, 100, ip);
            }
            countdown.Signal();
            countdown.Wait();
            sw.Stop();
            TimeSpan span = new TimeSpan(sw.ElapsedTicks);
            Console.WriteLine("Took {0} milliseconds. {1} hosts active.", sw.ElapsedMilliseconds, upCount);
            //Console.ReadLine();
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

        static string arp(string ip)
        {
            return null;
        }
    }
}
