using System.Web;
using System.Web.Optimization;

namespace IEGen
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryunobtrusive").Include("~/Scripts/jquery.unobtrusive*"));
            bundles.Add(new ScriptBundle("~/bundles/datatables-core").Include(
                                "~/Scripts/DataTables/jquery.dataTables.min.js",
                                "~/Scripts/DataTables/dataTables.bootstrap.min.js",
                                "~/Scripts/SiteDT.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables-buttons").Include(
                                "~/Scripts/DataTables/dataTables.buttons.min.js",
                                "~/Scripts/DataTables/buttons.bootstrap.min.js",
                                "~/Scripts/jszip.min.js",
                                "~/Scripts/pdfmake/pdfmake.min.js",
                                "~/Scripts/pdfmake/vfs_fonts.js",
                                "~/Scripts/DataTables/buttons.html5.min.js",
                                "~/Scripts/DataTables/buttons.print.min.js",
                                "~/Scripts/DataTables/buttons.colVis.min.js",
                                "~/Scripts/SiteDTBtn.js"));

            bundles.Add(new StyleBundle("~/bundles/datatables-styles").Include("~/Content/DataTables/css/dataTables.bootstrap.min.css"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/malsupform").Include(
                        "~/Scripts/jquery.form.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/chart").Include(
                        "~/Scripts/Chart.bundle.min.js",
                        "~/Scripts/chartjs-plugin-labels.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/customval").Include(
                        "~/Scripts/customValidation.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                                "~/Scripts/jquery.datetimepicker.full.min.js",
                                "~/Scripts/Site.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/jquery.datetimepicker.min.css",
                      "~/Content/site.css"));
        }
    }
}
