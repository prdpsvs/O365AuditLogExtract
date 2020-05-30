namespace AuditLogExtract.Config
{
    using AuditLogExtract.Entities;
    using AuditLogExtract.ServiceClient;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Environment = Entities.Environment;

    public class Initializer : IInitializer
    {
        private Configuration _configuration;
        IKeyVaultClientWrapper _keyVaultClientWrapper;
        private Dictionary<string, object> _localMap;

        public Initializer(IKeyVaultClientWrapper keyVaultWrapper, AppSettings appSettings, Configuration configuration)
        {
            _localMap = new Dictionary<string, object>();
            _keyVaultClientWrapper = keyVaultWrapper;
            _configuration = configuration;
            _configuration.AppSettings = appSettings;
        }

        public Configuration Configuration { get => _configuration; }

        /// <summary>
        /// Add Item to local Map InMemory
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddItem(string key, object value)
        {
            if (_localMap.ContainsKey(key))
                _localMap[key] = value;
            else
                _localMap.Add(key, value);
        }

        /// <summary>
        /// Fetch object from InMemory
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetItem(string key)
        {
            object result = null;
            if (_localMap.ContainsKey(key))
                result = _localMap[key];

            return result;
        }

        /// <summary>
        /// Check if key exists in InMemory storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool DoesExist(string key)
        {
            if (_localMap.ContainsKey(key))
                return true;
            return false;
        }

        /// <summary>
        /// Initialize configuration
        /// </summary>
        /// <returns></returns>
        public async Task InitializeConfigurationAsync()
        {
            if (_configuration.AppSettings.Env != Enum.GetName(typeof(Environment.Env), 0))
            {
                _configuration.AppSettings.DatabaseClientId = await _keyVaultClientWrapper.GetSecretAsync(_configuration.AppSettings.DatabaseClientIdName).ConfigureAwait(false);
                _configuration.AppSettings.DatabaseClientSecret = await _keyVaultClientWrapper.GetSecretAsync(_configuration.AppSettings.DatabaseClientSecretName).ConfigureAwait(false);
            }
            _configuration.AppSettings.AuditLogClientId = await _keyVaultClientWrapper.GetSecretAsync(_configuration.AppSettings.AuditLogClientIdName).ConfigureAwait(false);
            _configuration.AppSettings.AuditLogClientSecret = await _keyVaultClientWrapper.GetSecretAsync(_configuration.AppSettings.AuditLogClientSecretName).ConfigureAwait(false);

        }
    }
}
