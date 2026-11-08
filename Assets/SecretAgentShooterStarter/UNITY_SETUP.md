# Unity Setup Notes

## Input

The starter scripts support both Unity input modes:

- New Input System package: `Keyboard.current` and `Mouse.current`
- Old Input Manager: `Input.GetAxis`, `Input.GetKey`, and mouse buttons

The compatibility layer is `Assets/Scripts/Player/SpyInput.cs`. If your Unity project is set to `Input System Package (New)` in Player Settings, the prototype should run without the `UnityEngine.Input` error.

## Scene Setup Without Prototype Builder

Create:

- Player GameObject with:
  - `CharacterController`
  - `SpyPlayerController`
  - `SpyWeaponController`
- Two cameras:
  - Assign one to `firstPersonCamera`
  - Assign one to `thirdPersonCamera`
- Empty child transform named `CarrySocket` near the player chest/shoulder.
- Enemy GameObjects with:
  - Collider
  - `EnemyOperative`
- Extraction point with:
  - Collider
  - `ExtractionZone`
- Lair manager:
  - Empty GameObject with `SecretLairManager`
- Roster manager:
  - Empty GameObject with `RecruitRoster`
- Mission manager:
  - Empty GameObject with `MissionManager`

## Recommended Layers

- Player
- Enemy
- Interactable
- Pickup
- Level

## Animation Hooks To Add Later

- Player idle, walk, sprint, jump.
- First-person weapon raise/fire/reload.
- Third-person aim/fire.
- Knockout fall.
- Bridal carry loop.
- Over-shoulder carry loop.
- Captive placed into extraction.
- Companion follow, take cover, attack.
