// =========================================================
// Very small-lived visual bolt for test turret hitscan fire.
// =========================================================
using Godot;

public partial class TestTracer : MeshInstance3D
{
    private float _life = 0.12f;

    // =========================================================
    // Make one simple glowing cylinder aligned between muzzle and hit.
    public void Initialize(Vector3 from, Vector3 to)
    {
        Vector3 segment = to - from;
        if (segment.LengthSquared() < 0.0001f) { QueueFree(); return; }
        Mesh = new CylinderMesh { TopRadius = 0.055f, BottomRadius = 0.055f, Height = segment.Length(), RadialSegments = 5 };
        MaterialOverride = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(0.14f, 0.85f, 1f),
            EmissionEnabled = true,
            Emission = new Color(0.12f, 0.7f, 1f),
            EmissionEnergyMultiplier = 2.5f
        };
        CastShadow = ShadowCastingSetting.Off;
        GlobalPosition = (from + to) * 0.5f;
        Quaternion = new Quaternion(Vector3.Up, segment.Normalized());
    }

    // =========================================================
    // No collision, particles or physics; retire after the flash.
    public override void _Process(double delta)
    {
        _life -= (float)delta;
        if (_life <= 0f) QueueFree();
    }
}
