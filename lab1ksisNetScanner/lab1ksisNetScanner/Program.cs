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
            int niIndex = 0;
            Console.WriteLine($"{hostname}: ");


            int hi = host.AddressList.Count() - 1;
            for (int i = 0; i <= hi / 2; i++)
            {
                Console.Write($"    Ipv4 {host.AddressList[i + hi / 2 + 1]} " +
                    $"Ipv6 {host.AddressList[i]} ");

                while(niIndex <= hi) {
                    if (niList[niIndex++].OperationalStatus == OperationalStatus.Up)
                    {
                        Console.WriteLine($"Name {niList[niIndex].Name} Mac {niList[niIndex].GetPhysicalAddress()}");
                        break;
                    }
                }
           
            }

            Console.ReadKey();
        }
    }
}
