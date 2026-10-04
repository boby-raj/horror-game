# Horror Game Project

## Overview
This is a Unity 3D First-Person Horror Game. It features a modular player controller, a raycast-based interaction system, NavMesh-driven enemy AI with jumpscares, and puzzle mechanics (such as collecting keys and body parts).

## Script Architecture Overview
- **Player Movement**: First Person Controller components (`FirstPersonMovement.cs`, `FirstPersonLook.cs`, etc.)
- **Interaction**: Raycast-based interactions (`PlayerInteraction.cs`, `IInteractable.cs`)
- **Enemy AI**: NavMesh-driven enemies with jumpscare mechanics (`enemychase.cs`, `kito.cs`, `followscript.cs`)
- **Puzzles & Items**: Collecting objects to progress (`WallPuzzleManager.cs`, `key.cs`, etc.)
- **Game State**: PlayerPrefs-based checkpoint saving (`GameSaveSystem.cs`, `PlayerSaveManager.cs`)

---

## Changelog
*This section tracks all modifications made to the project to maintain continuity across sessions.*

### [2026-09-25]
- Initial creation of the `README.md` file to track future changes.
- Completed full read-only analysis of the project's 65 C# scripts.

### [2026-09-25] - Controller Refactor
- Consolidated all player controller scripts (FirstPersonLook.cs, Jump.cs, Crouch.cs, GroundCheck.cs, Zoom.cs) into a single, unified FirstPersonMovement.cs script.
- Fixed physics desyncs (moved input to Update, physics to FixedUpdate).
- Added headroom checking for crouching to prevent clipping into ceilings.
- Improved ground detection by switching from a single Raycast to a SphereCast with LayerMask support.
- Replaced direct linear velocity overrides with Rigidbody.AddForce (ForceMode.VelocityChange) so the player can be affected by external forces like knockbacks.
- Updated FirstPersonAudio.cs to correctly interface with the newly unified FirstPersonMovement.cs.
- Deleted obsolete component scripts.
- Updated key.cs to instantly disable all child renderers and colliders upon pickup, so the key visually disappears immediately rather than waiting for the 2.5s text delay.
- Updated key.cs so that the key is only picked up when pressing the Interaction button ('E') instead of automatically on hover.
- Increased interactionDistance in PlayerInteraction.cs from 3.0 to 6.0 so the player can interact with objects from further away.

### [2026-10-03 & 2026-10-04] - UI, Lighting & Performance Updates
- Added `hoverText` and `inventoryIcon` fields to `key.cs` and `box_open_withkey.cs` to show "Press E" prompts and top-right key inventory icons.
- Removed unused Cinemachine and PlayableDirector intro logic from `OpeningManager.cs`.
- Removed legacy `OnGUI()` timer rendering from `lever_script.cs` for better frame rate performance.
- Added Editor utilities (`LightingFixer.cs` and `UIPromptFixer.cs`) under `Assets/Editor/` to configure Mixed baked lighting and automatically disable stray active UI prompt texts.

