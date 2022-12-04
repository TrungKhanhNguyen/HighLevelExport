using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallBackListener
{
    public class SignalRMasterClient
    {
        public string Url { get; set; }
        public HubConnection Connection { get; set; }
        public IHubProxy Hub { get; set; }

        public SignalRMasterClient(string url)
        {
            Url = url;
            Connection = new HubConnection(url, useDefaultUrl: false);
            Hub = Connection.CreateHubProxy("ServiceStatusHub");
            Connection.Start().Wait();

            Hub.On<string>("acknowledgeMessage", (message) =>
            {
                Console.WriteLine("Message received: " + message);

                /// TODO: Check status of the LDAP
                /// and update status to Web API.
            });
        }

        public void SayHello(string message)
        {
            Hub.Invoke("GetStatus", message);
            //Console.WriteLine("hello method is called!");
        }

        public void Stop()
        {
            Connection.Stop();
        }

    }
}
