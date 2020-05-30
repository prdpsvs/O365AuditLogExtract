using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditLogExtract.Entities
{
    public class Configuration
    {
        private AppSettings appSettings;

        public Configuration()
        {
            AppSettings = new AppSettings();
        }

        public AppSettings AppSettings { get => appSettings; set => appSettings = value; }
    }
}
