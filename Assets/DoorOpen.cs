using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public bool open = false;
    public Vector3 goTo;
    public float speed = 2f;
    private Vector3 _velocity;
    public void Open(float delay = 0f)
    {
        Invoke(nameof(OpenDoor), delay);
    }

    public void Close(float delay = 0f)
    {
        Invoke(nameof(CloseDoor), delay);
    }

    private void OpenDoor()
    {
        open = true;
    }

    private void CloseDoor()
    {
        open = false;
    }
    void Update()
    {
        if (open)
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, goTo, ref _velocity, Time.deltaTime * speed);
        }
        else
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref _velocity, Time.deltaTime * speed);
        }
    }
}
