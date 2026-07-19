using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public float distortionStrength;

    private TripState _tripState;
    private readonly Dictionary<drug.DrugEffect.TargetVariable, Coroutine> _activeLerps =
        new Dictionary<drug.DrugEffect.TargetVariable, Coroutine>();

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

    public void ApplyDrug(drug drug)
    {
        if (drug == null)
        {
            return;
        }

        IReadOnlyList<drug.DrugEffect> effects = drug.Effects;
        for (int i = 0; i < effects.Count; i++)
        {
            ApplyDrugEffect(effects[i]);
        }
    }

    private void ApplyDrugEffect(drug.DrugEffect effect)
    {
        if (effect.Mode == drug.DrugEffect.EffectMode.Immediate)
        {
            StopLerp(effect.Variable);
            SetVariableValue(effect.Variable, effect.TargetValue);
            return;
        }

        StopLerp(effect.Variable);
        Coroutine lerpRoutine = StartCoroutine(LerpVariable(effect.Variable, effect.TargetValue, effect.LerpDuration));
        _activeLerps[effect.Variable] = lerpRoutine;
    }

    private void StopLerp(drug.DrugEffect.TargetVariable variable)
    {
        if (_activeLerps.TryGetValue(variable, out Coroutine existing) && existing != null)
        {
            StopCoroutine(existing);
        }

        _activeLerps.Remove(variable);
    }

    private IEnumerator LerpVariable(drug.DrugEffect.TargetVariable variable, float targetValue, float duration)
    {
        if (duration <= 0f)
        {
            SetVariableValue(variable, targetValue);
            _activeLerps.Remove(variable);
            yield break;
        }

        float startValue = GetVariableValue(variable);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetVariableValue(variable, Mathf.Lerp(startValue, targetValue, t));
            yield return null;
        }

        SetVariableValue(variable, targetValue);
        _activeLerps.Remove(variable);
    }

    private float GetVariableValue(drug.DrugEffect.TargetVariable variable)
    {
        switch (variable)
        {
            case drug.DrugEffect.TargetVariable.CurveX:
                return curve.x;
            case drug.DrugEffect.TargetVariable.CurveY:
                return curve.y;
            case drug.DrugEffect.TargetVariable.CurveZ:
                return curve.z;
            case drug.DrugEffect.TargetVariable.DistortionStrength:
                return distortionStrength;
            default:
                return 0f;
        }
    }

    private void SetVariableValue(drug.DrugEffect.TargetVariable variable, float value)
    {
        switch (variable)
        {
            case drug.DrugEffect.TargetVariable.CurveX:
                curve.x = value;
                break;
            case drug.DrugEffect.TargetVariable.CurveY:
                curve.y = value;
                break;
            case drug.DrugEffect.TargetVariable.CurveZ:
                curve.z = value;
                break;
            case drug.DrugEffect.TargetVariable.DistortionStrength:
                distortionStrength = value;
                break;
        }
    }
}
