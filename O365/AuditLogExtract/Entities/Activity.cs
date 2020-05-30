using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditLogExtract.Entities
{
    public class Activity
    {
        public string RawActivity { get; set; }
        public Guid ActivityId { get; set; }
        public string Operation { get; set; }
    }

}
