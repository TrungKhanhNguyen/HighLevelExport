using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMCHub
{
    [HubName("TestHub")]
    public class TestHub : Hub
    {
        public void CallBackExport(string message)
        {
            Console.WriteLine("[INFO]Received callback export: " + message);

            //string newMessage = string.Format(@"{0} has a length of: {1}", message, message.Length);
            Clients.All.ReceiveCallBack(message);
        }
    }
}
