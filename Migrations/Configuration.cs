namespace IEGen.Migrations
{
    using AppSecrets;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<IEGen.Models.IEContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(IEContext context)
        {
            SeedUserRole(context);
            SeedAccessGroup(context);
            SeedAccessGroupRole(context);
            SeedIEUser(context);
            SeedEmailSettings(context);
        }

        void SeedAccessGroup(IEContext context)
        {
            context.AccessGroupList.AddOrUpdate
                (ag => ag.AccessGroupID,
                 new AccessGroup { AccessGroupID = 1, Name = "Super Administrators", TimeChanged = DateTime.Now },
                 new AccessGroup { AccessGroupID = 2, Name = "School Administrators", TimeChanged = DateTime.Now },
                 new AccessGroup { AccessGroupID = 3, Name = "New Users", TimeChanged = DateTime.Now }
                );
            context.SaveChanges();
        }

        void SeedAccessGroupRole(IEContext context)
        {
            foreach (byte roleID in Enum.GetValues(typeof(UserRoles)))
            {
                context.AccessGroupRoleList.AddOrUpdate
                (ag => new { ag.AccessGroupID, ag.RoleID },
                 new AccessGroupRole { AccessGroupID = 1, RoleID = roleID }//,
                 //new AccessGroupRole { AccessGroupID = 2, RoleID = roleID }
                );
            }
            context.SaveChanges();
        }

        void SeedIEUser(IEContext context)
        {
            context.IEUserList.AddOrUpdate
                (sg => sg.UserID,
                 new IEUser
                 {
                     UserID = 1,
                     Name = "System Administrator",
                     Email = Secrets.AdminUser,
                     AccessGroupID = 1,
                     TypeID = (byte)UserType.AppAdmin
                 }
                );
            context.SaveChanges();
        }

        void SeedEmailSettings(IEContext context)
        {
            context.EmailSettingsList.AddOrUpdate
                (ss => ss.SettingsID,
                 new EmailSettings
                 {
                     SettingsID = 1,
                     SmtpServer = "",
                     SmtpPort = 25,
                     UseDefaultMailParameters = true,
                     EnableSslMail = true
                 }
                );
            context.SaveChanges();
        }

        void SeedUserRole(IEContext context)
        {
            context.UserRoleList.AddOrUpdate
                (ur => ur.RoleID,
                 new UserRole { RoleID = (byte)UserRoles.ManageAccessGroups, Name = "System Admin: Manage Access Groups" },
                 new UserRole { RoleID = (byte)UserRoles.ManageUsers, Name = "System Admin: Manage Users" },
                 new UserRole { RoleID = (byte)UserRoles.SetupMailingSystem, Name = "System Admin: Setup Mailing System" },
                 new UserRole { RoleID = (byte)UserRoles.ViewAuditTrail, Name = "System Admin: View Audit Trail" },
                 new UserRole { RoleID = (byte)UserRoles.ManageSystemAudit, Name = "System Admin: Manage System Audit" },
                 new UserRole { RoleID = (byte)UserRoles.ManageOtherSettings, Name = "System Admin: Manage Other Settings" },
                 new UserRole { RoleID = (byte)UserRoles.ManageSchools, Name = "System Admin: Manage Organizations" },
                 new UserRole { RoleID = (byte)UserRoles.ManageStudents, Name = "System Admin: Manage Students" },
                 new UserRole { RoleID = (byte)UserRoles.ManageTeachers, Name = "System Admin: Manage Teachers" },
                 new UserRole { RoleID = (byte)UserRoles.EditSchoolDetails, Name = "Setup: Edit School Details" },
                 new UserRole { RoleID = (byte)UserRoles.ManageTerm, Name = "Setup: Manage School Term" },
                 new UserRole { RoleID = (byte)UserRoles.ViewReports, Name = "Reports: View Reports" },
                 new UserRole { RoleID = (byte)UserRoles.EnterScores, Name = "Data: Enter Scores" },
                 new UserRole { RoleID = (byte)UserRoles.EnterSkills, Name = "Data: Enter Skills" },
                 new UserRole { RoleID = (byte)UserRoles.EnterAttendance, Name = "Data: Enter Attendance" },
                 new UserRole { RoleID = (byte)UserRoles.EditStudent, Name = "Data: Edit Student Info" }
                );
            context.SaveChanges();
        }
    }
}
