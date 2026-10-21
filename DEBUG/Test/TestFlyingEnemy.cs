// =========================================================
// TEST-only flying drone, with direct-line travel plus visual weave.
// =========================================================
using Godot;
using System;
public partial class TestFlyingEnemy : Node3D
{
    public event Action<TestFlyingEnemy> Removed;
    public bool Alive { get; private set; } = true;
    public float Progress { get; private set; }
    private Vector3 _start, _goal, _heading, _side;
    private float _length, _distance, _speed, _health, _phase, _sway;

    // =========================================================
    // Capture independent flight characteristics at the spawn point.
    public void Initialize(Vector3 start, Vector3 goal, float speed, float health, float phase, float sway)
    {
        _start = start; _goal = goal; _speed = speed; _health = health; _phase = phase; _sway = sway;
        Vector3 delta = goal - start; _length = Mathf.Max(.1f, delta.Length()); _heading = delta.Normalized();
        _side = new Vector3(_heading.Z, 0f, -_heading.X).Normalized();
        Position = start;
    }

    // =========================================================
    // The weave and bob gently fade out as the drone reaches its goal.
    public override void _PhysicsProcess(double delta)
    {
        if (!Alive) return;
        _distance += _speed * (float)delta;
        Progress = Mathf.Clamp(_distance / _length, 0f, 1f);
        if (Progress >= 1f) { Remove(); return; }
        float wave = _distance * .14f + _phase;
        float fade = Mathf.Sin(Mathf.Pi * Progress);
        Position = _start.Lerp(_goal, Progress) + _side * (Mathf.Sin(wave) * _sway * fade)
            + Vector3.Up * (.65f * Mathf.Sin(wave * 1.4f) * fade);
        Rotation = new Vector3(0f, Mathf.Atan2(-_heading.X, -_heading.Z), -.12f * Mathf.Cos(wave) * fade);
    }

    // =========================================================
    // AA hits are simple test-only direct damage.
    public void Damage(float value) { if (!Alive) return; _health -= value; if (_health <= 0f) Remove(); }

    // =========================================================
    // Signal destruction or arrival before freeing the instance.
    private void Remove() { if (!Alive) return; Alive = false; Removed?.Invoke(this); QueueFree(); }
}
