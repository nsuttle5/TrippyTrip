using UnityEngine;

[CreateAssetMenu(fileName = "New Passenger Item", menuName = "Passenger Seat/Passenger Item Data")]
public class PassengerItemData : ScriptableObject
{
    [Header("Model")]
    public GameObject modelPrefab;

    [Header("Transform Overrides (relative to the seat anchor)")]
    [Tooltip("Local position offset from the seat anchor's position.")]
    public Vector3 positionOffset = Vector3.zero;
    [Tooltip("Local rotation (Euler) relative to the seat anchor's facing. Fixes a model showing the wrong side/facing the wrong way.")]
    public Vector3 rotationOffset = Vector3.zero;
    [Tooltip("Multiplies the prefab's own scale, per axis.")]
    public Vector3 scaleMultiplier = Vector3.one;
}
