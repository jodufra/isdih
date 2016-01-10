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
    public enum OrderOptionRecord { IdRecordAsc, IdRecordDesc, NodeIdAsc, NodeIdDesc, ChannelAsc, ChannelDesc, ValueAsc, ValueDesc, DateCreatedAsc, DateCreatedDesc }
    public enum FilterOptionRecord { NodeId, NodeIds, Channel, Channels, DateCreatedMin, DateCreatedMax }
    public enum GroupOptionRecord { Average, Min, Max }

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

        public static List<Record> Get(Int32? PageId, Int32? PageSize, Int32? MaxCount, out Int32 Count, OrderOptionRecord? OrderBy, Dictionary<FilterOptionRecord, Object> Options)
        {
            var query = from p in db.Records select p;

            if (Options != null)
            {
                if (Options.ContainsKey(FilterOptionRecord.NodeId))
                {
                    Int32 id = (Int32)Options[FilterOptionRecord.NodeId];
                    query = from p in query where p.NodeId == id select p;
                }
                if (Options.ContainsKey(FilterOptionRecord.NodeIds))
                {
                    Int32[] ids = (Int32[])Options[FilterOptionRecord.NodeIds];
                    query = from p in query where ids.Contains(p.NodeId) select p;
                }
                if (Options.ContainsKey(FilterOptionRecord.Channel))
                {
                    String channel = (String)Options[FilterOptionRecord.Channel];
                    query = from p in query where p.Channel == channel select p;
                }
                if (Options.ContainsKey(FilterOptionRecord.Channels))
                {
                    String[] channels = (String[])Options[FilterOptionRecord.Channels];
                    query = from p in query where channels.Contains(p.Channel) select p;
                }
                if (Options.ContainsKey(FilterOptionRecord.DateCreatedMin))
                {
                    DateTime date = (DateTime)Options[FilterOptionRecord.DateCreatedMin];
                    query = from p in query where p.DateCreated >= date select p;
                }
                if (Options.ContainsKey(FilterOptionRecord.DateCreatedMax))
                {
                    DateTime date = (DateTime)Options[FilterOptionRecord.DateCreatedMax];
                    query = from p in query where p.DateCreated <= date select p;
                }
            }

            switch (OrderBy)
            {
                case OrderOptionRecord.IdRecordAsc:
                    query = query.OrderBy(p => p.IdRecord);
                    break;
                case OrderOptionRecord.IdRecordDesc:
                    query = query.OrderByDescending(p => p.IdRecord);
                    break;
                case OrderOptionRecord.NodeIdAsc:
                    query = query.OrderBy(p => p.NodeId);
                    break;
                case OrderOptionRecord.NodeIdDesc:
                    query = query.OrderByDescending(p => p.NodeId);
                    break;
                case OrderOptionRecord.ChannelAsc:
                    query = query.OrderBy(p => p.Channel);
                    break;
                case OrderOptionRecord.ChannelDesc:
                    query = query.OrderByDescending(p => p.Channel);
                    break;
                case OrderOptionRecord.ValueAsc:
                    query = query.OrderBy(p => p.Value);
                    break;
                case OrderOptionRecord.ValueDesc:
                    query = query.OrderByDescending(p => p.Value);
                    break;
                case OrderOptionRecord.DateCreatedAsc:
                    query = query.OrderBy(p => p.DateCreated);
                    break;
                case OrderOptionRecord.DateCreatedDesc:
                    query = query.OrderByDescending(p => p.DateCreated);
                    break;
                default:
                    query = query.OrderByDescending(p => p.IdRecord);
                    break;
            }
            Count = query.Count();

            List<Record> result;
            if (PageId != null && PageSize != null)
                result = query.Skip(Math.Max(((Int32)PageId - 1), 0) * (Int32)PageSize).Take((Int32)PageSize).Select(s => s).ToList();
            else if (MaxCount != null && MaxCount > 0)
                result = query.Take((int)MaxCount).Select(s => s).ToList();
            else result = query.Select(s => s).ToList();

            return result;

        }

        public static List<RecordStatistic> Get(DateTime dateMin, DateTime dateMax, GroupOptionRecord option)
        {
            var query = (from p in db.Records where p.DateCreated >= dateMin && p.DateCreated <= dateMax select p).ToList();
            var interval = dateMax - dateMin;
            var days = interval.TotalDays;
            var hours = interval.TotalHours;
            var minutes = interval.TotalMinutes;

            IEnumerable<RecordStatistic> qrs;
            if (days > 365)
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = (p.DateCreated.Day < 16 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, 1) : new DateTime(p.DateCreated.Year, p.DateCreated.Month, 16))
                      };
            }
            else if (days > 182)
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = (p.DateCreated.Day < 11 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, 1) :
                          p.DateCreated.Day < 21 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, 11) :
                          new DateTime(p.DateCreated.Year, p.DateCreated.Month, 21))
                      };
            }
            else if (days > 91)
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = (p.DateCreated.Day < 6 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, 1) :
                          p.DateCreated.Day < 11 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, 6) :
                          p.DateCreated.Day < 16 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, 11) :
                          new DateTime(p.DateCreated.Year, p.DateCreated.Month, 16))
                      };
            }
            else if (days > 30)
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day)
                      };
            }
            else if (days > 7)
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = (p.DateCreated.Hour < 8 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, 0, 0, 0) :
                          p.DateCreated.Hour < 16 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, 8, 0, 0) :
                          new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, 16, 0, 0))
                      };
            }
            else if (days > 1)
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 0, 0)
                      };
            }
            else if (hours > 12)
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = (p.DateCreated.Minute < 30 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 0, 0) :
                          new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 30, 0))
                      };
            }
            else if (hours > 6)
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = (p.DateCreated.Minute < 15 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 0, 0) :
                          p.DateCreated.Minute < 30 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 15, 0) :
                          p.DateCreated.Minute < 45 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 30, 0) :
                          new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 45, 0))
                      };
            }
            else if (hours > 2)
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = (p.DateCreated.Minute < 5 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 0, 0) :
                          p.DateCreated.Minute < 10 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 5, 0) :
                          p.DateCreated.Minute < 15 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 10, 0) :
                          p.DateCreated.Minute < 20 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 15, 0) :
                          p.DateCreated.Minute < 25 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 20, 0) :
                          p.DateCreated.Minute < 30 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 25, 0) :
                          p.DateCreated.Minute < 35 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 30, 0) :
                          p.DateCreated.Minute < 40 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 35, 0) :
                          p.DateCreated.Minute < 45 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 40, 0) :
                          p.DateCreated.Minute < 50 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 45, 0) :
                          p.DateCreated.Minute < 55 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 50, 0) :
                          new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, 58, 0))
                      };
            }
            else if (hours > 1)
            {

                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = (p.DateCreated.Minute % 2 == 0 ? new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, p.DateCreated.Minute, 0) :
                          new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, p.DateCreated.Minute - 1, 0))
                      };
            }
            else
            {
                qrs = from p in query
                      select new RecordStatistic
                      {
                          Channel = p.Channel,
                          Value = p.Value,
                          Date = new DateTime(p.DateCreated.Year, p.DateCreated.Month, p.DateCreated.Day, p.DateCreated.Hour, p.DateCreated.Minute, 0)
                      };
            }


            switch (option)
            {
                case GroupOptionRecord.Average:
                    qrs = from p in qrs group p by new { p.Channel, p.Date } into g select new RecordStatistic { Channel = g.Key.Channel, Value = g.Average(p => p.Value), Date = g.Key.Date };
                    break;
                case GroupOptionRecord.Min:
                    qrs = from p in qrs group p by new { p.Channel, p.Date } into g select new RecordStatistic { Channel = g.Key.Channel, Value = g.Min(p => p.Value), Date = g.Key.Date };
                    break;
                case GroupOptionRecord.Max:
                    qrs = from p in qrs group p by new { p.Channel, p.Date } into g select new RecordStatistic { Channel = g.Key.Channel, Value = g.Max(p => p.Value), Date = g.Key.Date };
                    break;
            }

            qrs.OrderBy(p => p.Date);

            return qrs.ToList();/*
            return result.Select(x => new RecordStatistic
            {
                Channel = x.Channel,
                Value = x.Value,
                Date = x.DateCreated
            }).ToList();*/
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
