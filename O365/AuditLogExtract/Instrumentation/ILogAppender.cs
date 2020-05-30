using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditLogExtract.Instrumentation
{
    public interface ILogAppender
    {
        void Initialize(List<string> targetAppenders);
    }
}
