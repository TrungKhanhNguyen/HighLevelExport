using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebHub
{
    public class ChatHub : Hub
    {
        public void Send(string message)
        {
            Clients.All.CallBackExport(message);
        }
    }
}