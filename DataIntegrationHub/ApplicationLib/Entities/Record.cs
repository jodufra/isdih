using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLib
{
    public partial class Record
    {
        public string Log { get { return (new StringBuilder().Append(NodeId).Append(" - ").Append(Channel).Append(" - ").Append(Value.ToString("F")).Append(" - ").Append(DateCreated.ToLongTimeString()).Append(" - ").Append(DateCreatedTicks)).ToString(); } }
    }
}
