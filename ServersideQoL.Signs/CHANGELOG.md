### v2.4.0
- Added option `TimeSignsDisplay` to show the analog clock emoji and the digital time, the analog clock emoji only, or the digital time only on time signs
- Added option `TimeSigns12Hour` to show the digital time on time signs in 12-hour format (e.g. 1:24 PM) instead of 24-hour format (13:24)
- Time signs now support per-sign display modes: append `:analog`, `:digital` or `:both` to a time emoji or the `TimeSignsPlaceholder` text to pin only the analog clock emoji, only the digital time, or both on that sign (overriding the `TimeSignsDisplay` option for it); `:auto` removes the pin and follows `TimeSignsDisplay` again
- Fixed `:digital` and the `DigitalOnly` display mode not updating after the first write: digital-only output now pins itself to the sign so it keeps ticking, including when a default sign color wraps the sign text

### v2.3.0
- Added option `TimeSignsPlaceholder` as an alternative means to turn a sign into a time sign when time emojis cannot be used (PS5) [#231](https://github.com/ArgusMagnus/ValheimServersideQoL/issues/231)

### v2.2.0
- Added option `FermenterSigns` to automatically put a sign on fermenters showing their content and remaining fermentation time [#293](https://github.com/ArgusMagnus/ValheimServersideQoL/pull/293) (contributed by [explosivo22](https://github.com/explosivo22))

### v2.0.11
- Support for the new core options `UnifiedConfig` and `ConfigPerWorld`

### v2.0.0
- Initial release