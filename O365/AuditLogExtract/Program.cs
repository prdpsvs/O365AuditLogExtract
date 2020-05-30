namespace AuditLogExtract
{
    using AuditLogExtract.Config;
    using AuditLogExtract.Data;
    using AuditLogExtract.Entities;
    using AuditLogExtract.Instrumentation;
    using AuditLogExtract.ServiceClient;
    using Autofac;
    using log4net.Layout;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    class Program
    {
        static private IContainer CompositionRoot(string[] args)
        {
            var builder = new ContainerBuilder();


            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            string appSettingContent = File.ReadAllText(path);

            var appSettings = JsonConvert.DeserializeObject<AppSettings>(appSettingContent);
            appSettings.ReleaseId = DateTime.Now.ToString("MMddyyHHmm");
            //Dha.APaaS.RM.Pinot.Entities
            builder.RegisterInstance(appSettings).As<AppSettings>().SingleInstance();

            //Dha.APaaS.RM.Pinot.Common
            builder.RegisterType<KeyVaultClientWrapper>().As<IKeyVaultClientWrapper>();
            builder.RegisterType<Initializer>().As<IInitializer>().SingleInstance();
            builder.RegisterType<Configuration>().AsSelf();
            builder.RegisterModule<LoggingModule>();
            builder.RegisterType<PatternLayout>().AsSelf();
            builder.RegisterType<LogAppender>().As<ILogAppender>().SingleInstance();
            builder.RegisterType<AuthenticationWrapper>().As<IAuthenticationWrapper>();
            builder.RegisterType<O365ActivityApiWrapper>().As<IO365ActivityApiWrapper>();
            builder.RegisterType<MetadataStoreContextFactory>().As<IMetadataStoreContextFactory>();
            builder.RegisterType<SqlConnectionProperties>().AsSelf();
            builder.RegisterType<Ado>().As<IAdo>();
            builder.RegisterType<Orchestrator>().AsSelf();
            builder.RegisterType<RetryClient>().As<IRetryClient>();

            return builder.Build();
        }

        static async Task Main(string[] args)
        {
            var ctxt = CompositionRoot(args);
            await ctxt.Resolve<Orchestrator>().ExtractOffice365AuditLogsAsync().ConfigureAwait(false);
        }
    }
}
