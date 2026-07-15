using UnityEngine;

/// <summary>
/// Attach to (or auto-added onto) the gas station prefab's trigger collider.
/// The station's own model/collider setup should have at least one Collider
/// with "Is Trigger" enabled, sized to span the road so the player can't miss it.
/// The player's car collider must be tagged "Player".
/// </summary>
[RequireComponent(typeof(Collider))]
public class GasStationTrigger : MonoBehaviour
{
    private GasStationCheckpointManager _manager;
    private bool _hasTriggered;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    public void Init(GasStationCheckpointManager manager)
    {
        _manager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        _hasTriggered = true;
        _manager.OnPlayerEnteredStation();
    }
}