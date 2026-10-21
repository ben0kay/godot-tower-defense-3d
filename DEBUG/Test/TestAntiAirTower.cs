// =========================================================
// TEST-only twin-barrel AA turret. It never targets ground units.
// =========================================================
using Godot;
public partial class TestAntiAirTower : Node3D
{
    [Export] public float Range { get; set; } = 46f;
    [Export] public float DamagePerShot { get; set; } = 26f;
    [Export] public float ShotsPerSecond { get; set; } = 2.5f;
    private TestAirSystem _air;
    private Node3D _yaw, _pitch;
    private Marker3D _left, _right;
    private TestFlyingEnemy _target;
    private float _search, _cooldown;
    private bool _nextRight;

    // =========================================================
    // Wire into the DEBUG air manager rather than production registries.
    public void Initialize(TestAirSystem air) => _air = air;

    // =========================================================
    // Cache turret yaw/pitch pivots and both firing points.
    public override void _Ready()
    {
        _yaw = GetNode<Node3D>("YawPivot"); _pitch = GetNode<Node3D>("YawPivot/PitchPivot");
        _left = GetNode<Marker3D>("YawPivot/PitchPivot/MuzzleLeft");
        _right = GetNode<Marker3D>("YawPivot/PitchPivot/MuzzleRight");
    }

    // =========================================================
    // Track approaching ships and raise barrel pitch before firing.
    public override void _PhysicsProcess(double delta)
    {
        if (_air == null) return;
        float dt = (float)delta; _search -= dt; _cooldown -= dt;
        if (_search <= 0f || !Valid(_target)) { _target = _air.FindTarget(_yaw.GlobalPosition, Range); _search = .14f; }
        if (!Valid(_target)) return;
        Vector3 aim = _target.GlobalPosition - _yaw.GlobalPosition;
        float yaw = Mathf.Atan2(-aim.X, -aim.Z);
        float next = Mathf.LerpAngle(_yaw.Rotation.Y, yaw, Mathf.Clamp(dt * 7f, 0f, 1f));
        _yaw.Rotation = new Vector3(0f, next, 0f);
        float pitch = Mathf.Atan2(aim.Y, new Vector2(aim.X, aim.Z).Length());
        _pitch.Rotation = new Vector3(Mathf.Lerp(_pitch.Rotation.X, pitch, Mathf.Clamp(dt * 7f, 0f, 1f)), 0f, 0f);
        if (_cooldown > 0f || Mathf.Abs(Mathf.AngleDifference(next, yaw)) > .15f
            || Mathf.Abs(_pitch.Rotation.X - pitch) > .16f) return;
        Marker3D muzzle = _nextRight ? _right : _left; _nextRight = !_nextRight;
        var tracer = new TestTracer();
        GetParent().AddChild(tracer);
        tracer.Initialize(muzzle.GlobalPosition, _target.GlobalPosition);
        _target.Damage(DamagePerShot);
        _cooldown = 1f / Mathf.Max(.1f, ShotsPerSecond);
    }

    // =========================================================
    // Reject invalid, destroyed or out-of-range aircraft.
    private bool Valid(TestFlyingEnemy enemy) =>
        enemy != null && GodotObject.IsInstanceValid(enemy) && enemy.Alive
        && enemy.GlobalPosition.DistanceSquaredTo(GlobalPosition) <= Range * Range;
}
