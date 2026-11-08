# Secret Agent Shooter Starter

Unity starter kit for a 3D spy shooter where the player can switch between first-person and third-person, infiltrate missions, capture or neutralize a femme fatale syndicate, recruit operatives, and spend mission currency upgrading a large secret lair.

## What Is Included

- A 20-contract campaign with sequential unlocks, replayable missions, briefing pages, mixed objectives, checkpoints, extraction, and graded debriefs.
- A multistory secret lair with an exterior courtyard, central operations hub, elevator, dedicated armory, research, medical, training, customization, intelligence, recruit, and mech-garage rooms.
- Eight mission environment families with distinct routes, rooms, floors, security systems, sky/fog/lighting profiles, and environment dressing.
- Original Blender-authored player, first-person arms, four weapons, six enemy-role accessories, and a 36-piece modular environment/prop library.
- First-/third-person movement, weapon switching, finite magazines and reserves, reloads, crouching, melee, carrying, throwing, recruitment, companion deployment, and a tactical map.
- Distance-based suspicion, localized gunshot and footstep investigation, body discovery, security cameras, keycard vaults, reinforcements, and facility alarm lighting.

## Suggested Unity Version

Use Unity 2022 LTS or newer. The scripts are plain C# MonoBehaviours and avoid extra packages except optional `UnityEngine.AI` for companion following.

## Quick Start

1. Create a new 3D Unity project.
2. Copy the `Assets` folder from this package into your Unity project.

For the current nested installation, close Play Mode and run:

```powershell
.\SYNC_TO_UNITY.ps1 -UnityProjectPath "C:\Users\piefi\Downloads\My project"
```

The script overlays `Assets/SecretAgentShooterStarter` without deleting unrelated project files.
3. In Unity, create an empty scene.
4. Create an empty GameObject named `PrototypeSceneBuilder`.
5. Add the `PrototypeSceneBuilder` component from `Assets/Scripts/Prototype`.
6. Press Play.

The builder creates the persistent secret lair first. Use its central mission terminal to select an unlocked contract; the selected mission is then assembled with its environment, modeled player and weapons, enemies, objectives, supplies, captive drop-off, and return terminal.

## Default Controls

- `WASD`: Move
- `Mouse`: Look
- `Left Mouse`: Fire weapon
- `R`: Reload weapon; throw a carried operative when shoulder carrying
- `Space`: Jump
- `Left Shift`: Sprint
- `Left Ctrl`: Crouch for quieter, lower-visibility movement
- `V`: Toggle first-person / third-person
- `E`: Interact; perform a silent takedown from behind an unaware non-commander
- `B`: Toggle carry style between bridal carry and over-shoulder
- `G`: Drop carried captive
- `C`: Deploy an available recruited operative
- `1`-`4` or `Mouse Wheel`: Switch owned weapons
- `M`: Toggle the tactical map
- `A` / `D` or `Left Arrow` / `Right Arrow`: Change difficulty at the mission terminal

## Unity Package Validation

After synchronizing the durable package and allowing Unity to finish importing, run `Secret Agent > Validate Durable Package`. The validator checks the 20-contract campaign chain and confirms that every required character, weapon, role-gear, and environment model can be loaded from `Resources`.

## Content Direction

The recruitment loop is designed as nonlethal spy extraction: knock out an adult enemy operative, carry her to an extraction point, and she may defect after debriefing. Keep customization tactical and character-focused: outfits, armor silhouettes, combat roles, colors, and gadgets.

## Folder Map

- `Assets/Scripts/Player`: player controller and interaction interface.
- `Assets/Scripts/Weapons`: weapon definitions and tranquilizer/projectile logic.
- `Assets/Scripts/Characters`: enemy operatives, ranks, specializations, companions.
- `Assets/Scripts/Missions`: objective tracking and extraction.
- `Assets/Scripts/Base`: secret lair currency, room upgrades, terminals, roster.
- `Assets/Scripts/Items`: modeled field pickups and enemy loot logic.
- `Assets/Scripts/Prototype`: runtime hub/mission construction, presentation, security, audio, doors, checkpoints, and alarm systems.
