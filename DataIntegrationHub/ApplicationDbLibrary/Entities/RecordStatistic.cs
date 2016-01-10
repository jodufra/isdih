using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationDbLibrary.Entities
{
    public class RecordStatistic
    {
        public string Channel { get; set; }

        public float Value { get; set; }

        public DateTime Date { get; set; }
    }
}
