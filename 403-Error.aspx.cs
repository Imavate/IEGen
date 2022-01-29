using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IEGen
{
    public partial class _403_Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpContext.Current.Response.StatusCode = 403;
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            HttpContext.Current.Response.StatusDescription = "403 Forbidden";
        }
    }
}