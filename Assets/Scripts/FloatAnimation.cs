using UnityEngine;

public class FloatAnimation : MonoBehaviour
{
    [Header("Movement")]
    public float distance = 20f;
    public float duration = 1.5f;

    [Header("Curve")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 startPos;
    private float timer;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t = Mathf.PingPong(timer / duration, 1f);
        float y = curve.Evaluate(t);

        transform.localPosition = startPos + Vector3.up * (y * distance);
    }
}