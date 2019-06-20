using System.Collections.Generic;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Ready.Framework.Versioning
{
    internal sealed class Host
    {
        private static volatile Host _currentHost;

        private static volatile object _lockObject = new object();

        private Host()
        {
            Bootsrap();
        }

        internal static Host Current
        {
            get
            {
                if (_currentHost != null)
                    return _currentHost;

                lock (_lockObject)
                {
                    if (_currentHost == null)
                        _currentHost = new Host();
                }

                return _currentHost;
            }
        }

        internal List<IServiceVersionFacade> ServiceModules { get; private set; }

        //private void Bootsrap()
        //{
        //    try
        //    {
        //        var path = ".\\bin";

        //        var directoryCatalog = new DirectoryCatalog(path, "*Service.v*.dll");
        //        using (var aggregateCatalog = new AggregateCatalog())
        //        {
        //            aggregateCatalog.Catalogs.Add(directoryCatalog);
        //            using (var componsitionContainer = new CompositionContainer(aggregateCatalog))
        //            {
        //                Expression<Func<ExportDefinition, bool>> constraint = ex => true;
        //                var importDefinition = new ImportDefinition(constraint, "", ImportCardinality.ZeroOrMore, true, true);
        //                var exports = componsitionContainer.GetExports(importDefinition);
        //                var modules = exports.Select(export => export.Value as IServiceVersionFacade).Where(m => m != null).ToList();
        //                ServiceModules = modules;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ServiceModules = new List<IServiceVersionFacade>();
        //    }
        //}

        private void Bootsrap()
        {
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = Path.Combine(Path.GetDirectoryName(executableLocation));
            var assemblies = Directory
                .GetFiles(path, "TFKB.BigBank.Service.*.dll", SearchOption.AllDirectories)
                .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                .ToList();
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);
            using (var container = configuration.CreateContainer())
            {
                ServiceModules = container.GetExports<IServiceVersionFacade>().ToList();
            }
        }
    }
}