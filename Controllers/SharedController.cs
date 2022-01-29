using IEGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace IEGen.Controllers
{
    public class SharedController : Controller
    {
        // GET: Shared
        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult _ExternalHeader()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult _WebHeader()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult _AdminHeader(BasePageViewModel model)
        {
            return PartialView("_Header", model.HeaderViewModel);
        }

        [ChildActionOnly]
        public ActionResult _Header()
        {
            return _Header(User.Identity.Name, (byte)HeaderTabs.Home);
        }

        [ChildActionOnly]
        [Route("_Header/{user}/{tab:int}")]
        public ActionResult _Header(string user, int tab)
        {
            return PartialView(General.GetHeaderModel(user, tab));
        }

        [Route("Error/{code:int}")]
        public ActionResult Error(int code)
        {
            Response.StatusCode = code;
            if (code == 403) return View("403");
            if (code == 404) return View("404");
            return View("Error");
        }

        [Route("Error/{ErrMsg?}")]
        public ActionResult Error(string ErrMsg)
        {
            General.AlertElmah(new Exception(ErrMsg));
            Response.StatusCode = 501;
            return View("Error", new HandleErrorInfo(new Exception(string.IsNullOrEmpty(ErrMsg) ? "Invalid Controller or/and Action Name" : ErrMsg), "Unknown", "Unknown"));
        }

        [Route("_Logo/{SchoolID:int}")]
        [OutputCache(Duration = 86400, VaryByParam = "SchoolID", Location = OutputCacheLocation.Client)]
        public ActionResult _Logo(int SchoolID)
        {
            return PartialView(General.GetHeaderLogo(SchoolID));
        }

    }
}