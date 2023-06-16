using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Asv.Drones.Gui.Core;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Gbs;
using Asv.Drones.Gui.Sdr;
using Asv.Drones.Gui.Uav;
using NLog;
using Avalonia.Controls.Templates;

namespace Asv.Drones.Gui
{
    public partial class App : Application
    {
        private readonly CompositionContainer _container;
        private readonly Stack<KeyValuePair<IPluginMetadata, IPluginEntryPoint>> _plugins = new();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public App()
        {
            _container = new CompositionContainer(new AggregateCatalog(Catalogs().ToArray()), CompositionOptions.IsThreadSafe);
            // we need to export the container itself 
            var batch = new CompositionBatch();
            batch.AddExportedValue(_container);
            batch.AddExportedValue<IDataTemplateHost>(this);
            _container.Compose(batch);


            #region loading plugins entry points

            var plugins = _container.GetExports<IPluginEntryPoint, IPluginMetadata>().ToArray();
            var sort = plugins.ToDictionary(_=>_.Metadata.Name, _=>_.Metadata.Dependency);
            Logger.Info($"Begin loading plugins [{plugins.Length} items]");
            foreach (var name in DepthFirstSearch.Sort(sort))
            {
                try
                {
                    Logger.Trace($"Init {name}");
                    var plugin = plugins.First(_ => _.Metadata.Name == name);
                    var item = new KeyValuePair<IPluginMetadata, IPluginEntryPoint>(plugin.Metadata, plugin.Value);
                    _plugins.Push(item);
                    Logger.Debug($"Load plugin entry point '{plugin.Metadata.Name}' depended on [{string.Join(",", plugin.Metadata.Dependency)}]");
                }
                catch (Exception e)
                {
                    Logger.Error(e,$"Error to load plugin entry point: {name}:{e.Message}");
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }

            #endregion

        }

        private IEnumerable<Assembly> Assemblies()
        {
            //yield return GetType().Assembly;                   // Asv.Drones.Gui
            yield return typeof(CorePlugin).Assembly;            // Asv.Drones.Gui.Core
            yield return typeof(UavPlugin).Assembly;             // Asv.Drones.Gui.Uav
            yield return typeof(GbsPlugin).Assembly;             // Asv.Drones.Gui.Gbs
            yield return typeof(FlightSdrWidgetBase).Assembly;   // Asv.Drones.Gui.Sdr
            // This section is for private plugins
#if PROPRIETARY
            yield return typeof(Afis.AfisPlugin).Assembly;        // [Asv.Drones.Gui.Afis]
#endif
            

        }

        private IEnumerable<ComposablePartCatalog> Catalogs()
        {
            foreach (var asm in Assemblies().Distinct().Select(assembly => new AssemblyCatalog(assembly)))
            {
                yield return asm;
            }


#if DEBUG

#else
            // Enable this feature to load plugins from folder
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (dir != null)
            {
                var cat = new DirectoryCatalog(dir, "Asv.Drones.Gui.Plugins.*.dll");
                cat.Refresh();
                yield return cat;
            }
#endif
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            foreach (var plugin in _plugins)
            {
                try
                {
                    plugin.Value.Initialize();
                    Logger.Trace($"Initialize plugin entry point '{plugin.Key.Name}'");
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Error to initialize plugin entry point: {plugin.Key.Name}:{e.Message}");
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }
            
        }

        public override void OnFrameworkInitializationCompleted()
        {
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownRequested += (_, _) =>
                {
                    foreach (var plugin in _plugins)
                    {
                        try
                        {
                            plugin.Value.OnShutdownRequested();
                            Logger.Trace($"Call plugin {plugin.Key.Name}.{nameof(plugin.Value.OnShutdownRequested)}()");
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, $"Error to call {plugin.Key.Name}.{nameof(plugin.Value.OnShutdownRequested)}() at plugin entry point:{e.Message}");
                            if (Debugger.IsAttached)
                            {
                                Debugger.Break();
                            }
                        }
                    }
                    _container.Dispose();
                };
                var configuration = _container.GetExportedValue<IConfiguration>();
                var window = new MainWindow(configuration);
                var navigation = _container.GetExportedValue<INavigationService>();
                navigation?.InitStorageProvider(window.StorageProvider);
                window.DataContext = _container.GetExportedValue<ShellViewModel>();
                desktop.MainWindow = window;
            }

            base.OnFrameworkInitializationCompleted();

            foreach (var plugin in _plugins)
            {
                try
                {
                    plugin.Value.OnFrameworkInitializationCompleted();
                    Logger.Trace($"Call plugin {plugin.Key.Name}.{nameof(plugin.Value.OnFrameworkInitializationCompleted)}()");
                }
                catch (Exception e)
                {
                    Logger.Error($"Error to call {plugin.Key.Name}.{nameof(plugin.Value.OnFrameworkInitializationCompleted)}() at plugin entry point:{e.Message}");
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }
        }

    }
}
