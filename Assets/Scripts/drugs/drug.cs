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
            Lerp
        }

        public enum TargetVariable
        {
            CurveX,
            CurveY,
            CurveZ,
            DistortionStrength
        }

        [SerializeField] private TargetVariable targetVariable;
        [SerializeField] private EffectMode mode = EffectMode.Immediate;
        [SerializeField] private float targetValue;
        [SerializeField, Min(0f)] private float lerpDuration = 1f;

        public TargetVariable Variable => targetVariable;
        public EffectMode Mode => mode;
        public float TargetValue => targetValue;
        public float LerpDuration => lerpDuration;
    }
}
