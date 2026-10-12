### v1.1.8
- New default offsets tuned by the user (wood: Right -1.1, Up 2.5 / stone: Right -2.1, Up 1.1, Forward 0.7)

### v1.1.7
- Fixed live offset updates throwing `MissingMethodException` when the server runs an older framework build: the yaml change handler now re-positions signs directly instead of calling `Processor.RequestReinitialization()`

### v1.1.6
- Reverted to v1.1.4 behavior: v1.1.5's self-refresh approach broke text coloring and still didn't move signs live, so it's removed until the cause is found

### v1.1.5
- Live offset updates no longer depend on the yaml file watcher: the mod now checks the yaml file itself every few seconds, so offset/color changes apply within ~5 seconds even on dedicated servers where file system events don't fire

### v1.1.4
- Added `TextColor` config option for the sign text color (replaces the hardcoded white default)

### v1.1.3
- Editing the Advanced yaml now re-positions existing signs live, no restart needed

### v1.1.2
- Log the configured portal types on startup (diagnostic)

### v1.1.1
- Removed the unused `portal` entry from the default offsets (only `portal_wood` and `portal_stone` remain)

### v1.1.0
- Sign offsets are now configured per portal type (`portal_wood`, `portal`, `portal_stone`) in the Advanced yaml

### v1.0.0
- Initial release
