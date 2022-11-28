using ConnectionHelper.Helper;
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
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                //var sds = System.Web.HttpContext.Current.User.Identity.Name;
                var listNumber = sqlserverHelper.GetAllHotNumber();
                return View(listNumber);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
                
            
        }

        public ActionResult Delete(string id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPhoneNumber(HotNumber phone)
        {
            try
            {
                //var tempTarget = new ExportTarget { TargetId = id, TargetName = name };
                sqlserverHelper.AddHotNumber(phone);
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("Failed!", JsonRequestBehavior.AllowGet);
            }
        }
    }
}