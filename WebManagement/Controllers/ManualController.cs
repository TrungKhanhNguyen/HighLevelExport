using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebManagement.Controllers
{
    public class ManualController : Controller
    {
        private DBHelper helper = new DBHelper();
        private MainHelper mainHelper = new MainHelper();
        private SQLServerHelper sqlserverHelper = new SQLServerHelper();
        private Utility utility = new Utility();
        // GET: Manual
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                //var sds = System.Web.HttpContext.Current.User.Identity.Name;
                //var listNumber = sqlserverHelper.GetAllHotNumber();

                //var listCase = mainHelper.GetListCaseObject();
                var listCase = new List<CaseObject>();
                listCase.Add(new CaseObject { id = "1", name = "C02" });
                listCase.Add(new CaseObject { id = "2", name = "C03" });
                listCase.Add(new CaseObject { id = "3", name = "C04" });
                var listIntercept = new List<ExportObject>();
                listIntercept.Add(new ExportObject { InterceptId = "123", InterceptName = "Intercept 1" });
                listIntercept.Add(new ExportObject { InterceptId = "658", InterceptName = "Apple Inc" });
                listIntercept.Add(new ExportObject { InterceptId = "987", InterceptName = "Microsoft JSC" });

                ViewBag.AllCase = new SelectList(listCase, "id", "name");
                ViewBag.AllIntercept = new SelectList(listIntercept, "InterceptId", "InterceptName");

                ViewBag.BeginDate = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
                ViewBag.EndDate = DateTime.Now.ToString("dd-MM-yyyy");

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        public ActionResult ExportByTime(string casename, string interceptname, string interceptid, string beginValue, string endValue)
        {
            DateTime beginDate = DateTime.ParseExact(beginValue, "dd-MM-yyyy HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(endValue, "dd-MM-yyyy HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
            var tempTarget = new ExportObject { InterceptId = interceptid, InterceptName = interceptname, CaseName = casename };
            var signalString = casename + ";" + interceptid + ";" + interceptname + ";" + beginValue + ";" + endValue;

            IHubProxy hub;
            var url = StaticKey.SIGNALR_LOCAL_IP;
            var Connection = new HubConnection(url, useDefaultUrl: false);
            hub = Connection.CreateHubProxy("ServiceStatusHub");
            Connection.Start().Wait();

            hub.Invoke("GetStatus", signalString).Wait();
            return View();
        }
    }
}