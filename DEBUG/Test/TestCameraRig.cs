// =========================================================
// TEST-only isometric pan and tilt controls.
// =========================================================
using Godot;
public partial class TestCameraRig : Node3D
{
    [Export] public float PanSpeed { get; set; } = 28f;
    [Export] public float TiltSpeed { get; set; } = 28f;
    private Node3D _tilt;
    private Camera3D _camera;

    // =========================================================
    // Cache the scene camera and its pitch pivot.
    public override void _Ready()
    {
        _tilt = GetNode<Node3D>("TiltPivot");
        _camera = GetNode<Camera3D>("TiltPivot/Camera");
    }

    // =========================================================
    // Move in screen-relative ground directions and tilt about the pivot.
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        float horizontal = (Input.IsKeyPressed(Key.Right) ? 1f : 0f) - (Input.IsKeyPressed(Key.Left) ? 1f : 0f);
        float vertical = (Input.IsKeyPressed(Key.Up) ? 1f : 0f) - (Input.IsKeyPressed(Key.Down) ? 1f : 0f);
        Vector3 right = _camera.GlobalTransform.Basis.X;
        Vector3 up = _camera.GlobalTransform.Basis.Y;
        right.Y = 0f; up.Y = 0f;
        Vector3 move = right.Normalized() * horizontal + up.Normalized() * vertical;
        if (move.LengthSquared() > .001f)
        {
            float boost = Input.IsKeyPressed(Key.Shift) ? 2.2f : 1f;
            Position += move.Normalized() * PanSpeed * boost * dt;
            Position = new Vector3(Mathf.Clamp(Position.X, -66f, 66f), 0f, Mathf.Clamp(Position.Z, -38f, 38f));
        }
        float tiltInput = (Input.IsKeyPressed(Key.Q) ? 1f : 0f) - (Input.IsKeyPressed(Key.E) ? 1f : 0f);
        if (tiltInput == 0f) return;
        float pitch = Mathf.Clamp(_tilt.RotationDegrees.X + tiltInput * TiltSpeed * dt, -70f, -24f);
        _tilt.RotationDegrees = new Vector3(pitch, 0f, 0f);
    }
}
