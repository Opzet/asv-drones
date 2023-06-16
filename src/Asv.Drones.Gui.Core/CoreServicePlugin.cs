﻿using Asv.Cfg;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Asv.Drones.Gui.Uav;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// This module only imports all core services to be created by IoC container
    /// </summary>
    [PluginEntryPoint(Name, CorePlugin.Name)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CoreServicePlugin:IPluginEntryPoint
    {
        public const string Name = "CoreServices";
        private readonly CompositionContainer _container;
        

        [ImportingConstructor]
        public CoreServicePlugin(CompositionContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            // We need the plugin to ask the container to create services to make them work
            var svc1 = _container.GetExport<IConfiguration>();
            var svc2 = _container.GetExport<IAppService>();
            var svc3 = _container.GetExport<IThemeService>();
            var svc4 = _container.GetExport<INavigationService>();
            var svc5 = _container.GetExport<ILocalizationService>();
            var svc6 = _container.GetExport<IMapService>();
            var svc7 = _container.GetExport<IMavlinkDevicesService>();
        }

        public void OnFrameworkInitializationCompleted()
        {
            
        }

        public void OnShutdownRequested()
        {
            
        }
    }
}