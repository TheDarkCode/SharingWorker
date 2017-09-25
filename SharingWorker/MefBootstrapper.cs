using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;

namespace SharingWorker
{
    class MefBootstrapper : BootstrapperBase
    {
        private static CompositionContainer container;

        public MefBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new CompositionContainer(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));
            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new MahWindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);

            container.Compose(batch);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = container.GetExportedValues<object>(contract);
            
            if (exports.Any())
                return exports.First();

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetExecutingAssembly() };
        }  

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }

        public static IEnumerable<T> GetAllInstances<T>(string key = null)
        {
            var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(typeof(T)) : key;
            return container.GetExportedValues<T>(contract);
        }

        public static IEnumerable<Lazy<T, TMetadata>> GetAllInstances<T, TMetadata>(string key = null)
        {
            var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(typeof(T)) : key;
            return container.GetExports<T, TMetadata>(contract);
        }
    }
}