using UnityEngine;

public class TripController : MonoBehaviour
{
    [System.Flags]
    public enum TripState
    {
        None        = 0,
        Fire        = 1 << 0, // 1
        Water       = 1 << 1, // 2
        Earth       = 1 << 2, // 4
        Air         = 1 << 3, // 8
        Lightning   = 1 << 4  // 16
    }

    public Vector3 curve;

    private TripState _tripState;

    private static readonly int GlobalCurveId =
        Shader.PropertyToID("_My_Global_Curve");

    void Update()
    {
        Shader.SetGlobalVector(
            GlobalCurveId,
            new Vector4(curve.x, curve.y, curve.z, 0f)
        );
    }
    public TripState tripState
    {
        get => _tripState;
        set
        {
            // Only trigger if the value actually changed
            if (_tripState != value)
            {
                _tripState = value;
                ChangeState(_tripState);
            }
        }
    }

    void ChangeState(TripState state)
    {
        if (state.HasFlag(TripState.Fire))
        {
            Debug.Log("Fire is active!");
        }
        
    }
}
