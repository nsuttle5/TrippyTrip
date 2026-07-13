using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public Vector3 direction;
    public float setSpeed;
    public static float speed;

    public float loopForward, loopBackward;
    void Update()
    {
        speed = setSpeed;
        foreach(Transform child in transform)
        {
            Vector3 targetPosition = child.position + direction.normalized * setSpeed * 10 * Time.deltaTime;
        
            // Move smoothly toward that target position
            child.position = Vector3.MoveTowards(child.position, targetPosition, setSpeed * 10 * Time.deltaTime);

            if(child.transform.position.z < loopBackward)
            {
                child.transform.position = new Vector3(child.transform.position.x, child.transform.position.y, loopForward);
            }
        }

        Shader.SetGlobalFloat("_speed", -speed);
    }
}
