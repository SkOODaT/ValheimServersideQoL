<details open><summary><b>Signs</b></summary>

|Option|Default Value|Acceptable Values|Description|
|------|-------------|-----------------|-----------|
|Enabled|true|True/False|Enables/disables the entire mod|
|DefaultColor|black|black, blue, green, orange, purple, red, white, yellow|Default color for signs|
|TimeSigns|true|True/False|True to update sign texts which contain time emojis (any of 🕛🕧🕐🕜🕑🕝🕒🕞🕓🕟🕔🕠🕕🕡🕖🕢🕗🕣🕘🕤🕙🕥🕚🕦) with the in-game time|
|TimeSignsPlaceholder|||Optional text that will be recognized in addition to time emojis and replaced with the in-game time on signs. Append :analog, :digital or :both to a time emoji or the placeholder to pin that display mode on the sign (overriding TimeSignsDisplay for it); :auto removes the pin and follows TimeSignsDisplay again|
|TimeSignsDisplay|AnalogAndDigital|AnalogAndDigital, AnalogOnly, DigitalOnly|What time signs show: the analog clock emoji and the digital time, the analog clock emoji only, or the digital time only|
|TimeSigns12Hour|false|True/False|True to show the digital time on time signs in 12-hour format (e.g. 1:24 PM) instead of 24-hour format (13:24)|
|FermenterSigns|false|True/False|True to automatically put a sign on the tap side of fermenters, showing the content and the remaining fermentation time|
