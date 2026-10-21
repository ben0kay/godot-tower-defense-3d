// =========================================================
// Test-only rotating hitscan turret. No generic tower framework yet.
// =========================================================
using Godot;

public partial class TestTower : Node3D
{
    private TestWorld _world;
    private TestMasterConfig _config;
    private Node3D _yaw, _pitch;
    private Marker3D _muzzle;
    private TestEnemy _target;
    private float _acquireTimer, _shotTimer;

    // =========================================================
    // Connect the turret to the local test world, not production registries.
    public void Initialize(TestWorld world, TestMasterConfig config)
    {
        _world = world; _config = config;
    }

    // =========================================================
    // Cache pivots that are already visible in TestTower.tscn.
    public override void _Ready()
    {
        _yaw = GetNode<Node3D>("YawPivot");
        _pitch = GetNode<Node3D>("YawPivot/PitchPivot");
        _muzzle = GetNode<Marker3D>("YawPivot/PitchPivot/Muzzle");
    }

    // =========================================================
    // Scan at intervals, rotate each tick, and shoot visible tracers.
    public override void _PhysicsProcess(double delta)
    {
        if (_world == null || _config == null) return;
        float dt = (float)delta;
        _acquireTimer -= dt; _shotTimer -= dt;
        if (_acquireTimer <= 0f || !ValidTarget(_target))
        {
            _target = _world.FindTarget(_yaw.GlobalPosition, _config.TowerRange);
            _acquireTimer = 0.12f;
        }
        if (!ValidTarget(_target)) return;

        Vector3 aim = (_target.GlobalPosition + Vector3.Up * 0.18f) - _yaw.GlobalPosition;
        float desiredYaw = Mathf.Atan2(-aim.X, -aim.Z);
        float nextYaw = Mathf.LerpAngle(_yaw.Rotation.Y, desiredYaw, Mathf.Clamp(dt * 8f, 0f, 1f));
        _yaw.Rotation = new Vector3(0f, nextYaw, 0f);
        float desiredPitch = Mathf.Atan2(aim.Y, new Vector2(aim.X, aim.Z).Length());
        _pitch.Rotation = new Vector3(Mathf.Lerp(_pitch.Rotation.X, desiredPitch, Mathf.Clamp(dt * 7f, 0f, 1f)), 0, 0);

        if (_shotTimer > 0f || Mathf.Abs(Mathf.AngleDifference(nextYaw, desiredYaw)) > 0.12f) return;
        Vector3 hit = _target.GlobalPosition + Vector3.Up * 0.18f;
        var tracer = new TestTracer();
        _world.AddChild(tracer);
        tracer.Initialize(_muzzle.GlobalPosition, hit);
        _target.Damage(_config.TowerDamage);
        _shotTimer = 1f / Mathf.Max(0.1f, _config.TowerShotsPerSecond);
    }

    // =========================================================
    // Keep the target safe when a killed enemy queues for deletion.
    private bool ValidTarget(TestEnemy enemy)
    {
        return enemy != null && GodotObject.IsInstanceValid(enemy) && enemy.Alive
            && enemy.GlobalPosition.DistanceSquaredTo(GlobalPosition) <= _config.TowerRange * _config.TowerRange;
    }
}
