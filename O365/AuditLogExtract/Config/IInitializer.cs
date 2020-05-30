using AuditLogExtract.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditLogExtract.Config
{
    public interface IInitializer
    {
        Configuration Configuration { get; }
        void AddItem(string key, object value);
        bool DoesExist(string key);
        object GetItem(string key);
        Task InitializeConfigurationAsync();
    }
}
