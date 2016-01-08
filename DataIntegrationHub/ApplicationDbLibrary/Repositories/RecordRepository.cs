using ApplicationDbLibrary.Entities;
using ApplicationDbLibrary.Entities.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationDbLibrary.Repositories
{
    public static class RecordRepository
    {
        private static AppDbContext db { get { return AppDbContext.DbContext; } }

        public static int Count()
        {
            return db.Records.Count();
        }

        public static List<Record> Get()
        {
            return db.Records.ToList();
        }

        public static Record Get(int id)
        {
            return db.Records.Where(r => r.IdRecord == id).FirstOrDefault();
        }

        public static List<Record> GetByNodeId(int id)
        {
            return db.Records.Where(r => r.NodeId == id).ToList();
        }

        public static List<Record> GetByChannel(string channel)
        {
            return db.Records.Where(r => r.Channel == channel).ToList();
        }

        public static List<String> Save(Record record)
        {
            var errors = new List<String>();
            try
            {
                if (record.IsNew)
                    record.DateCreated = DateTime.Now;
                db.Records.Add(record);
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errors.Add(ve.PropertyName + "-" + ve.ErrorMessage);
                    }
                }
            }

            return errors;
        }
    }
}
