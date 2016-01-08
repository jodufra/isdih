using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationDbLibrary.Entities
{
    [Table("records")]
    public class Record
    {
        [Key]
        public int IdRecord { get; set; }

        [Required]
        public int NodeId { get; set; }

        [Required]
        public string Channel { get; set; }

        [Required]
        public float Value { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [NotMapped]
        public string Log { get { return new StringBuilder().Append(NodeId).Append(" - ").Append(Channel).Append(" - ").Append(Value.ToString("F")).Append(" - ").Append(DateCreated.ToLongTimeString()).Append(" - ").ToString(); } }

        [NotMapped]
        public bool IsNew { get { return this.IdRecord == 0; } }
    }
}
