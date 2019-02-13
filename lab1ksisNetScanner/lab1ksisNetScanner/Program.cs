using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

namespace lab1ksisNetScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            string hostname = Dns.GetHostName();
            IPHostEntry host = Dns.GetHostEntry(hostname);
            NetworkInterface[] niList = NetworkInterface.GetAllNetworkInterfaces();
            
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
            for (int i = host.AddressList.Count()/2;i< host.AddressList.Count();i++)
            {
                string subnet = host.AddressList[i].ToString();
                subnet = subnet.Remove(subnet.LastIndexOf('.') + 1);
                string strIp;

                //skipping broadcast and reserved 0 255
                for (byte j = 1; j < 255; j++)
                {
                    strIp = (subnet + j.ToString());
                    ip = IPAddress.Parse(strIp);

                    
                    PingOptions o = new PingOptions();
                    o.Ttl = 30;
                    byte[] buf = new byte[32];
                    reply = ping.Send(ip, 1, buf, o);
                    //reply = ping.SendPingAsync(ip, 30);
                    //ping
                    
                    if (reply.Status == IPStatus.Success)
                    {
                        try
                        {
                            host = Dns.GetHostEntry(ip);

                            Console.WriteLine($"Name {host.HostName}  Adr {ip} Mac {(char)65}");
                        }
                        catch { Console.WriteLine("Couldnt retrieve hostname for " + strIp); }
                    }
                    //adrBytes[3] = j;
                }
            }

            Console.ReadKey();
        }
    }
}
