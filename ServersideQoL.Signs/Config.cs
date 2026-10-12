using BepInEx.Configuration;

namespace ServersideQoL.Signs;

public sealed class Config(ConfigFile cfg, Logger logger) : ConfigBase<Config>(cfg, logger)
{
  internal const string NotSetDefaultColor = "black";
  internal const string TimeSignsDisplayAnalogAndDigital = "AnalogAndDigital";
  internal const string TimeSignsDisplayAnalogOnly = "AnalogOnly";
  internal const string TimeSignsDisplayDigitalOnly = "DigitalOnly";

  public override ConfigEntry<bool> Enabled { get; } = BindEx(cfg, true,
    "Enables/disables the entire mod");
  public ConfigEntry<string> DefaultColor { get; } = BindEx(cfg, NotSetDefaultColor,
    "Default color for signs", new AcceptableValueList<string>([
      // https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichTextColor.html (maybe obsolete)
      // https://docs.unity3d.com/Manual/UIE-supported-tags.html (doesn't mention supported color names)
      "black", "blue", "green", "orange", "purple", "red", "white", "yellow"]));
  public ConfigEntry<bool> TimeSigns { get; } = BindEx(cfg, true,
    Invariant($"True to update sign texts which contain time emojis (any of {string.Concat(SignProcessor.ClockEmojis)}) with the in-game time"));
  public ConfigEntry<string> TimeSignsPlaceholder { get; } = BindEx(cfg, "",
    "Optional text that will be recognized in addition to time emojis and replaced with the in-game time on signs. Append :analog, :digital or :both to a time emoji or the placeholder to pin that display mode on the sign (overriding TimeSignsDisplay for it); :auto removes the pin and follows TimeSignsDisplay again");
  public ConfigEntry<string> TimeSignsDisplay { get; } = BindEx(cfg, TimeSignsDisplayAnalogAndDigital,
    "What time signs show: the analog clock emoji and the digital time, the analog clock emoji only, or the digital time only",
    new AcceptableValueList<string>([TimeSignsDisplayAnalogAndDigital, TimeSignsDisplayAnalogOnly, TimeSignsDisplayDigitalOnly]));
  public ConfigEntry<bool> TimeSigns12Hour { get; } = BindEx(cfg, false,
    "True to show the digital time on time signs in 12-hour format (e.g. 1:24 PM) instead of 24-hour format (13:24)");
  public ConfigEntry<bool> FermenterSigns { get; } = BindEx(cfg, false,
    "True to automatically put a sign on the tap side of fermenters, showing the content and the remaining fermentation time");

  public YamlConfigEntry<AdvancedConfig> Advanced { get; } = BindYaml<AdvancedConfig>(cfg);

  public sealed class AdvancedConfig
  {
    public ProcessingDelaysConfig ProcessingDelays { get; init; } = new();

    public sealed class ProcessingDelaysConfig
    {
      public float TimeSigns { get; init; } = 0.5f;
    }
  }
}
