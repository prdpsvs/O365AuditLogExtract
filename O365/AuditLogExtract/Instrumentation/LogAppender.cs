using AuditLogExtract.Config;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Log4NetAppender;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuditLogExtract.Instrumentation
{
    public class LogAppender : ILogAppender
    {
        IInitializer _initializer;
        PatternLayout _patternLayout;
        public readonly bool Verbosity;
        private static ApplicationInsightsAppender _aIAppender;

        public LogAppender(IInitializer configurationManager, PatternLayout patternLayout)
        {
            _initializer = configurationManager;
            _patternLayout = patternLayout;
            Verbosity = this.GetVerbosity();
        }

        /// <summary>
        /// Initialize Console Appender to Log4net library
        /// </summary>
        /// <param name="targetAppenders"></param>
        public void Initialize(List<string> targetAppenders)
        {
            _patternLayout.ConversionPattern = "%date | %-5level | %logger | %property{ReleaseId} | %message%newline";
            var root = ((Hierarchy)log4net.LogManager.GetRepository()).Root;
            root.RemoveAllAppenders();

            //Configuring Console Appender
            ConfigureWithConsole(_patternLayout);

            //Configuring AI Appender
            ConfigureWithAI(_initializer.Configuration.AppSettings.TelemetryInstrumentationKey, _patternLayout);

            TelemetryConfiguration.Active.InstrumentationKey = _initializer.Configuration.AppSettings.TelemetryInstrumentationKey;

            // Configuring Log Level
            if (!Verbosity)
            {
                ((Hierarchy)log4net.LogManager.GetRepository()).Root.Level = Level.Info;
                ((Hierarchy)log4net.LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
            }
            else
            {
                ((Hierarchy)log4net.LogManager.GetRepository()).Root.Level = Level.Debug;
                ((Hierarchy)log4net.LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
            }
        }

        private static void ConfigureWithAI(string TelemetryInstrumentationKey, PatternLayout patternLayout)
        {
            Hierarchy h = (Hierarchy)log4net.LogManager.GetRepository();
            h.Root.Level = Level.All;
            h.Root.AddAppender(CreateAIAppender(TelemetryInstrumentationKey, patternLayout));
            h.Configured = true;
        }

        /// <summary>
        /// Creating Application Insights Appender
        /// </summary>
        /// <param name="TelemetryInstrumentationKey"></param>
        /// <param name="patternLayout"></param>
        private static IAppender CreateAIAppender(string TelemetryInstrumentationKey, PatternLayout patternLayout)
        {
            ApplicationInsightsAppender aIAppender = new ApplicationInsightsAppender();
            aIAppender.Name = "ApplicationInsightsAppender";
            aIAppender.InstrumentationKey = TelemetryInstrumentationKey;
            patternLayout.ActivateOptions();
            aIAppender.Layout = patternLayout;
            aIAppender.ActivateOptions();
            _aIAppender = aIAppender;

            return aIAppender;
        }

        /// <summary>
        /// Configuring Console Appender
        /// </summary>
        /// <param name="layout"></param>
        private static void ConfigureWithConsole(PatternLayout layout)
        {
            Hierarchy h = (Hierarchy)log4net.LogManager.GetRepository();
            h.Root.Level = Level.All;
            h.Root.AddAppender(CreateConsoleAppender(layout));
            h.Configured = true;
        }

        //Creating Console Appender
        private static IAppender CreateConsoleAppender(PatternLayout layout)
        {
            var appender = new ConsoleAppender();
            appender.Name = "ConsoleAppender";
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.ActivateOptions();
            return appender;
        }

        //Get Verbosity level
        private bool GetVerbosity()
        {
            return _initializer.Configuration.AppSettings.LogVerbosity;
        }

        public static void FlushApplicationInsightsTelemetry()
        {
            _aIAppender?.Flush(500);
        }
    }
}
