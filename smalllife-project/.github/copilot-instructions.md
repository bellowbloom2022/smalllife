# Project Guidelines

## Project Scope
- Unity game project; primary code is under Assets/Script and runtime assets/scenes under Assets.
- Use Unity Editor as the source of truth for scene, prefab, controller, and animation changes.
- Package baseline is defined in Packages/manifest.json (includes Cinemachine, TextMeshPro, Unity Test Framework, LeanLocalization dependencies).

## Architecture
- Core always-on singletons: GameManager, InputRouter, SaveSystem (DontDestroyOnLoad).
- Gameplay flow and level progress orchestration centers around Level, Goal, and GoalManager.
- Goal interactions are animation-event-driven (Step1/Step2). Review architecture notes before changing flow:
  - .github/notes/architecture/goal-step-system.md
  - .github/notes/architecture/input-system.md

## Build And Test
- Open with Unity Editor and run from the Build Settings first scene (currently Assets/Scenes/TitlePage.unity).
- Command-line build pattern (project has no dedicated BuildClass yet):
  - /Applications/Unity/Hub/Editor/<version>/Unity -batchmode -quit -projectPath <path> -executeMethod <BuildClass.Method>
- Tests use Unity Test Framework (com.unity.test-framework). Prefer Unity Test Runner unless a dedicated CLI test entry is added.

## Conventions
- Start gameplay logic work in Assets/Script, not generated csproj files.
- Keep asset references stable: do not hand-edit .meta GUID relationships.
- Resource loading convention is active in this project. Keep runtime-loaded assets inside Assets/Resources and verify load paths.
- Level data assets are expected at Resources/LevelDataAssets/{sceneName}.asset.
- Localization bootstrap happens in GameManager; avoid changes that skip LeanLocalization initialization.

## Critical Pitfalls
- InputRouter uses a single lock flag model, not a lock stack. Any LockInput must have a guaranteed UnlockInput path.
- Goal Step animation events must fire before clip end; event time beyond clip length can leave input locked.
- Animator transition ExitTime values can introduce user-visible delays in Step progression.
- If you touch input/goal flow, check ongoing issue notes first:
  - .github/notes/tasks/ongoing/goal-input-lock-bug.md

## Safe Editing Rules
- Do not edit generated or machine-local folders/files directly: Library, Temp, Logs, Obj.
- Prefer making rename/move operations for Unity assets in the Editor to preserve references.
- Treat Plugins as ABI-sensitive and avoid changes unless explicitly required.

## Fast Entry Points
- Assets/Script/Core/InputRouter.cs
- Assets/Script/Goals/Goal.cs
- Assets/Script/Player/GoalManager.cs
- Assets/Script/Manager/Level.cs
- Assets/Script/SaveSystem/SaveSystem.cs
- Assets/Script/UI/GoalNotePanelController.cs
- Assets/Scenes/Level2.unity

## Linked Project Notes
- Notes index: .github/notes/README.md
- Architecture details: .github/notes/architecture/goal-step-system.md, .github/notes/architecture/input-system.md
- Resolved implementation references: .github/notes/tasks/resolved/

Keep changes small and reversible, then validate behavior in Unity Editor Play Mode before finalizing.
