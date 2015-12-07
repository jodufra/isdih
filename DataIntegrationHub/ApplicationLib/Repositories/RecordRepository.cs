using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLib.Repositories
{
    public enum RecordValidationError { RequiredNodeId, RequiredChannel, RequiredValue, RequiredDateCreated, RequiredDateCreatedTicks }
    public enum FilterOptionRecord { NodeId, Channel, DateCreatedStart, DateCreatedEnd }
    public enum OrderOptionRecord { None, DateCreatedAsc, DateCreatedDesc }
    public interface IRecordRepository
    {
        List<RecordValidationError> Save(Record record);
        Record Get(int idRecord);
        Record GetByNodeAndTicks(int nodeId, int ticks);
        List<Record> GetAllByNodeId(int nodeId);
        List<Record> GetAllByChannel(string channel);
        List<Record> Get(Int32? PageId, Int32? PageSize, out Int32 Count, OrderOptionRecord Order, Dictionary<FilterOptionRecord, Object> Options);
    }

    public class RecordRepository : IRecordRepository
    {
        DbContext db;
        public RecordRepository(DbContext _db)
        {
            db = _db;
        }

        public List<RecordValidationError> Save(Record record)
        {
            List<RecordValidationError> errors = new List<RecordValidationError>();

            if (record.NodeId <= 0)
                errors.Add(RecordValidationError.RequiredNodeId);
            if (String.IsNullOrEmpty(record.Channel)) 
                errors.Add(RecordValidationError.RequiredChannel);
            if (record.Value <= 0)
                errors.Add(RecordValidationError.RequiredValue);
            if (record.DateCreated == null)
                errors.Add(RecordValidationError.RequiredDateCreated);
            if (record.DateCreatedTicks <= 0)
                errors.Add(RecordValidationError.RequiredDateCreatedTicks);

            if (!errors.Any())
            {
                db.Add(record);
                db.SaveChanges();
            }
            return errors;
        }

        public Record Get(int idRecord)
        {
            return (from r in db.Records where r.IdRecord == idRecord select r).FirstOrDefault();
        }

        public Record GetByNodeAndTicks(int nodeId, int ticks)
        {
            return (from r in db.Records where r.NodeId == nodeId && r.DateCreatedTicks == ticks select r).FirstOrDefault();
        }

        public List<Record> GetAllByNodeId(int nodeId)
        {
            return (from r in db.Records where r.NodeId == nodeId select r).ToList();
        }

        public List<Record> GetAllByChannel(string channel)
        {
            return (from r in db.Records where r.Channel == channel select r).ToList();
        }

        public List<Record> Get(int? PageId, int? PageSize, out int Count, OrderOptionRecord Order, Dictionary<FilterOptionRecord, object> Options)
        {
            var query = from r in db.Records select r;
            if (Options != null)
            {
                if (Options.ContainsKey(FilterOptionRecord.NodeId))
                    query = from r in query where r.NodeId == (int)Options[FilterOptionRecord.NodeId] select r;
                if (Options.ContainsKey(FilterOptionRecord.Channel))
                    query = from r in query where r.Channel == (string)Options[FilterOptionRecord.Channel] select r;
                if (Options.ContainsKey(FilterOptionRecord.DateCreatedStart))
                    query = from r in query where r.DateCreatedTicks >= ((DateTime)Options[FilterOptionRecord.DateCreatedStart]).Ticks  select r;
                if (Options.ContainsKey(FilterOptionRecord.DateCreatedEnd))
                    query = from r in query where r.DateCreatedTicks <=  ((DateTime)Options[FilterOptionRecord.DateCreatedEnd]).Ticks select r;
            }

            switch (Order)
            {
                case OrderOptionRecord.DateCreatedAsc:
                    query.OrderBy(r => r.DateCreated); break;
                case OrderOptionRecord.DateCreatedDesc:
                    query.OrderByDescending(r => r.DateCreated); break;
            }

            Count = query.Count();

            if (PageId != null && PageSize != null)
                return query.Skip(((Int32)PageId - 1) * (Int32)PageSize).Take((Int32)PageSize).ToList();
            return query.ToList();
        }
    }
}
