using ServersideQoL.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ServersideQoL.PortalSigns;

[Processor(Id)]
public sealed class PortalSignProcessor : Processor<PortalSignProcessor.PrefabInfo>
{
  public const string Id = "057543ce-b9d3-43b2-beb4-1f9dd52b46c0";
  public sealed record PrefabInfo(TeleportWorld? TeleportWorld) : ProcessorPrefabInfo;

  readonly Dictionary<ServersideQoLZDO, SignState> _portalToSign = [];
  readonly Dictionary<ServersideQoLZDO, ServersideQoLZDO> _signToPortal = [];
  Regex? _includeRegex;
  Regex? _excludeRegex;
  bool _enabled;

  protected override void Initialize()
  {
    _enabled = Config.Instance.Enabled.Value;

    Logger.LogInfo($"Configured portal types: {string.Join(", ", Config.Instance.Advanced.Value.PortalSignOffsetsYaml.Keys)}");

    // Re-place signs live when the yaml is edited, no restart needed while tuning offsets.
    Config.Instance.Advanced.ValueChanged -= OnAdvancedConfigChanged;
    Config.Instance.Advanced.ValueChanged += OnAdvancedConfigChanged;

    foreach (var (portal, _) in _portalToSign)
      portal.Destroyed -= OnPortalDestroyed;
    _portalToSign.Clear();
    _signToPortal.Clear();
    // Signs are recreated by Process, no need to keep the old ones around.
    foreach (var zdo in PlacedObjects.ToArray())
      zdo.Destroy();

    var filter = Config.Instance.Include.Value.Trim();
    _includeRegex = string.IsNullOrEmpty(filter.Trim('*')) ? null : new(ConvertToRegexPattern(filter));
    filter = Config.Instance.Exclude.Value.Trim();
    _excludeRegex = string.IsNullOrEmpty(filter) ? null : new(ConvertToRegexPattern(filter));
  }

  protected override ProcessResult Process(ServersideQoLZDO zdo, IReadOnlyList<Peer> peers, PrefabInfo prefabInfo)
  {
    if (!_enabled || prefabInfo.TeleportWorld is null || zdo.IsModCreator())
      return ProcessResult.UnregisterProcessor;

    var cfg = Config.Instance;
    var tag = zdo.Vars.GetTag();
    if ((string.IsNullOrEmpty(tag) && cfg.SkipUntagged.Value) || !CheckFilter(tag))
    {
      RemoveSign(zdo);
      return ProcessResult.Default;
    }

    if (!_portalToSign.TryGetValue(zdo, out var state) || !PlacedObjects.Contains(state.Sign))
    {
      RemoveSign(zdo);
      state = CreateSign(zdo, tag, GetOffset(zdo));
    }
    else if (state.Tag != tag)
    {
      // Portal was renamed, keep the sign in sync.
      state.Sign.Vars.SetText(FormatText(tag));
      state.Tag = tag;
    }

    return ProcessResult.Default;
  }

  void RemoveSign(ServersideQoLZDO portal)
  {
    if (!_portalToSign.Remove(portal, out var state))
      return;
    _signToPortal.Remove(state.Sign);
    portal.Destroyed -= OnPortalDestroyed;
    if (PlacedObjects.Contains(state.Sign))
      DestroyObject(state.Sign);
  }

  void OnPortalDestroyed(ServersideQoLZDO portal) => RemoveSign(portal);

  void OnAdvancedConfigChanged(Config.YamlConfigEntry<Config.AdvancedConfig> _)
  {
    // The yaml was edited live. Re-place every sign whose configured offsets changed.
    // Done directly here instead of RequestReinitialization() so this works even when
    // the server runs an older framework build than the module was compiled against.
    foreach (var (portal, state) in _portalToSign.ToList())
    {
      var offset = GetOffset(portal);
      if (offset.Equals(state.Offset))
        continue;
      var tag = state.Tag;
      RemoveSign(portal);
      CreateSign(portal, tag, offset);
    }
  }

  SignState CreateSign(ServersideQoLZDO portal, string tag, Config.AdvancedConfig.PortalSignOffset offset)
  {
    var sign = PlaceSign(portal, tag, offset);
    var state = new SignState { Sign = sign, Tag = tag, Offset = offset };
    _portalToSign[portal] = state;
    _signToPortal[sign] = portal;
    portal.Destroyed += OnPortalDestroyed;
    return state;
  }

  static Config.AdvancedConfig.PortalSignOffset GetOffset(ServersideQoLZDO portal)
  {
    if (Config.Instance.Advanced.Value.PortalSignOffsets.TryGetValue(portal.ZDO.GetPrefab(), out var offset))
      return offset;
    return new Config.AdvancedConfig.PortalSignOffset();
  }

  bool CheckFilter(string tag)
  {
    if ((_includeRegex ?? _excludeRegex) is null)
      return true;

    return _includeRegex?.IsMatch(tag) is not false && _excludeRegex?.IsMatch(tag) is not true;
  }

  static string FormatText(string tag)
  {
    var cfg = Config.Instance;
    var text = cfg.TextFormat.Value.Replace("{0}", tag);
    if (cfg.TextColor.Value is { Length: > 0 } color)
      text = $"<color={color}>{text}";
    return text;
  }

  ServersideQoLZDO PlaceSign(ServersideQoLZDO portal, string tag, Config.AdvancedConfig.PortalSignOffset offset)
  {
    var pos = portal.ZDO.GetPosition();
    var rot = portal.ZDO.GetRotation();
    var signPos = pos + rot * new Vector3(offset.Right, offset.Up, offset.Forward);
    var sign = PlacePiece(signPos, Prefabs.Sign, rot.eulerAngles.y + offset.YawOffset);
    sign.Fields<WearNTear>().Set(static () => x => x.m_supports, false);
    sign.Vars.SetText(FormatText(tag));
    return sign;
  }

  sealed class SignState
  {
    public required ServersideQoLZDO Sign { get; set; }
    public required string Tag { get; set; }
    public required Config.AdvancedConfig.PortalSignOffset Offset { get; set; }
  }
}
