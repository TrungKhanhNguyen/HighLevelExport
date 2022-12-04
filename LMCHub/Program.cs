using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMCHub
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IHubProxy _hub;
            string url = @"http://172.10.2.39:8089/";
            var connection = new HubConnection(url);
            _hub = connection.CreateHubProxy("ChatHub");
            connection.Start().Wait();

            _hub.On("CallBackExport", x => {
                Console.WriteLine(x);
            });

            //string line = null;
            //while ((line = System.Console.ReadLine()) != null)
            //{
            //    _hub.Invoke("DetermineLength", line).Wait();
            //}

            Console.Read();
        }
    }
}
