using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ServersideQoL.Signs;


[Processor(Id)]
public sealed class SignProcessor : Processor<ProcessorPrefabInfo<Sign>>
{
  public const string Id = "806bdb85-c857-4154-a246-a0b1d0917987";
  internal static IReadOnlyList<string> ClockEmojis { get; } = ["🕛", "🕧", "🕐", "🕜", "🕑", "🕝", "🕒", "🕞", "🕓", "🕟", "🕔", "🕠", "🕕", "🕡", "🕖", "🕢", "🕗", "🕣", "🕘", "🕤", "🕙", "🕥", "🕚", "🕦"];
  Regex _clockRegex = default!;
  Regex _bareTimeRegex = default!;

  internal Regex DefaultColorRegex { get; } = new(@"<color=[^>]+ d>");

  /// <summary>
  /// ZDO key holding a sign's pinned display mode. Set when a :analog/:digital/:both suffix is used,
  /// and whenever digital-only output is written (its trigger is consumed by the write, so the pin
  /// is what keeps the sign ticking). The pin is what keeps the mode across refreshes.
  /// </summary>
  const string DisplayModeZdoKey = "ServersideQoL.Signs.DisplayMode";

  Dictionary<string, string>? _timeTexts;

  [MemberNotNull(nameof(_clockRegex), nameof(_bareTimeRegex))]
  protected override void Initialize()
  {
    var placeholder = Config.Instance.TimeSignsPlaceholder.Value;
    var emojiPattern = string.Join("|", ClockEmojis.Select(Regex.Escape));
    var triggerPattern = string.IsNullOrEmpty(placeholder)
      ? emojiPattern
      : $@"{emojiPattern}|{Regex.Escape(placeholder)}";
    // A trigger with an optional per-sign display mode suffix (e.g. ":analog", ":digital", ":both", ":auto"),
    // followed by an optional previously-written time, in 24-hour ("13:24") or 12-hour ("1:24 PM") format.
    _clockRegex = new($@"(?:{triggerPattern})(?::(\w+))?(?:\s*\d{{1,2}}\:\d\d(?:\s*[AP]M)?)?");
    // A sign whose entire text is just a previously-written digital time (pinned digital-only signs),
    // optionally wrapped in the default-color tag.
    _bareTimeRegex = new(@"^(?<prefix><color=[^>]+>\s*)?\d{1,2}\:\d\d(?:\s*[AP]M)?\s*$");
  }

  protected override void PreProcess(PeersEnumerable peers)
  {
    _timeTexts = null;
  }

  static Dictionary<string, string> BuildTimeTexts()
  {
    var cfg = Config.Instance;
    var dayFraction = EnvMan.instance.GetDayFraction();
    var emojiIdx = (int)Math.Floor(ClockEmojis.Count * 2 * dayFraction) % ClockEmojis.Count;
    var time = TimeSpan.FromDays(dayFraction);
    var digitalTime = cfg.TimeSigns12Hour.Value
      ? $"{(time.Hours % 12 == 0 ? 12 : time.Hours % 12)}:{time.Minutes:D2} {(time.Hours < 12 ? "AM" : "PM")}"
      : $@"{time:hh\:mm}";
    return new()
    {
      [Config.TimeSignsDisplayAnalogAndDigital] = $"{ClockEmojis[emojiIdx]} {digitalTime}",
      [Config.TimeSignsDisplayAnalogOnly] = $"{ClockEmojis[emojiIdx]}",
      [Config.TimeSignsDisplayDigitalOnly] = digitalTime,
    };
  }

  protected override ProcessResult Process(ServersideQoLZDO zdo, IReadOnlyList<Peer> peers, ProcessorPrefabInfo<Sign> prefabInfo)
  {
    var cfg = Config.Instance;
    var result = ProcessResult.Default;
    var text = zdo.Vars.GetText();
    if (cfg.TimeSigns.Value)
    {
      var matched = false;
      var newText = _clockRegex.Replace(text, match =>
      {
        matched = true;
        result = ScheduleReprocessing(Config.Instance.Advanced.Value.ProcessingDelays.TimeSigns);
        _timeTexts ??= BuildTimeTexts();
        var suffix = match.Groups[1].Value.ToLowerInvariant();
        var pinned = zdo.ZDO.GetString(DisplayModeZdoKey, "");
        string display = suffix switch
        {
          "analog" => Config.TimeSignsDisplayAnalogOnly,
          "digital" => Config.TimeSignsDisplayDigitalOnly,
          "both" => Config.TimeSignsDisplayAnalogAndDigital,
          "auto" => "",
          _ => pinned,
        };
        if (display is "")
        {
          // ":auto" or no pin: follow the global option (and drop any stale pin).
          if (pinned is not "")
            zdo.ZDO.Set(DisplayModeZdoKey, "");
          display = cfg.TimeSignsDisplay.Value;
        }

        if (suffix is "analog" or "digital" or "both" || display == Config.TimeSignsDisplayDigitalOnly)
        {
          // Pin the mode so it survives the trigger being consumed: explicit suffixes pin their mode,
          // and digital-only output pins itself however it was chosen (its trigger never survives the write).
          zdo.ZDO.Set(DisplayModeZdoKey, display);
        }
        if (!_timeTexts.TryGetValue(display, out var timeText))
          timeText = _timeTexts[Config.TimeSignsDisplayAnalogAndDigital];
        return timeText;
      });

      if (!matched)
      {
        var pinned = zdo.ZDO.GetString(DisplayModeZdoKey, "");
        var bareMatch = pinned is "" ? null : _bareTimeRegex.Match(text);
        if (bareMatch is { Success: true })
        {
          // Pinned digital-only sign: the trigger was consumed when the time was written, keep it ticking.
          _timeTexts ??= BuildTimeTexts();
          if (!_timeTexts.TryGetValue(pinned, out var timeText))
            timeText = _timeTexts[Config.TimeSignsDisplayAnalogAndDigital];
          var newBareText = $"{bareMatch.Groups["prefix"].Value}{timeText}";
          if (newBareText != text)
            zdo.Vars.SetText(text = newBareText);
          result = ScheduleReprocessing(Config.Instance.Advanced.Value.ProcessingDelays.TimeSigns);
        }
        else if (pinned is not "")
        {
          // No time trigger left on this sign; drop the stale pin.
          zdo.ZDO.Set(DisplayModeZdoKey, "");
        }
      }
      else if (newText != text)
      {
        zdo.Vars.SetText(text = newText);
        //zdo.Set(ZDOVars.s_author, );
      }
    }

    {
      var found = false;
      var newText = DefaultColorRegex.Replace(text, match =>
      {
        found = true;
        if (Config.Instance.DefaultColor.Value is { Length: > 0 } and not Config.NotSetDefaultColor)
          return $"<color=\"{Config.Instance.DefaultColor.Value}\" d>";
        return "";
      }, 1);

      if (!found && Config.Instance.DefaultColor.Value is { Length: > 0 } and not Config.NotSetDefaultColor)
        newText = $"<color=\"{Config.Instance.DefaultColor.Value}\" d>{text}";

      if (newText != text)
        zdo.Vars.SetText(text = newText);
    }

    return result;
  }
}
