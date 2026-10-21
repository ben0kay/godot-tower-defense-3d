# 3D Tower Defense — Test World

## Pass 1 — basic scene and interactions (2026-10-21)

**Contained prototype, not production architecture.** Only the test world, its local enemy/tower scripts, and a tiny MasterConfig are implemented. No shared tower/enemy infrastructure, navigation grid, collision steering, economy, or wave system.

### Run

- Use **Godot 4.7.x .NET**, open the repo, and **Build** C# once. The project includes a \`.csproj\` because the clean repo did not have one.
- Open \`res://DEBUG/Test/TestWorld.tscn\` and press **F6**, or **F5** (the project main scene is this test).
- Expected: static desert platform and road visible already in the editor; one test tower tracks red ground drones; cyan tracers remove their health; enemies move from a red entry marker towards the blue citadel.
- **1**: queue one enemy; **2** or **F2**: queue ten; **3** or **F3**: queue fifty; **V**: toggle lateral variation for newly spawned enemies; **R**: clear enemies, queued spawns and counters. Automated spawning defaults on.
- The HUD shows alive/killed/escaped counts and the number queued.

### Local organization

- \`TestWorld.tscn\`: editor-visible ground, a segmented static road, rocks, lights, camera, markers, turret and HUD.
- \`TestWorld.cs\`: local spawning/counters/keys, test-only target lookup.
- \`TestRoute.cs\`: one route built once from the editable \`Route/PointXX\` markers; cached Catmull-Rom samples and cumulative distances, with lateral offsets constrained by road half width and enemy radius. No A* or per-enemy path building.
- \`TestEnemy.tscn/.cs\`: primitive mesh and simple health/movement.
- \`TestTower.tscn/.cs\`: local rotating gun, periodic target acquisition and direct-hit damage.
- \`TestTracer.cs\`: short-lived visual hitscan streak.
- \`TestMasterConfig.cs/.tres\`: Inspector-adjustable prototype speeds, HP, damage, ranges, spawn budget, lateral spread, drift and auto spawning.

### Intentional limitations

- The visible road is made of **editor-saved segments** so it cannot disappear due to runtime terrain generation. The runtime movement curve comes from the similarly placed marker nodes. **Moving markers does not yet regenerate those saved visual road segments**; keep them aligned or rebuild visual road geometry when authoring a new course. An editor road authoring tool is a later pass.
- No ground physics/avoidance, collision bodies, dynamic obstacles, terrain-height sampling or flying ships yet. The test enemy intentionally intersects no collision physics.
- Turret shots are instant gameplay hits with temporary luminous tracer meshes, *not* simulated ballistics.
- Initial route variation is bounded by a single constant-width corridor; no varying-width clearance profile.
- 3D and C# build/runtime verification must be performed on your Godot desktop; GitHub source inspection alone is not proof of a successful engine run.

### Suggested follow-up passes

- **Pass 2:** tune camera, terrain, road silhouette, enemy/tower scale, lighting and materials after an actual screenshot review.
- **Pass 3:** improve route authoring (editable smooth road mesh + guide) and tune corridor-safe variations and larger enemy batches.
- **Pass 4:** add an elevated flight route and one simple flying enemy, separately from ground movement.
- **Pass 5:** combat feedback polish and profiling at realistic tower/enemy counts before production extraction.

The existing 2D repository is a mechanics reference only. Keep each job to one GitHub commit and follow root \`CodePreferences.md\`.
