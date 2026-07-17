using UnityEngine;

public enum ObstacleLane
{
    Left,
    Center,
    Right
}

/// <summary>
/// Data-driven obstacle definition. Create instances via
/// Assets > Create > Obstacles > Obstacle Data.
/// The spawner reads this to instantiate the model in the right lane and
/// configure an ObstacleSpeedPenalty on it at spawn time.
/// </summary>
[CreateAssetMenu(fileName = "New Obstacle", menuName = "Obstacles/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    [Header("Placement")]
    public ObstacleLane lane = ObstacleLane.Center;
    public GameObject modelPrefab;

    [Header("Speed Penalty")]
    public float speedPenalty = 1.5f;
    public bool destroySelfOnHit = true;

    [Header("Transform Overrides")]
    [Tooltip("Added to the computed lane spawn position. Fixes a model that's offset from where it should sit.")]
    public Vector3 positionOffset = Vector3.zero;
    [Tooltip("Extra Euler rotation applied on top of the prefab's own rotation and the spawn point's facing. Use this to fix orientation instead of editing the prefab.")]
    public Vector3 rotationOffset = Vector3.zero;
    [Tooltip("Multiplies the prefab's own scale per-axis. (1,1,1) leaves the prefab's scale untouched.")]
    public Vector3 scaleMultiplier = Vector3.one;

    [Header("Sound Effects")]
    [Tooltip("One is picked at random when the player hits this obstacle. Leave empty for silent.")]
    public AudioClip[] hitSounds;
    [Header("Particles")]
    public GameObject deathParticle;
}
