// =========================================================
// Lightweight TEST-ONLY fixed 3D route. Sample a smooth shared
// centerline once; enemies retain only their own progress/offset.
// =========================================================
using Godot;
using System.Collections.Generic;

public partial class TestRoute : Node3D
{
    [Export(PropertyHint.Range, "1,10,0.1")] public float RoadHalfWidth { get; set; } = 2.45f;
    private readonly List<Vector3> _samples = new();
    private readonly List<float> _distances = new();
    public float TotalLength { get; private set; }

    // =========================================================
    // Called once by the test-world scene after its markers exist.
    public void Build()
    {
        var control = new List<Vector3>();
        foreach (Node child in GetChildren())
            if (child is Marker3D marker) control.Add(marker.GlobalPosition);
        _samples.Clear(); _distances.Clear(); TotalLength = 0f;
        if (control.Count < 2) { GD.PushError("TestRoute needs at least two Marker3D children."); return; }

        AddSample(control[0]);
        for (int i = 0; i < control.Count - 1; i++)
        {
            Vector3 a = control[Mathf.Max(0, i - 1)], b = control[i];
            Vector3 c = control[i + 1], d = control[Mathf.Min(control.Count - 1, i + 2)];
            for (int step = 1; step <= 12; step++)
                AddSample(Catmull(a, b, c, d, step / 12f));
        }
    }

    // =========================================================
    // Shared cubic interpolation: fixed at build time, never per enemy.
    private static Vector3 Catmull(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * (2f * b + (c - a) * t + (2f * a - 5f * b + 4f * c - d) * t2
            + (-a + 3f * b - 3f * c + d) * t3);
    }

    // =========================================================
    // Cache arc lengths for binary-search lookups while enemies move.
    private void AddSample(Vector3 point)
    {
        if (_samples.Count > 0) TotalLength += _samples[^1].DistanceTo(point);
        _samples.Add(point);
        _distances.Add(TotalLength);
    }

    // =========================================================
    // Sample at a distance plus a clamped, perpendicular road offset.
    public Vector3 Sample(float distance, float lateral, float enemyRadius, out Vector3 heading)
    {
        heading = Vector3.Forward;
        if (_samples.Count < 2) return GlobalPosition;
        distance = Mathf.Clamp(distance, 0f, TotalLength);
        int low = 1, high = _distances.Count - 1;
        while (low < high)
        {
            int middle = (low + high) / 2;
            if (_distances[middle] < distance) low = middle + 1;
            else high = middle;
        }
        int segment = low;
        Vector3 before = _samples[segment - 1], after = _samples[segment];
        float length = _distances[segment] - _distances[segment - 1];
        float blend = length > 0.0001f ? (distance - _distances[segment - 1]) / length : 0f;
        heading = (after - before).Normalized();
        if (heading.LengthSquared() < 0.001f) heading = Vector3.Forward;
        Vector3 right = new(heading.Z, 0, -heading.X);
        float clearance = Mathf.Max(0f, RoadHalfWidth - enemyRadius - 0.2f);
        return before.Lerp(after, blend) + right * Mathf.Clamp(lateral, -clearance, clearance);
    }
}
