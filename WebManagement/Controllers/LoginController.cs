using ConnectionHelper.Helper;
using ConnectionHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace WebManagement.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        private Utility util = new Utility();
        private DBHelper helper = new DBHelper();
        private SQLServerHelper sqlServerHelper = new SQLServerHelper();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manual");
            }
            else
            {
                return View();
            }
            
        }

        [HttpPost]
        public ActionResult Login(Login user)
        {
            var username = user.Username;
            var clearPass = user.Password;
            var md5Pass = util.MD5Hash(clearPass);
            var userLogin = sqlServerHelper.GetUserByUsernameAndPass(username, md5Pass);
            if (userLogin != null)
            {
                FormsAuthentication.SetAuthCookie(userLogin.Username + "-" + userLogin.IsAdmin, false);
                return RedirectToAction("Index", "Manual");
            }
            else
            {
                ViewBag.IsFailed = "Failed";
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login","Login");
        }
    }
}