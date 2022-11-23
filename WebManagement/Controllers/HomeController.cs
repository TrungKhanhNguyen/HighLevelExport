﻿
using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebManagement.Controllers
{
    public class HomeController : Controller
    {
        private DBHelper helper = new DBHelper();
        private SQLServerHelper sqlserverHelper = new SQLServerHelper();
        private MainHelper mainHelper = new MainHelper();
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                var listTarget = sqlserverHelper.GetListExportTarget();
                //List<CaseObject> test = new List<CaseObject>();
                var listCase = mainHelper.GetListCaseObject();
                //test.Add(new CaseObject { id = "1", name = "C02" });
                //test.Add(new CaseObject { id = "2", name = "C03" });
                //test.Add(new CaseObject { id = "3", name = "C04" });
                ViewBag.AllCase = new SelectList(listCase, "id", "name");

                return View(listTarget);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
                
        }
        [HttpPost]
        public ActionResult AddTarget(ExportTarget target)
        {
            try
            {
                //var tempTarget = new ExportTarget { TargetId = id, TargetName = name };
                sqlserverHelper.AddTarget(target);
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("Failed!", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Delete(string targetId)
        {
                var response = sqlserverHelper.DeleteTarget(targetId);
                return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}