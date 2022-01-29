using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.Dashboard;

[assembly: OwinStartupAttribute(typeof(IEGen.Startup))]
namespace IEGen
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            var options = new Hangfire.SqlServer.SqlServerStorageOptions
            {
                QueuePollInterval = System.TimeSpan.FromSeconds(30) // Default value = 15
            };

            GlobalConfiguration.Configuration.UseSqlServerStorage(Models.IEContext.ConnString, options);

            app.UseHangfireServer();

            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            });

            RecurringJob.AddOrUpdate(() => Models.General.SendEmails(), Cron.MinuteInterval(1));
        }
    }

    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In case you need an OWIN context, use the next line, `OwinContext` class
            // is the part of the `Microsoft.Owin` package.
            var owinContext = new OwinContext(context.GetOwinEnvironment());

            var users = new System.Collections.Generic.List<string> { "admin@imavate.com" };

            return users.Contains(owinContext.Authentication.User.Identity.Name);
        }
    }
}
