using BepInEx.Configuration;
using YamlDotNet.Serialization;

namespace ServersideQoL.PortalSigns;

public sealed class Config(ConfigFile cfg, Logger logger) : ConfigBase<Config>(cfg, logger)
{
  public override ConfigEntry<bool> Enabled { get; } = BindEx(cfg, true, """
    Enables/disables the entire mod.
    True to automatically place a sign above each portal showing the portal's tag.
    The sign text is kept in sync when the portal is renamed and the sign is removed when the portal is destroyed.
    """);

  public ConfigEntry<string> Include { get; } = BindEx(cfg, "*",
    "Only portals with a tag that matches this filter get a sign ('*' and '?' wildcards are supported)");
  public ConfigEntry<string> Exclude { get; } = BindEx(cfg, "",
    "Portals with a tag that matches this filter do not get a sign ('*' and '?' wildcards are supported)");
  public ConfigEntry<bool> SkipUntagged { get; } = BindEx(cfg, true,
    "True to skip portals without a tag");
  public ConfigEntry<string> TextFormat { get; } = BindEx(cfg, "{0}",
    "Sign text, {0} is replaced with the portal tag");
  public ConfigEntry<string> TextColor { get; } = BindEx(cfg, "white",
    "Color of the sign text: a color name (white, red, blue, green, yellow, orange, purple, black) or hex value (#FFF, #FFFFFF). Empty for the default sign color.");

  public YamlConfigEntry<AdvancedConfig> Advanced { get; } = BindYaml<AdvancedConfig>(cfg, static x => x.Reset());

  public sealed class AdvancedConfig
  {
    public sealed record PortalSignOffset(float Right, float Up, float Forward, float YawOffset)
    {
      public PortalSignOffset() : this(-1.1f, 2.5f, 0.25f, 0f) { }
    }

    [YamlMember(Alias = nameof(PortalSignOffsets))]
    internal Dictionary<string, PortalSignOffset> PortalSignOffsetsYaml { get; init; } = new()
    {
      ["portal_wood"] = new(-1.1f, 2.5f, 0.25f, 0f),
      ["portal_stone"] = new(-2.1f, 1.1f, 0.7f, 0f),
    };

    [YamlIgnore]
    public IReadOnlyDictionary<int, PortalSignOffset> PortalSignOffsets
    {
      get => field ??= PortalSignOffsetsYaml.ToDictionary(static x => x.Key.GetStableHashCode(), static x => x.Value);
      private set => field = value;
    }

    internal void Reset() => PortalSignOffsets = default!;
  }
}
