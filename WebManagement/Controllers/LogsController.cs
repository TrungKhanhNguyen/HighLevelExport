using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebManagement.Controllers
{
    public class LogsController : Controller
    {
        // GET: Logs
        private SQLServerHelper sqlserverHelper = new SQLServerHelper();
        public ActionResult Index()
        {
            //var listNumber = sqlserverHelper.GetAllHotNumber();

            ////var listCase = mainHelper.GetListCaseObject();
            //var listCase = new List<CaseObject>();
            //listCase.Add(new CaseObject { id = "1", name = "C02" });
            //listCase.Add(new CaseObject { id = "2", name = "C03" });
            //listCase.Add(new CaseObject { id = "3", name = "C04" });
            //var listIntercept = new List<ExportObject>();
            ////if(listCase.Count() > 0)
            ////{
            ////    var casename = listCase[0].name;
            ////    var tempTarget = new ExportTarget { TargetName = casename };
            ////    listIntercept = mainHelper.GetListInterceptName(tempTarget);
            ////}
            //listIntercept.Add(new ExportObject { InterceptId = "123", InterceptName = "Intercept 1" });
            //listIntercept.Add(new ExportObject { InterceptId = "658", InterceptName = "Apple Inc" });
            //listIntercept.Add(new ExportObject { InterceptId = "987", InterceptName = "Microsoft JSC" });

            //ViewBag.AllCase = new SelectList(listCase, "id", "name");

            //ViewBag.AllIntercept = new SelectList(listIntercept, "InterceptId", "InterceptName");
            var listLogs = new List<ExportLog>();
            ViewBag.BeginDate = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
            ViewBag.BeginTime = "07:00";
            ViewBag.EndDate = DateTime.Now.ToString("dd-MM-yyyy");
            ViewBag.EndTime = "07:00";

            return View(listLogs);
            //return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            var beginDate = form["beginDate"].ToString();
            var beginTime = form["beginTime"].ToString();
            var endDate = form["endDate"].ToString();
            var endTime = form["endTime"].ToString();
            var tempbegin = beginDate + " " + beginTime;
            var tempend = endDate + " " + endTime;
            var listLogs = sqlserverHelper.GetLogByTime(tempbegin, tempend);

            ViewBag.BeginDate = beginDate;
            ViewBag.BeginTime = beginTime;
            ViewBag.EndDate = endDate;
            ViewBag.EndTime = endTime;

            return View(listLogs);
        }
    }
}