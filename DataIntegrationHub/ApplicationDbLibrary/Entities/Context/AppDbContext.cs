using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationDbLibrary.Entities.Context
{
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class AppDbContext : DbContext
    {
        private static AppDbContext dbContext = null;
        public static AppDbContext DbContext { get { return dbContext ?? (dbContext = new AppDbContext()); } }

        public AppDbContext() : this("Connection") { }
        public AppDbContext(string connStringName) : base(connStringName) { }
        static AppDbContext()
        {
            DbConfiguration.SetConfiguration(new MySql.Data.Entity.MySqlEFConfiguration());
        }

        public DbSet<Record> Records { get; set; }

        public override int SaveChanges()
        {
            return SaveChanges("");
        }
        public int SaveChanges(String debugInfo)
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = new List<String>();
                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorMessages.Add(ve.ErrorMessage);
                    }
                }
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage, debugInfo);
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }
    }
}
