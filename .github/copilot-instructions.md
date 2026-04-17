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
- `Assets/Scripts/View/CoverView.cs` is the core abstraction for cover/safe-zone behavior:
  - abstract `ActivateCover()`
  - cooldown state (`_cooldownTime`, `_currentCooldown`)
  - overridable readiness (`IsReady`)
- `SafeZoneCover` (`Assets/Scripts/View/CoverViews/SafeZoneCover.cs`) is a `CoverView` specialization intended to be always active in safe zones.
- `CoverSystem` (`Assets/Scripts/System/CoverSystem.cs`) is intended to own:
  - scene-level `CoverView` (`currentSceneCoverView`)
  - multiple safe-zone covers (`List<SafeZoneCover>`)
  - final safety query (`IsInSafeZone()`), currently TODO.
- `SafeZoneSystem` (`Assets/Scripts/System/SafeZoneSystem.cs`) is intended to discover scene `SafeZoneView` instances and sync them to `CoverSystem` (TODO in code).
- Singleton base classes in `Assets/Scripts/System/Base` are shared infrastructure:
  - `SingletonBaseWithMono<T>` for persistent MonoBehaviour managers (`DontDestroyOnLoad`)
  - `SingletonBaseWithoutMono<T>` for plain C# singleton services

## Key conventions in this codebase
- **Folder-role convention**: gameplay orchestration in `System/`, scene-attached entities in `View/`.
- **Inspector-first fields**: private serialized fields use underscore prefix (`[SerializeField] private ... _name`) and are exposed via read-only properties where needed.
- **Framework-style extension points**: common behavior lives in abstract `CoverView`; concrete cover types override behavior instead of duplicating state logic.
- **Singleton usage pattern**: systems intended to be global use the shared singleton base types rather than ad-hoc static instances.
- **Implementation status is signaled inline**: TODO comments in `CoverSystem` and `SafeZoneSystem` are the source of truth for incomplete wiring between systems.
