using UnityEngine;

public enum ScenerySide
{
    Left,
    Right,
    Either
}

[CreateAssetMenu(fileName = "New Scenery", menuName = "Scenery/Scenery Data")]
public class SceneryData : ScriptableObject
{
    [Header("Placement")]
    public GameObject modelPrefab;
    public ScenerySide spawnSide = ScenerySide.Left;
    [Tooltip("If enabled, the object will be rotated inward toward the road when it spawns on either side.")]
    public bool faceRoad = true;

    [Header("Side Facing Offsets")]
    [Tooltip("Extra rotation applied when this scenery spawns on the left side. Use this to correct the prefab's natural forward direction.")]
    public Vector3 leftFacingOffset = Vector3.zero;
    [Tooltip("Extra rotation applied when this scenery spawns on the right side. Set this to 180 degrees around Y if the right side faces backwards.")]
    public Vector3 rightFacingOffset = new Vector3(0f, 180f, 0f);

    [Header("Transform Overrides")]
    [Tooltip("Added to the computed spawn position. Use this to fine-tune where the scenery sits.")]
    public Vector3 positionOffset = Vector3.zero;
    [Tooltip("Extra Euler rotation applied on top of the spawn rotation.")]
    public Vector3 rotationOffset = Vector3.zero;
    [Tooltip("Multiplies the prefab's own scale per-axis. (1,1,1) leaves the prefab's scale untouched.")]
    public Vector3 scaleMultiplier = Vector3.one;
}