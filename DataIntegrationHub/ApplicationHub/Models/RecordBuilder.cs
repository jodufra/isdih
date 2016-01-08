using ApplicationLib;
using ApplicationLib.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationHub.Models
{
    public static class RecordBuilder
    {
        public static Record BuildRecord(String record)
        {
            DateTime date = DateTime.Now;
            string[] data;
            if (string.IsNullOrEmpty(record) || (data = record.Split(';')).Length != 3)
                throw new ArgumentException();
            return new Record() { NodeId = int.Parse(data[0]), Channel = data[1], Value = float.Parse(data[2], CultureInfo.InvariantCulture), DateCreated = date, DateCreatedTicks = date.Ticks };
        }
    }
}
