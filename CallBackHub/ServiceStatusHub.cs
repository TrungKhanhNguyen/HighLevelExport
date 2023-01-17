using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallBackHub
{
    [HubName("ServiceStatusHub")]
    public class ServiceStatusHub : Hub
    {
        private static IHubContext hubContext =
        GlobalHost.ConnectionManager.GetHubContext<ServiceStatusHub>();

        public void GetStatus(string message)
        {
            hubContext.Clients.All.acknowledgeMessage(message);
        }

        public void GetLogStatus(string message)
        {
            hubContext.Clients.All.logMessage(message);
        }
    }
}