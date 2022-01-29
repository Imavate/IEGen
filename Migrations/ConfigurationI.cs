namespace IEGen.Migrations
{
    using AppSecrets;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class ConfigurationI : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public ConfigurationI()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            ApplicationUserManager UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            string AdminPasswordHash = UserManager.PasswordHasher.HashPassword(Secrets.AdminPass);

            context.Users.AddOrUpdate
                (u => u.UserName,
                 new ApplicationUser
                 {
                     UserName = Secrets.AdminUser,
                     Email = Secrets.AdminUser,
                     PasswordHash = AdminPasswordHash,
                     SecurityStamp = Guid.NewGuid().ToString()
                 }
                );

            context.SaveChanges();
        }
    }
}
