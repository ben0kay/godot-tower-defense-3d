// =========================================================
// Completely self-contained DEBUG/Test orchestration and keyboard UI.
// All production extraction is intentionally deferred.
// =========================================================
using Godot;
using System.Collections.Generic;

public partial class TestWorld : Node3D
{
    [Export] public TestMasterConfig Config { get; set; }
    [Export] public PackedScene EnemyScene { get; set; }

    private readonly List<TestEnemy> _enemies = new();
    private TestRoute _route;
    private TestTower _tower;
    private Label _stats;
    private RandomNumberGenerator _random = new();
    private float _autoTimer, _manualTimer, _hudTimer;
    private int _pending, _killed, _escaped;
    private bool _variationEnabled;

    // =========================================================
    // Prepare a single curve before any enemy begins moving.
    public override void _Ready()
    {
        _route = GetNode<TestRoute>("Route");
        _tower = GetNode<TestTower>("Tower");
        _stats = GetNode<Label>("HUD/Panel/Content/Stats");
        Config ??= new TestMasterConfig();
        _route.Build();
        _tower.Initialize(this, Config);
        _variationEnabled = Config.EnableLateralVariation;
        _random.Randomize();
        SpawnOne();
        UpdateHud();
    }

    // =========================================================
    // Two bounded, independent spawn queues: automatic and manual.
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        if (Config.AutoSpawn)
        {
            _autoTimer -= dt;
            if (_autoTimer <= 0f)
            {
                if (_enemies.Count < Config.MaxAliveEnemies) SpawnOne();
                _autoTimer = Mathf.Max(0.2f, Config.AutoSpawnInterval);
            }
        }

        if (_pending > 0)
        {
            _manualTimer -= dt;
            if (_manualTimer <= 0f && _enemies.Count < Config.MaxAliveEnemies)
            {
                SpawnOne();
                _pending--;
                _manualTimer = Mathf.Max(0.02f, Config.ManualBatchGap);
            }
        }
        _hudTimer -= dt;
        if (_hudTimer <= 0f) { UpdateHud(); _hudTimer = 0.2f; }
    }

    // =========================================================
    // Keyboard-only debug commands so the prototype needs no full UI.
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed || key.Echo) return;
        switch (key.Keycode)
        {
            case Key.Key1: _pending += 1; break;
            case Key.Key2:
            case Key.F2: _pending += 10; break;
            case Key.Key3:
            case Key.F3: _pending += 50; break;
            case Key.V: _variationEnabled = !_variationEnabled; break;
            case Key.R: ResetTest(); break;
            default: return;
        }
        GetViewport().SetInputAsHandled();
        UpdateHud();
    }

    // =========================================================
    // Allocate just one tiny enemy with a stable lane and drift seed.
    private void SpawnOne()
    {
        if (EnemyScene == null || _enemies.Count >= Config.MaxAliveEnemies || _route.TotalLength <= 0) return;
        var enemy = EnemyScene.Instantiate<TestEnemy>();
        float maxOffset = Mathf.Min(Config.MaxLateralOffset, Mathf.Max(0f, _route.RoadHalfWidth - Config.EnemyRadius - 0.2f));
        float offset = _variationEnabled ? _random.RandfRange(-maxOffset, maxOffset) : 0f;
        float phase = _random.RandfRange(0f, Mathf.Tau);
        enemy.Initialize(_route, Config, offset, phase);
        enemy.Removed += OnEnemyRemoved;
        AddChild(enemy);
        enemy.GlobalPosition = _route.Sample(0f, offset, Config.EnemyRadius, out _) + Vector3.Up * 0.58f;
        _enemies.Add(enemy);
    }

    // =========================================================
    // One turret may linearly inspect the lightweight active list.
    public TestEnemy FindTarget(Vector3 position, float range)
    {
        TestEnemy best = null;
        float bestProgress = -1f, squaredRange = range * range;
        foreach (TestEnemy enemy in _enemies)
        {
            if (!GodotObject.IsInstanceValid(enemy) || !enemy.Alive
                || enemy.GlobalPosition.DistanceSquaredTo(position) > squaredRange) continue;
            if (enemy.Travelled > bestProgress) { bestProgress = enemy.Travelled; best = enemy; }
        }
        return best;
    }

    // =========================================================
    // Shared removal callback: tell reached-goal and combat apart.
    private void OnEnemyRemoved(TestEnemy enemy)
    {
        if (enemy.Travelled >= _route.TotalLength) _escaped++;
        else _killed++;
        _enemies.Remove(enemy);
    }

    // =========================================================
    // Clear this TEST scene without affecting the rest of the project.
    private void ResetTest()
    {
        foreach (TestEnemy enemy in _enemies)
        {
            if (!GodotObject.IsInstanceValid(enemy)) continue;
            enemy.Removed -= OnEnemyRemoved;
            enemy.QueueFree();
        }
        _enemies.Clear();
        _pending = _killed = _escaped = 0;
        _manualTimer = _autoTimer = 0f;
    }

    // =========================================================
    // Debug-only compact HUD, refreshed at 5 Hz instead of each frame.
    private void UpdateHud()
    {
        _stats.Text = $"Alive: {_enemies.Count}  |  Killed: {_killed}  |  Escaped: {_escaped}  |  Queued: {_pending}"
            + $"\nAuto spawn: {(Config.AutoSpawn ? "ON" : "OFF")}  |  Lane variation (new spawns): {(_variationEnabled ? "ON" : "OFF")}"
            + "\n1 = +1   2/F2 = +10   3/F3 = +50   V = lane variation   R = reset";
    }
}
