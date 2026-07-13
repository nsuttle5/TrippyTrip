using UnityEngine;
using UnityEngine.InputSystem;

public class CarMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float bound;
    [SerializeField] private float boundForce;
    [SerializeField] private float boundStart;


    void Update()
    {
        float horizontalInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
            {
                horizontalInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed)
            {
                horizontalInput = 1f;
            }
        }

        Vector3 position = rb.position;
        position.x = Mathf.Clamp(position.x, -bound, bound);
        rb.position = position;

        float closeness = Mathf.Clamp01((Mathf.Abs(position.x) - (bound - boundStart)) / boundStart);

        float desiredSpeed = horizontalInput * moveSpeed - closeness*boundForce*moveSpeed*Mathf.Sign(position.x);
        float currentSpeed = rb.linearVelocity.x;
        float newSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.deltaTime);
        rb.linearVelocity = new Vector3(newSpeed, rb.linearVelocity.y, rb.linearVelocity.z);
    }
}
