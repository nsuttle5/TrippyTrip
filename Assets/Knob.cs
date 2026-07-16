using UnityEngine;
using UnityEngine.InputSystem;

public class Knob : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up;
    public float minValue = -135f;
    public float maxValue = 135f;
    public float scrollStep = 5f;
    public float value;

    Quaternion initialLocalRotation;
    bool hasInitialLocalRotation;

    void Awake()
    {
        CacheInitialRotation();
        value = Mathf.Clamp(value, minValue, maxValue);
        ApplyRotation();
    }

    void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f) && hit.collider.gameObject == gameObject)
        {
            float scrollDelta = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
            if (Mathf.Abs(scrollDelta) <= 0.01f)
            {
                return;
            }

            value = Mathf.Clamp(value + scrollDelta * scrollStep, minValue, maxValue);
            ApplyRotation();
        }
    }

    void OnValidate()
    {
        CacheInitialRotation();
        value = Mathf.Clamp(value, minValue, maxValue);

        if (!Application.isPlaying)
        {
            ApplyRotation();
        }
    }

    void CacheInitialRotation()
    {
        if (hasInitialLocalRotation)
        {
            return;
        }

        initialLocalRotation = transform.localRotation;
        hasInitialLocalRotation = true;
    }

    void ApplyRotation()
    {
        transform.localRotation = initialLocalRotation * Quaternion.AngleAxis(value, rotationAxis.normalized);
    }
}
