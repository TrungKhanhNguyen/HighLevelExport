﻿using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace WebManagement.Controllers
{
    public class PerTwoMinutesController : Controller
    {
        // GET: PerTwoMinutes
        private SQLServerHelper sqlserverHelper = new SQLServerHelper();
        private MainHelper mainHelper = new MainHelper();
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                //sqlserverHelper.InsertLogToDB("Error export 1", DateTime.Now.AddHours(-5), "C02", ErrorType.HourError.ToString(), "4945", "84658974565");
                //sqlserverHelper.InsertLogToDB("Error export 2", DateTime.Now.AddHours(-4), "B06", ErrorType.MinuteError.ToString(), "7956", "846111111");
                //sqlserverHelper.InsertLogToDB("Error export 3", DateTime.Now.AddHours(-3), "A03", ErrorType.HourError.ToString(), "562", "8466666666");
                //sqlserverHelper.InsertLogToDB("Error export 4", DateTime.Now.AddHours(-2), "X01", ErrorType.MinuteError.ToString(), "789", "843333333");

                //var sds = System.Web.HttpContext.Current.User.Identity.Name;
                var temp = sqlserverHelper.GetAllHotNumber();
                var selectedCaseId = temp[0].InterceptId;
                var currentId = temp[0].Id;

                var listCase = mainHelper.GetListCaseObject().OrderBy(m=>m.name).ToList();
                //var listCase = new List<CaseObject>();
                //listCase.Add(new CaseObject { id = "1", name = "C02" });
                //listCase.Add(new CaseObject { id = "2", name = "C03" });
                //listCase.Add(new CaseObject { id = "3", name = "C04" });
                var listIntercept = new List<ExportObject>();
                //if (listCase.Count() > 0)
                //{
                //    var casename = listCase[0].name;
                //    var tempTarget = new ExportTarget { TargetName = casename };
                //    listIntercept = mainHelper.GetListInterceptName(tempTarget);
                //}

                //listIntercept.Add(new ExportObject { InterceptId = "123", InterceptName = "Intercept 1" });
                //listIntercept.Add(new ExportObject { InterceptId = "658", InterceptName = "Apple Inc" });
                //listIntercept.Add(new ExportObject { InterceptId = "987", InterceptName = "Microsoft JSC" });

                ViewBag.AllCase = new SelectList(listCase, "id", "name");
                ViewBag.SelectedCaseId = selectedCaseId;
                ViewBag.CurrentId = currentId;

                //ViewBag.AllIntercept = new SelectList(listIntercept, "InterceptId", "InterceptName");

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
                
            
        }

        [HttpGet]
        public ActionResult GetListIntercept(string casename)
        {
            var tempTarget = new ExportTarget { TargetName = casename };
            var tempList = new List<ExportObject>();
            tempList = mainHelper.GetListInterceptName(tempTarget).OrderBy(m=>m.InterceptName).ToList();
            tempList.Insert(0, new ExportObject { InterceptId = "0", InterceptName = "All" });

            return Json(tempList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(HotNumber number)
        {
            var response = sqlserverHelper.DeleteHotNumber(number.CaseName, number.PhoneNumber);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Update(HotNumber number)
        {
            var response = sqlserverHelper.UpdateHotNumber(number.CaseName, number.InterceptId, number.Id);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddPhoneNumber(HotNumber phone)
        {
            try
            {
                sqlserverHelper.AddHotNumber(phone);
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
        }
    }
}