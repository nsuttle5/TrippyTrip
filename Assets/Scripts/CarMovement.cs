using UnityEngine;
using UnityEngine.InputSystem;

public class CarMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private Transform leftBound;
    [SerializeField] private Transform rightBound;

    void Start()
    {

    }

    void Update()
    {
        float horizontalInput = 0f;

        if (Keyboard.current != null && Keyboard.current.aKey.isPressed)
        {
            horizontalInput = -1f;
        }
        else if (Keyboard.current != null && Keyboard.current.dKey.isPressed)
        {
            horizontalInput = 1f;
        }

        Vector3 position = transform.position;
        position.x += horizontalInput * moveSpeed * Time.deltaTime;

        if (leftBound != null)
        {
            position.x = Mathf.Max(position.x, leftBound.position.x);
        }

        if (rightBound != null)
        {
            position.x = Mathf.Min(position.x, rightBound.position.x);
        }

        transform.position = position;
    }
}
