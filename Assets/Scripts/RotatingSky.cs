using UnityEngine;

public class RotatingSky : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float speedIncreasePerSecond = 1f;
    [SerializeField] private bool useWorldSpace = true;

    void Update()
    {
        Vector3 axis = rotationAxis.sqrMagnitude > 0f ? rotationAxis.normalized : Vector3.up;

        transform.Rotate(axis, rotationSpeed * Time.deltaTime, useWorldSpace ? Space.World : Space.Self);
        rotationSpeed += speedIncreasePerSecond * Time.deltaTime;
    }
}
