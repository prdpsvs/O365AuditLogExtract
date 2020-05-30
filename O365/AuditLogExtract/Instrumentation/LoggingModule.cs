using Autofac.Core;
using Autofac.Core.Registration;
using log4net;
using System.Linq;
using System.Reflection;

namespace AuditLogExtract.Instrumentation
{
    public class LoggingModule : Autofac.Module
    {
        /// <summary>
        /// Inject Logger Properties
        /// </summary>
        /// <param name="instance"></param>
        private static void InjectLoggerProperties(object instance)
        {
            var instanceType = instance.GetType();

            // Get all the injectable properties to set.
            // If you wanted to ensure the properties were only UNSET properties,
            // here's where you'd do it.
            var properties = instanceType
       .GetProperties(BindingFlags.Public | BindingFlags.Instance)
       .Where(p => p.PropertyType == typeof(ILog) && p.CanWrite && p.GetIndexParameters().Length == 0);

            // Set the properties located.
            foreach (var propToSet in properties)
            {
                propToSet.SetValue(instance, log4net.LogManager.GetLogger(instanceType), null);
            }
        }

        private static void OnComponentPreparing(object sender, PreparingEventArgs e)
        {
            e.Parameters = e.Parameters.Union(
             new[]
             {
                new ResolvedParameter(
                  (p, i) => p.ParameterType == typeof(ILog),
                  (p, i) => log4net.LogManager.GetLogger(p.Member.DeclaringType)
                ),
            });
        }

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            // Handle constructor parameters.
            registration.Preparing += OnComponentPreparing;

            // Handle properties.
            registration.Activated += (sender, e) => InjectLoggerProperties(e.Instance);
        }
    }
}
