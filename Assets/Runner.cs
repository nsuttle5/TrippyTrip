using UnityEngine;

public class Runner : MonoBehaviour
{
    public Vector3 forwardDirection = Vector3.forward;
    public Rigidbody rb;
    public float farDistance = 10f;
    public float farSpeedMultiplier = 2f;
    public float nearSpeedMultiplier = 1.2f;
    void Update()
    {
        float speed = CarMovement.scrollSpeed;
        float distance = Vector3.Distance(transform.position, Vector3.zero);
        if (distance < farDistance)
        {
            speed *= farSpeedMultiplier;
        }
        else
        {
            speed *= nearSpeedMultiplier;
        }
        rb.linearVelocity = speed * forwardDirection;
    }
}
