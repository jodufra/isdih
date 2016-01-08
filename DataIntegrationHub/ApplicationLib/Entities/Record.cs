using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLib.Entities
{
    [Serializable]
    public partial class Record
    {
        private int _idRecord;
        public virtual int IdRecord
        {
            get
            {
                return this._idRecord;
            }
            set
            {
                this._idRecord = value;
            }
        }

        private int _nodeId;
        public virtual int NodeId
        {
            get
            {
                return this._nodeId;
            }
            set
            {
                this._nodeId = value;
            }
        }

        private string _channel;
        public virtual string Channel
        {
            get
            {
                return this._channel;
            }
            set
            {
                this._channel = value;
            }
        }

        private DateTime _dateCreated;
        public virtual DateTime DateCreated
        {
            get
            {
                return this._dateCreated;
            }
            set
            {
                this._dateCreated = value;
            }
        }

        private long _dateCreatedTicks;
        public virtual long DateCreatedTicks
        {
            get
            {
                return this._dateCreatedTicks;
            }
            set
            {
                this._dateCreatedTicks = value;
            }
        }

        private float _value;
        public virtual float Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }
        public string Log { get { return (new StringBuilder().Append(NodeId).Append(" - ").Append(Channel).Append(" - ").Append(Value.ToString("F")).Append(" - ").Append(DateCreated.ToLongTimeString()).Append(" - ").Append(DateCreatedTicks)).ToString(); } }
    }
}
