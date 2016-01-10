using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationDbLibrary.Entities.Context
{
    public class Configuration : DbMigrationsConfiguration<AppDbContext>
    {

        private readonly bool hasPendingMigrations;
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;

            var migrator = new DbMigrator(this);
            hasPendingMigrations = migrator.GetPendingMigrations().Any();
        }


        private readonly bool SEED_SAMPLE_DATA = false; /* May take a long to complete */

        protected override void Seed(AppDbContext context)
        {
            /*
             * Helper for DB debug: SHOW ENGINE INNODB STATUS
             */

            if (!hasPendingMigrations || !SEED_SAMPLE_DATA) return;
            Console.WriteLine("INFO: Seeding DB");

            #region Seeds
            var mh = new MigrationHelper();
            Record[] records = new Record[1095];
            var date = new DateTime(2015, 1, 22);
            int j = 0;
            for (int i = 0; i < 1095; j++, i++)
            {
                if (j == 3)
                {
                    j = 0;
                    date = date.AddDays(1);
                }
                records[i] = new Record()
                {
                    NodeId = j + 1,
                    Channel = j == 0 ? "T" : j == 1 ? "H" : "P",
                    DateCreated = date,
                    Value = j == 0 ? mh.random.Next(20, 30) : j == 1 ? mh.random.Next(70, 85) : (float)30.0 + (float)Math.Round(mh.random.NextDouble(), 2)
                };
            }
            context.Records.AddOrUpdate(records);

            #endregion

            Console.WriteLine("INFO: Seeding Complete");
        }


        private class MigrationHelper
        {
            public readonly Random random = new Random();
            public readonly String chars = " ABCDEFGHIJKLMNOPQRSTUVWXYZ - abcdefghijklmnopqrstuvwxyz - 0123456789 ";
            public readonly DateTime startDate = new DateTime(2015, 1, 1, 0, 0, 0);
            public readonly TimeSpan timeSpan = new DateTime(2016, 1, 1, 0, 0, 0) - new DateTime(2010, 1, 1, 0, 0, 0);


            public bool RandomBoolean(int probatility)
            {
                return (random.Next(100) <= probatility);
            }

            public string RandomString()
            {
                return new string(Enumerable.Repeat(chars, 16).Select(s => s[random.Next(s.Length)]).ToArray());
            }

            public DateTime RandomDateTime()
            {
                return startDate + new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);
            }

        }
    }
}
