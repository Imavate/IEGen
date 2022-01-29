using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace IEGen
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfig.RegisterMappings();
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Get the error details
            HttpException ex = Server.GetLastError() as HttpException;

            if (ex != null && ex.GetHttpCode() != 404)
            {
                if (ex.InnerException == null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                else
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex.InnerException);
            }
        }
    }
}
