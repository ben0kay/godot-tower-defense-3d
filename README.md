# Godot Tower Defense 3D

Prototype-first isometric 3D tower defense using Godot 4.7 / C#.

## DEBUG/Test status

- **Pass 1:** fixed winding road, ground drone, automated/manual spawner, ground-only turret.
- **Pass 2 (2026-10-21):** larger saved battlefield and longer winding route, flying aircraft entering from varied off-map positions, independent anti-air turret, movable/tiltable camera, and a 1920×1080 viewport/window.

Run in **Godot 4.7.x .NET** after building C#. Open `res://DEBUG/Test/TestWorld.tscn` and press F6, or press F5 for the project startup scene.

See [DEBUG/Test/README.md](DEBUG/Test/README.md) for camera and spawning controls, architecture, tunables and limitations. Everything is still a disposable debug prototype; no gameplay systems have been extracted into proper production folders.
