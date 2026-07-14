using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public Vector3 direction;
    private float setSpeed;

    public float loopForward, loopBackward;
    void Update()
    {
        setSpeed = CarMovement.scrollSpeed;
        foreach(Transform child in transform)
        {
            Vector3 targetPosition = child.position + direction.normalized * setSpeed * 10 * Time.deltaTime;
        
            // Move smoothly toward that target position
            child.position = Vector3.MoveTowards(child.position, targetPosition, setSpeed * 10 * Time.deltaTime);

            if(child.transform.position.z < loopBackward)
            {
                //child.transform.position = new Vector3(child.transform.position.x, child.transform.position.y, loopForward);
                Destroy(child.gameObject);
            }
        }
    }
}
