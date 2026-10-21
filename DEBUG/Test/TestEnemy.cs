// =========================================================
// One disposable ground target: shared test route, private progress,
// simple HP, and no navigation/physics queries.
// =========================================================
using Godot;
using System;

public partial class TestEnemy : Node3D
{
    public event Action<TestEnemy> Removed;
    public bool Alive { get; private set; } = true;
    public float Travelled { get; private set; }

    private TestRoute _route;
    private TestMasterConfig _config;
    private float _health, _baseOffset, _phase;
    private MeshInstance3D _hull;

    // =========================================================
    // Initialize BEFORE AddChild so the first physics tick is valid.
    public void Initialize(TestRoute route, TestMasterConfig config, float offset, float phase)
    {
        _route = route; _config = config; _health = config.EnemyHealth;
        _baseOffset = offset; _phase = phase;
    }

    // =========================================================
    // Cache only this enemy's presentation mesh.
    public override void _Ready() => _hull = GetNode<MeshInstance3D>("Hull");

    // =========================================================
    // Increment arc distance; all expensive curve building is shared.
    public override void _PhysicsProcess(double delta)
    {
        if (!Alive || _route == null || _config == null) return;
        Travelled += Mathf.Max(0.1f, _config.EnemySpeed) * (float)delta;
        if (Travelled >= _route.TotalLength) { RemoveFromTest(); return; }
        float offset = _baseOffset + Mathf.Sin(Travelled * 0.13f + _phase) * _config.SideDriftAmplitude;
        GlobalPosition = _route.Sample(Travelled, offset, _config.EnemyRadius, out Vector3 forward) + Vector3.Up * 0.58f;
        Rotation = new Vector3(0, Mathf.Atan2(-forward.X, -forward.Z), 0);
    }

    // =========================================================
    // Prototype direct hit and tiny scale feedback.
    public void Damage(float amount)
    {
        if (!Alive) return;
        _health -= amount;
        if (_health <= 0f) { RemoveFromTest(); return; }
        _hull.Scale = new Vector3(1f, Mathf.Lerp(0.55f, 1f, _health / Mathf.Max(1f, _config.EnemyHealth)), 1f);
    }

    // =========================================================
    // Signal synchronously so the tower no longer targets freed enemies.
    private void RemoveFromTest()
    {
        if (!Alive) return;
        Alive = false;
        Removed?.Invoke(this);
        QueueFree();
    }
}
