<details open><summary><b>PortalSigns</b></summary>

|Option|Default Value|Acceptable Values|Description|
|------|-------------|-----------------|-----------|
|Enabled|true|True/False|Enables/disables the entire mod. <br>True to automatically place a sign above each portal showing the portal's tag. <br>The sign text is kept in sync when the portal is renamed and the sign is removed when the portal is destroyed.|
|Include|*||Only portals with a tag that matches this filter get a sign ('*' and '?' wildcards are supported)|
|Exclude|||Portals with a tag that matches this filter do not get a sign ('*' and '?' wildcards are supported)|
|SkipUntagged|true|True/False|True to skip portals without a tag|
|TextFormat|{0}||Sign text, {0} is replaced with the portal tag|
|TextColor|white||Color of the sign text: a color name (white, red, blue, green, yellow, orange, purple, black) or hex value (#FFF, #FFFFFF). Empty for the default sign color.|
</details>
