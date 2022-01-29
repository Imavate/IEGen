namespace IEGen.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class ConfigurationF : DbMigrationsConfiguration<IEGen.Models.FileContext>
    {
        public ConfigurationF()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(IEGen.Models.FileContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
