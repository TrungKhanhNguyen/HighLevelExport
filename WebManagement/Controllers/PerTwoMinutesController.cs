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
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                var sds = System.Web.HttpContext.Current.User.Identity.Name;
                
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
                
            return View();
        }
    }
}