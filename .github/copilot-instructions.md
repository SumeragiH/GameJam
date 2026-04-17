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
- Fast C# compile check used in this repo: `dotnet build .\GameJam.sln -nologo`

## High-level architecture
- Runtime scripts are split into **System** and **View** layers under `Assets/Scripts`.
- URP 2D render pipeline is configured by:
  - `Assets/Settings/UniversalRP.asset`
  - `Assets/Settings/Renderer2D.asset`
- Fullscreen dimming outside selected regions is implemented with:
  - `Assets/Scripts/System/Rendering/DarkenOutsideRegionsRendererFeature.cs`
  - `Assets/Shader/DarkenOutsideRegions.shader`
  - `Assets/Settings/DarkenOutsideRegionsFeature.asset` (linked in `Renderer2D.asset`)
- Region data flow:
  - `RegionProviderBase` + concrete providers (`CircleRegionProvider`, `BoxRegionProvider`) produce region shape/position parameters
  - `RegionMaskManager` collects providers, filters by group mask, and uploads shader arrays each frame
  - renderer feature pass blits camera color through the shader; pixels outside all active regions are multiplied by outside brightness
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
- Sample scene integration:
  - `SampleScene` has a `RegionMaskManager` object under `-- SYSTEM --`
  - `SafeZone` uses `CircleRegionProvider` and trigger collider (`m_IsTrigger: 1`)
- Singleton base classes in `Assets/Scripts/System/Base` are shared infrastructure:
  - `SingletonBaseWithMono<T>` for persistent MonoBehaviour managers (`DontDestroyOnLoad`)
  - `SingletonBaseWithoutMono<T>` for plain C# singleton services

## Key conventions in this codebase
- **Folder-role convention**: gameplay orchestration in `System/`, scene-attached entities in `View/`.
- **Inspector-first fields**: private serialized fields use underscore prefix (`[SerializeField] private ... _name`) and are exposed via read-only properties where needed.
- **Event-driven gameplay state**: trigger callbacks in `CoverView` publish overlap events, and systems react to those events instead of polling collisions every frame.
- **Multi-collider safe handling**: a cover only emits enter on first player collider and exit on last player collider, preventing duplicate state toggles.
- **Screen-space mask convention**: providers convert world parameters to viewport-space (`WorldToViewportPoint`) before shader upload.
- **Region selection control**: which regions participate is controlled by provider enable state + provider group index + `RegionMaskManager` active group mask.
- **URP feature-first post effect**: new full-screen effects should prefer `ScriptableRendererFeature + ScriptableRenderPass` instead of per-camera hacks.
- **Framework-style extension points**: common behavior lives in abstract `CoverView`; concrete cover types override behavior instead of duplicating state logic.
- **Singleton usage pattern**: systems intended to be global use the shared singleton base types rather than ad-hoc static instances.
