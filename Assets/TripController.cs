using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TripController : MonoBehaviour
{
    [System.Flags]
    public enum TripState
    {
        None = 0,
        Fire = 1 << 0, // 1
        Water = 1 << 1, // 2
        Earth = 1 << 2, // 4
        Air = 1 << 3, // 8
        Lightning = 1 << 4  // 16
    }

    public Vector3 curve;
    public float distortionStrength;
    public Color tintColor = Color.white;
    public float swirlStrength;
    public float noiseStrength;

    private TripState _tripState;
    private readonly Dictionary<drug.DrugEffect.TargetVariable, Coroutine> _activeEffects =
        new Dictionary<drug.DrugEffect.TargetVariable, Coroutine>();

    private static readonly int GlobalCurveId =
        Shader.PropertyToID("_My_Global_Curve");
    private static readonly int GlobalTintColorId =
        Shader.PropertyToID("_Drug_TintColor");
    private static readonly int GlobalSwirlStrengthId =
        Shader.PropertyToID("_Drug_SwirlStrength");
    private static readonly int GlobalNoiseStrengthId =
        Shader.PropertyToID("_Drug_NoiseStrength");

    void Update()
    {
        Shader.SetGlobalVector(
            GlobalCurveId,
            new Vector4(curve.x, curve.y, curve.z, 0f)
        );
        Shader.SetGlobalColor(GlobalTintColorId, tintColor);
        Shader.SetGlobalFloat(GlobalSwirlStrengthId, swirlStrength);
        Shader.SetGlobalFloat(GlobalNoiseStrengthId, noiseStrength);
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
        if (effect == null)
        {
            return;
        }

        if (effect.HasParameters)
        {
            IReadOnlyList<drug.DrugEffect.EffectParameter> parameters = effect.Parameters;
            for (int i = 0; i < parameters.Count; i++)
            {
                ApplyEffectParameter(parameters[i]);
            }

            return;
        }

        ApplyLegacyEffect(effect);
    }

    private void ApplyLegacyEffect(drug.DrugEffect effect)
    {
        if (effect.Mode == drug.DrugEffect.EffectMode.Immediate)
        {
            StopEffectRoutine(effect.Variable);
            SetVariableValue(effect.Variable, effect.TargetValue);
            return;
        }

        if (effect.Mode == drug.DrugEffect.EffectMode.Lerp)
        {
            StopEffectRoutine(effect.Variable);
            Coroutine lerpRoutine = StartCoroutine(LerpFloatVariable(effect.Variable, effect.TargetValue, effect.LerpDuration));
            _activeEffects[effect.Variable] = lerpRoutine;
            return;
        }

        if (effect.Mode == drug.DrugEffect.EffectMode.Loop)
        {
            StopEffectRoutine(effect.Variable);
            Coroutine loopRoutine = StartCoroutine(LoopFloatVariable(effect.Variable, effect.TargetValue, effect.SecondaryValue, effect.LoopDuration));
            _activeEffects[effect.Variable] = loopRoutine;
        }
    }

    private void ApplyEffectParameter(drug.DrugEffect.EffectParameter parameter)
    {
        if (parameter.Mode == drug.DrugEffect.EffectMode.Immediate)
        {
            StopEffectRoutine(parameter.Variable);
            SetParameterValue(parameter.Variable, parameter.TargetValue, parameter.TargetColor);
            return;
        }

        if (parameter.Mode == drug.DrugEffect.EffectMode.Lerp)
        {
            StopEffectRoutine(parameter.Variable);

            Coroutine lerpRoutine = parameter.Variable == drug.DrugEffect.TargetVariable.TintColor
                ? StartCoroutine(LerpColorVariable(parameter.Variable, parameter.TargetColor, parameter.LerpDuration))
                : StartCoroutine(LerpFloatVariable(parameter.Variable, parameter.TargetValue, parameter.LerpDuration));

            _activeEffects[parameter.Variable] = lerpRoutine;
            return;
        }

        StopEffectRoutine(parameter.Variable);

        Coroutine loopRoutine = parameter.Variable == drug.DrugEffect.TargetVariable.TintColor
            ? StartCoroutine(LoopColorVariable(parameter.Variable, parameter.TargetColor, parameter.SecondaryColor, parameter.LoopDuration))
            : StartCoroutine(LoopFloatVariable(parameter.Variable, parameter.TargetValue, parameter.SecondaryValue, parameter.LoopDuration));

        _activeEffects[parameter.Variable] = loopRoutine;
    }

    private void StopEffectRoutine(drug.DrugEffect.TargetVariable variable)
    {
        if (_activeEffects.TryGetValue(variable, out Coroutine existing) && existing != null)
        {
            StopCoroutine(existing);
        }

        _activeEffects.Remove(variable);
    }

    private IEnumerator LerpFloatVariable(drug.DrugEffect.TargetVariable variable, float targetValue, float duration)
    {
        if (duration <= 0f)
        {
            SetVariableValue(variable, targetValue);
            _activeEffects.Remove(variable);
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
        _activeEffects.Remove(variable);
    }

    private IEnumerator LerpColorVariable(drug.DrugEffect.TargetVariable variable, Color targetColor, float duration)
    {
        if (duration <= 0f)
        {
            SetParameterValue(variable, 0f, targetColor);
            _activeEffects.Remove(variable);
            yield break;
        }

        Color startColor = GetColorValue(variable);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetColorValue(variable, Color.Lerp(startColor, targetColor, t));
            yield return null;
        }

        SetColorValue(variable, targetColor);
        _activeEffects.Remove(variable);
    }

    private IEnumerator LoopFloatVariable(drug.DrugEffect.TargetVariable variable, float startValue, float endValue, float duration)
    {
        if (duration <= 0f)
        {
            SetVariableValue(variable, endValue);
            _activeEffects.Remove(variable);
            yield break;
        }

        float elapsed = 0f;
        while (true)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed / duration, 1f);
            SetVariableValue(variable, Mathf.Lerp(startValue, endValue, t));
            yield return null;
        }
    }

    private IEnumerator LoopColorVariable(drug.DrugEffect.TargetVariable variable, Color startColor, Color endColor, float duration)
    {
        if (duration <= 0f)
        {
            SetColorValue(variable, endColor);
            _activeEffects.Remove(variable);
            yield break;
        }

        float elapsed = 0f;
        while (true)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed / duration, 1f);
            SetColorValue(variable, Color.Lerp(startColor, endColor, t));
            yield return null;
        }
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
            case drug.DrugEffect.TargetVariable.SwirlStrength:
                swirlStrength = value;
                break;
            case drug.DrugEffect.TargetVariable.NoiseStrength:
                noiseStrength = value;
                break;
        }
    }

    private Color GetColorValue(drug.DrugEffect.TargetVariable variable)
    {
        switch (variable)
        {
            case drug.DrugEffect.TargetVariable.TintColor:
                return tintColor;
            default:
                return Color.white;
        }
    }

    private void SetColorValue(drug.DrugEffect.TargetVariable variable, Color value)
    {
        switch (variable)
        {
            case drug.DrugEffect.TargetVariable.TintColor:
                tintColor = value;
                break;
        }
    }

    private void SetParameterValue(drug.DrugEffect.TargetVariable variable, float value, Color color)
    {
        if (variable == drug.DrugEffect.TargetVariable.TintColor)
        {
            SetColorValue(variable, color);
            return;
        }

        SetVariableValue(variable, value);
    }
}
