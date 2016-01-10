using ApplicationDbLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace ApplicationWebService
{
    [ServiceContract]
    public interface IDataIntegrationService
    {
        [OperationContract]
        List<Record> GetRecordsByChannelAndDatespan(String[] channels, QueryTimeSpan datespan);

        [OperationContract]
        List<RecordStatistic> GetRecordsByDataRange(DateTime dateMin, DateTime dateMax, QueryGroupFunction groupFunction);
    }

    [DataContract]
    public enum QueryGroupFunction
    {
        [EnumMember]
        Average,
        [EnumMember]
        Min,
        [EnumMember]
        Max
    }

    [DataContract]
    public enum QueryTimeSpan
    {
        [EnumMember]
        LastHours,
        [EnumMember]
        LastWeek,
        [EnumMember]
        LastMonth
    }

    [DataContract]
    public class Record
    {
        private int _idRecord;
        private int _nodeId;
        private string _channel;
        private float _value;
        private DateTime _dateCreated;

        [DataMember]
        public int IdRecord { get { return _idRecord; } set { _idRecord = value; } }
        [DataMember]
        public int NodeId { get { return _nodeId; } set { _nodeId = value; } }
        [DataMember]
        public string Channel { get { return _channel; } set { _channel = value; } }
        [DataMember]
        public float Value { get { return _value; } set { _value = value; } }
        [DataMember]
        public DateTime DateCreated { get { return _dateCreated; } set { _dateCreated = value; } }

        public static List<Record> Convert(List<ApplicationDbLibrary.Entities.Record> records)
        {
            var result = new List<Record>();
            foreach (var item in records)
            {
                result.Add(Convert(item));
            }
            return result;
        }

        public static Record Convert(ApplicationDbLibrary.Entities.Record record)
        {
            return new Record()
            {
                IdRecord = record.IdRecord,
                NodeId = record.NodeId,
                Channel = record.Channel,
                Value = record.Value,
                DateCreated = record.DateCreated,
            };
        }
    }


    public class RecordStatistic
    {
        private string _channel;
        private float _value;
        private DateTime _date;

        [DataMember]
        public string Channel { get { return _channel; } set { _channel = value; } }
        [DataMember]
        public float Value { get { return _value; } set { _value = value; } }
        [DataMember]
        public DateTime Date { get { return _date; } set { _date = value; } }

        public static List<RecordStatistic> Convert(List<ApplicationDbLibrary.Entities.RecordStatistic> records)
        {
            var result = new List<RecordStatistic>();
            foreach (var item in records)
            {
                result.Add(Convert(item));
            }
            return result;
        }

        public static RecordStatistic Convert(ApplicationDbLibrary.Entities.RecordStatistic record)
        {
            return new RecordStatistic()
            {
                Channel = record.Channel,
                Value = record.Value,
                Date = record.Date,
            };
        }
    }

}
