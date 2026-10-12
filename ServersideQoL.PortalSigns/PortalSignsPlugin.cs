using BepInEx.Configuration;

namespace ServersideQoL.PortalSigns;

partial class PortalSignsPlugin : ServersideQoLPluginBase<PortalSignsPlugin, Config>
{
  protected override Config CreateConfigSingleton(ConfigFile configFile, Logger logger)
    => new(configFile, logger);

  protected override void RegisterProcessors(IProcessorCollection processors) => processors
    .Add<PortalSignProcessor>();
}
