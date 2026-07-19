using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDrug", menuName = "TrippyTrip/Drug")]
public class drug : ScriptableObject
{
    [SerializeField] private string drugName;
    [SerializeField] private List<DrugEffect> effects = new List<DrugEffect>();

    public string DrugName => drugName;
    public IReadOnlyList<DrugEffect> Effects => effects;

    [Serializable]
    public class DrugEffect
    {
        public enum EffectMode
        {
            Immediate,
            Lerp,
            Loop
        }

        public enum TargetVariable
        {
            CurveX,
            CurveY,
            CurveZ,
            DistortionStrength,
            TintColor,
            SwirlStrength,
            NoiseStrength
        }

        [Serializable]
        public class EffectParameter
        {
            [SerializeField] private TargetVariable targetVariable;
            [SerializeField] private EffectMode mode = EffectMode.Immediate;
            [SerializeField] private float targetValue;
            [SerializeField] private float secondaryValue;
            [SerializeField] private Color targetColor = Color.white;
            [SerializeField] private Color secondaryColor = Color.white;
            [SerializeField, Min(0f)] private float lerpDuration = 1f;
            [SerializeField, Min(0.01f)] private float loopDuration = 2f;

            public TargetVariable Variable => targetVariable;
            public EffectMode Mode => mode;
            public float TargetValue => targetValue;
            public float SecondaryValue => secondaryValue;
            public Color TargetColor => targetColor;
            public Color SecondaryColor => secondaryColor;
            public float LerpDuration => lerpDuration;
            public float LoopDuration => loopDuration;
        }

        [SerializeField] private List<EffectParameter> parameters = new List<EffectParameter>();

        public IReadOnlyList<EffectParameter> Parameters => parameters;

        // Legacy single-parameter path, kept so existing assets continue to work.
        [SerializeField] private TargetVariable targetVariable;
        [SerializeField] private EffectMode mode = EffectMode.Immediate;
        [SerializeField] private float targetValue;
        [SerializeField] private float secondaryValue;
        [SerializeField] private Color targetColor = Color.white;
        [SerializeField] private Color secondaryColor = Color.white;
        [SerializeField, Min(0f)] private float lerpDuration = 1f;
        [SerializeField, Min(0.01f)] private float loopDuration = 2f;

        public TargetVariable Variable => targetVariable;
        public EffectMode Mode => mode;
        public float TargetValue => targetValue;
        public float SecondaryValue => secondaryValue;
        public Color TargetColor => targetColor;
        public Color SecondaryColor => secondaryColor;
        public float LerpDuration => lerpDuration;
        public float LoopDuration => loopDuration;
        public bool HasParameters => parameters != null && parameters.Count > 0;
    }
}
