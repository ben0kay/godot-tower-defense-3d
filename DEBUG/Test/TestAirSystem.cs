// =========================================================
// Isolated flying-unit spawner, aerial target search and compact HUD.
// =========================================================
using Godot;
using System.Collections.Generic;
public partial class TestAirSystem : Node3D
{
    [Export] public PackedScene AirEnemyScene { get; set; }
    [Export] public bool AutoSpawn { get; set; } = true;
    [Export] public float AutoInterval { get; set; } = 2.4f;
    [Export] public float FlightSpeed { get; set; } = 12f;
    [Export] public float FlightAltitude { get; set; } = 13f;
    [Export] public float AirHealth { get; set; } = 78f;
    [Export] public int MaxAlive { get; set; } = 100;
    private readonly List<TestFlyingEnemy> _flying = new();
    private readonly RandomNumberGenerator _rng = new();
    private Label _status;
    private int _pending, _kills, _escaped;
    private float _autoTimer, _batchTimer, _hudTimer;

    // =========================================================
    // Attach the AA turret and begin with one flying drone.
    public override void _Ready()
    {
        _rng.Randomize();
        _status = GetNode<Label>("AirHUD/Panel/Stats");
        GetParent().GetNode<TestAntiAirTower>("AntiAirTower").Initialize(this);
        SpawnOne(); UpdateHud();
    }

    // =========================================================
    // Automatic aircraft and manual batches have independent timers.
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        if (AutoSpawn)
        {
            _autoTimer -= dt;
            if (_autoTimer <= 0f) { SpawnOne(); _autoTimer = Mathf.Max(.2f, AutoInterval); }
        }
        if (_pending > 0 && _flying.Count < MaxAlive)
        {
            _batchTimer -= dt;
            if (_batchTimer <= 0f) { SpawnOne(); _pending--; _batchTimer = .19f; }
        }
        _hudTimer -= dt;
        if (_hudTimer <= 0f) { UpdateHud(); _hudTimer = .2f; }
    }

    // =========================================================
    // Flying debug spawns use 4/5/6, separate from ground 1/2/3.
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed || key.Echo) return;
        switch (key.Keycode)
        {
            case Key.Key4: _pending++; break;
            case Key.Key5: case Key.F4: _pending += 10; break;
            case Key.Key6: _pending += 50; break;
            default: return;
        }
        GetViewport().SetInputAsHandled(); UpdateHud();
    }

    // =========================================================
    // Reset air on R even when the ground test handles that same key.
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.R) ResetAir();
    }

    // =========================================================
    // Spawn beyond randomly selected west, north and south map borders.
    private void SpawnOne()
    {
        if (AirEnemyScene == null || _flying.Count >= MaxAlive) return;
        Vector3 from;
        switch (_rng.RandiRange(0, 2))
        {
            case 0: from = new Vector3(-89f, 0f, _rng.RandfRange(-37f, 37f)); break;
            case 1: from = new Vector3(_rng.RandfRange(-62f, 12f), 0f, -59f); break;
            default: from = new Vector3(_rng.RandfRange(-62f, 12f), 0f, 59f); break;
        }
        from.Y = FlightAltitude + _rng.RandfRange(-2.5f, 2.5f);
        Vector3 goal = GetParent().GetNode<Node3D>("Citadel").GlobalPosition + Vector3.Up * 1.25f;
        var enemy = AirEnemyScene.Instantiate<TestFlyingEnemy>();
        enemy.Initialize(from, goal, FlightSpeed * _rng.RandfRange(.88f, 1.12f),
            AirHealth, _rng.RandfRange(0f, Mathf.Tau), _rng.RandfRange(.8f, 2f));
        enemy.Removed += OnRemoved; AddChild(enemy); enemy.GlobalPosition = from;
        _flying.Add(enemy);
    }

    // =========================================================
    // Simple air-only nearest-to-goal lookup for one prototype turret.
    public TestFlyingEnemy FindTarget(Vector3 origin, float radius)
    {
        TestFlyingEnemy best = null; float progress = -1f, rangeSq = radius * radius;
        foreach (TestFlyingEnemy enemy in _flying)
        {
            if (!GodotObject.IsInstanceValid(enemy) || !enemy.Alive ||
                enemy.GlobalPosition.DistanceSquaredTo(origin) > rangeSq) continue;
            if (enemy.Progress > progress) { progress = enemy.Progress; best = enemy; }
        }
        return best;
    }

    // =========================================================
    // Count reaches-the-goal separately from turret destructions.
    private void OnRemoved(TestFlyingEnemy enemy)
    {
        if (enemy.Progress >= 1f) _escaped++; else _kills++;
        _flying.Remove(enemy);
    }

    // =========================================================
    // Discard queued aircraft and alive units without touching ground.
    private void ResetAir()
    {
        foreach (TestFlyingEnemy enemy in _flying)
        {
            if (!GodotObject.IsInstanceValid(enemy)) continue;
            enemy.Removed -= OnRemoved; enemy.QueueFree();
        }
        _flying.Clear(); _pending = _kills = _escaped = 0;
        _autoTimer = _batchTimer = 0f; UpdateHud();
    }

    // =========================================================
    // Air counters and camera controls in a dedicated HUD panel.
    private void UpdateHud()
    {
        if (_status == null) return;
        _status.Text = $"AIR  Alive {_flying.Count}  Killed {_kills}  Escaped {_escaped}  Queued {_pending}"
            + "\n4/5/6 spawn +1/+10/+50 flying   |   R resets both tests"
            + "\nCamera: Arrows pan  Shift fast  Q tilt up  E tilt down";
    }
}
