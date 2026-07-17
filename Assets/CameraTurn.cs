using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTurn : MonoBehaviour
{
    public Transform Player;
    public Vector2 sensitivity = new Vector2(100f, 100f);
    public Vector2 UpDownClamp = new Vector2(-30, 60);
    public Vector2 LeftRightClamp = new Vector2(-110, 110);
    public float rotationSmoothing = 0;

    [Header("Zoom")]
    public float zoomFieldOfView = 45f;
    public float zoomSmoothTime = 0.12f;

    public float xRotation = 0f;
    public float yRotation = 0f;
    public GameObject testCamera;

    public static bool cursorLocked = true;

    private Camera _camera;
    private float _defaultFieldOfView;
    private float _zoomVelocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera != null)
        {
            _defaultFieldOfView = _camera.fieldOfView;
        }
    }

    void LateUpdate()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                testCamera.SetActive(!testCamera.activeSelf);
                GetComponent<Camera>().enabled = !testCamera.activeSelf;
            }
        }

        if (_camera != null)
        {
            float targetFieldOfView = Mouse.current != null && Mouse.current.rightButton.isPressed
                ? zoomFieldOfView
                : _defaultFieldOfView;

            _camera.fieldOfView = Mathf.SmoothDamp(
                _camera.fieldOfView,
                targetFieldOfView,
                ref _zoomVelocity,
                zoomSmoothTime);
        }

        SetCursorLocked(cursorLocked);
        if (!cursorLocked) return;

        // Get mouse input from the new Input System
        Vector2 mouseDelta = Vector2.zero;
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }

        float mouseX = mouseDelta.x * sensitivity.x;
        float mouseY = mouseDelta.y * sensitivity.y;

        // Update vertical rotation (X axis)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, UpDownClamp.x, UpDownClamp.y);
        yRotation += mouseX;
        //yRotation = Mathf.Repeat(yRotation, 360f); // Keep yRotation within 0-360 degrees
        yRotation = Mathf.Clamp(yRotation, LeftRightClamp.x, LeftRightClamp.y);

        // Apply rotations with angle wrap handling
        float smoothPitch = rotationSmoothing > 0
            ? Mathf.LerpAngle(transform.localEulerAngles.x, xRotation, rotationSmoothing * Time.deltaTime)
            : xRotation;
        float smoothYaw = rotationSmoothing > 0
            ? Mathf.LerpAngle(transform.localEulerAngles.y, yRotation, rotationSmoothing * Time.deltaTime)
            : yRotation;

        transform.localRotation = Quaternion.Euler(smoothPitch, smoothYaw, 0f);
    }

    private void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
