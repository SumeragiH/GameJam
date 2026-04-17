# Copilot Instructions for this Repository

## Project snapshot
- Unity project for **腾讯 72H 开发大赛** (from `README.md`).
- Unity Editor version is pinned in `ProjectSettings/ProjectVersion.txt`: **2022.3.62f3c1**.
- 2D game

## Build, test, and lint commands
Use Unity Editor CLI (Windows examples):

```powershell
# Open/import/compile the project in batch mode (no custom build entry point currently in repo)
"<UnityEditorPath>\Unity.exe" -batchmode -quit -projectPath "E:\.useful\code\unity\tencent_gamejam\GameJam" -logFile "E:\.useful\code\unity\tencent_gamejam\GameJam\Logs\compile.log"

# Run all EditMode tests
"<UnityEditorPath>\Unity.exe" -batchmode -quit -projectPath "E:\.useful\code\unity\tencent_gamejam\GameJam" -runTests -testPlatform editmode -testResults "E:\.useful\code\unity\tencent_gamejam\GameJam\Logs\editmode-results.xml" -logFile "E:\.useful\code\unity\tencent_gamejam\GameJam\Logs\editmode-tests.log"

# Run all PlayMode tests
"<UnityEditorPath>\Unity.exe" -batchmode -quit -projectPath "E:\.useful\code\unity\tencent_gamejam\GameJam" -runTests -testPlatform playmode -testResults "E:\.useful\code\unity\tencent_gamejam\GameJam\Logs\playmode-results.xml" -logFile "E:\.useful\code\unity\tencent_gamejam\GameJam\Logs\playmode-tests.log"

# Run a single test (example filter format)
"<UnityEditorPath>\Unity.exe" -batchmode -quit -projectPath "E:\.useful\code\unity\tencent_gamejam\GameJam" -runTests -testPlatform editmode -testFilter "Namespace.ClassName.TestMethodName" -testResults "E:\.useful\code\unity\tencent_gamejam\GameJam\Logs\single-test.xml" -logFile "E:\.useful\code\unity\tencent_gamejam\GameJam\Logs\single-test.log"
```

Notes:
- `com.unity.test-framework` is included in `Packages/manifest.json`.
- There is currently no dedicated lint config/tooling in-repo (`.editorconfig`/`.ruleset` not present).

## High-level architecture
- Runtime scripts are split into **System** and **View** layers under `Assets/Scripts`.
- `Assets/Scripts/View/CoverView.cs` is the trigger entry point for cover detection:
  - emits `PlayerEnteredCover` / `PlayerExitedCover` events from trigger callbacks
  - handles multi-collider player overlap safely inside each cover
  - filters player by tag (`_playerTag`, default `Player`)
- `SafeZoneCover` (`Assets/Scripts/View/CoverViews/SafeZoneCover.cs`) is a `CoverView` specialization for always-on safe areas.
- `SafeZoneSystem` (`Assets/Scripts/System/SafeZoneSystem.cs`) discovers `SafeZoneView` objects in scene and syncs their covers to `CoverSystem`.
- `CoverSystem` (`Assets/Scripts/System/CoverSystem.cs`) is the gameplay state owner:
  - keeps registered covers (`currentSceneCoverView` + `safezoneCoverViews`) and reacts to `CoverView` enter/exit events
  - tracks currently overlapped covers with event-driven updates (no per-frame bounds scanning)
  - exposes state via `IsPlayerInCover`, `ActiveCovers`, `SafeZoneStateChanged`, and `IsInSafeZone()`
- Lighting is authored directly on scene/prefab objects (no dedicated runtime `LightingSystem` controller script in current codebase).
- Singleton base classes in `Assets/Scripts/System/Base` are shared infrastructure:
  - `SingletonBaseWithMono<T>` for persistent MonoBehaviour managers (`DontDestroyOnLoad`)
  - `SingletonBaseWithoutMono<T>` for plain C# singleton services

## Key conventions in this codebase
- **Folder-role convention**: gameplay orchestration in `System/`, scene-attached entities in `View/`.
- **Inspector-first fields**: private serialized fields use underscore prefix (`[SerializeField] private ... _name`) and are exposed via read-only properties where needed.
- **Event-driven gameplay state**: trigger callbacks in `CoverView` publish overlap events, and systems react to those events instead of polling collisions every frame.
- **Multi-collider safe handling**: a cover only emits enter on first player collider and exit on last player collider, preventing duplicate state toggles.
- **Prefab/scene-owned light setup**: visual lighting is configured on prefab/scene components, not generated/switched by centralized light-profile code.
- **Framework-style extension points**: common behavior lives in abstract `CoverView`; concrete cover types override behavior instead of duplicating state logic.
- **Singleton usage pattern**: systems intended to be global use the shared singleton base types rather than ad-hoc static instances.
