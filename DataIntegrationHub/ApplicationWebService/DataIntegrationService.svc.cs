using ApplicationDbLibrary.Entities;
using ApplicationDbLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace ApplicationWebService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class DataIntegrationService : IDataIntegrationService
    {
        public List<Record> GetRecordsByChannelAndDatespan(string[] channels, QueryTimeSpan timespan)
        {
            var options = new Dictionary<FilterOptionRecord, Object>();
            if (channels != null && channels.Count() > 0)
                options.Add(FilterOptionRecord.Channels, channels);

            DateTime dateMin = DateTime.Now;
            DateTime dateMax = DateTime.Now;
            switch (timespan)
            {
                case QueryTimeSpan.LastHours:
                    dateMin = dateMin.AddDays(-1);
                    break;
                case QueryTimeSpan.LastWeek:
                    dateMin = dateMin.AddDays(-6);
                    break;
                case QueryTimeSpan.LastMonth:
                    dateMin = dateMin.AddDays(-30);
                    break;
            }
            options.Add(FilterOptionRecord.DateCreatedMin, dateMin);
            options.Add(FilterOptionRecord.DateCreatedMax, dateMax);

            int count;
            var records = RecordRepository.Get(null, null, null, out count, OrderOptionRecord.DateCreatedAsc, options);

            return ApplicationWebService.Record.Convert(records);
        }


        public List<RecordStatistic> GetRecordsByDataRange(DateTime dateMin, DateTime dateMax, QueryGroupFunction groupFunction)
        {
            GroupOptionRecord option;
            switch (groupFunction)
            {
                case QueryGroupFunction.Average:
                    option = GroupOptionRecord.Average;
                    break;
                case QueryGroupFunction.Min:
                    option = GroupOptionRecord.Min;
                    break;
                case QueryGroupFunction.Max:
                    option = GroupOptionRecord.Max;
                    break;
                default:
                    option = GroupOptionRecord.Average;
                    break;
            }

            var records = RecordRepository.Get(dateMin, dateMax, option);
            return ApplicationWebService.RecordStatistic.Convert(records);
        }
    }
}
