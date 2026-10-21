# DEBUG/Test — Ground and air visual prototype

## Pass 2 (2026-10-21)

This remains a contained experiment inside DEBUG/Test. The existing ground tower, route and enemy remain independent of the new flying-unit experiment.

### Run

Open the repository in **Godot 4.7.x .NET**, build the C# project, and press **F5**, or open `res://DEBUG/Test/TestWorld.tscn` and press **F6**.

The scene now features a wider **156 × 96** desert platform, a substantially longer winding road with **13 route markers**, terrain rocks, a ground-only turret and a separate **anti-air-only dual-barrel tower** that rotates horizontally and raises its barrels to attack aircraft.

Flying drones start outside the **west, north or south** map edges, at randomized horizontal locations and heights. They follow a direct 3D trajectory toward the Citadel, with gentle sideways weaving, altitude bobbing and banking. They need no A*, NavMesh or terrain collision checks. The anti-air tower cannot target ground drones, and the original turret remains ground-only.

### Controls

| Key | Action |
| --- | --- |
| Arrow keys | Pan across battlefield, relative to screen |
| Shift + arrow | Pan faster |
| Q / E | Tilt camera up / down |
| 1 / 2 / 3 | Queue 1 / 10 / 50 **ground** drones |
| 4 / 5 / 6 | Queue 1 / 10 / 50 **flying** drones |
| F2 / F3 / F4 | Additional +10 ground / +50 ground / +10 flying shortcuts |
| V | Toggle ground route variation for new ground enemies |
| R | Reset both sets of enemies and their counters |

Automatic spawning for both ground and flying drones is on by default. The two HUD panels track ground and air separately. The root project's viewport and requested window dimensions are **1920×1080**.

### Local folders/files

- `TestWorld.tscn`: visible road meshes, rock props, camera rig, both towers, goal, spawners and HUD.
- `TestRoute.cs`, `TestEnemy.*`, `TestTower.*`: unchanged ground route/combat experiment.
- `TestCameraRig.cs`: screen-relative panning, faster Shift movement and clamped Q/E tilt.
- `TestAirSystem.cs`: independent flying spawner, air-only turret search and air HUD.
- `TestFlyingEnemy.tscn/.cs`: simple aircraft geometry and visual flight movement.
- `TestAntiAirTower.tscn/.cs`: pitched dual-barrel weapon with direct-hit test damage.
- `TestMasterConfig.cs/.tres`: original ground tuning. Air and camera controls are separately exported on their test-only scene nodes.

### Known limitations

The road consists of editor-saved static segments aligned with the route markers. Editing the route markers does **not** yet automatically regenerate the road geometry. Terrain remains flat. Air ships do not steer around buildings or attack towers, and there is no proper wave, economy or Citadel-health gameplay. These are deliberately temporary implementations for evaluating the presentation.

No Godot/C# engine build or GPU desktop preview has been executed through the repository connector. The first in-editor build and screenshot review are still needed.
