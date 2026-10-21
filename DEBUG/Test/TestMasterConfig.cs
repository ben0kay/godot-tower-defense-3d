// =========================================================
// Inspector-friendly settings for the disposable DEBUG/Test prototype.
// =========================================================
using Godot;

[GlobalClass]
public partial class TestMasterConfig : Resource
{
    [ExportGroup("Enemy")]
    [Export(PropertyHint.Range, "0.5,20,0.1")] public float EnemySpeed { get; set; } = 5.6f;
    [Export(PropertyHint.Range, "1,200,1")] public float EnemyHealth { get; set; } = 45f;
    [Export(PropertyHint.Range, "0.2,2,0.1")] public float EnemyRadius { get; set; } = 0.52f;
    [Export(PropertyHint.Range, "1,1000,1")] public int MaxAliveEnemies { get; set; } = 300;

    [ExportGroup("Ground Route Variation")]
    [Export] public bool EnableLateralVariation { get; set; } = true;
    [Export(PropertyHint.Range, "0,2,0.05")] public float MaxLateralOffset { get; set; } = 1.45f;
    [Export(PropertyHint.Range, "0,0.75,0.05")] public float SideDriftAmplitude { get; set; } = 0.24f;

    [ExportGroup("Tower")]
    [Export(PropertyHint.Range, "2,40,0.5")] public float TowerRange { get; set; } = 15.5f;
    [Export(PropertyHint.Range, "1,100,1")] public float TowerDamage { get; set; } = 18f;
    [Export(PropertyHint.Range, "0.1,20,0.1")] public float TowerShotsPerSecond { get; set; } = 2.5f;

    [ExportGroup("Spawning")]
    [Export] public bool AutoSpawn { get; set; } = true;
    [Export(PropertyHint.Range, "0.2,10,0.1")] public float AutoSpawnInterval { get; set; } = 1.2f;
    [Export(PropertyHint.Range, "0.02,2,0.01")] public float ManualBatchGap { get; set; } = 0.17f;
}
