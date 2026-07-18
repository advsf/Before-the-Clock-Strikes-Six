using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleLook : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity;
    [SerializeField] private float minYRot = -25f;
    [SerializeField] private float maxYRot = 80f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform camHolder;
    [SerializeField] private Transform orientation;
    [SerializeField] private Camera cam;
    
    private float xRotation = 0f;
    private float yRotation;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            cam.enabled = false;
            cam.GetComponent<AudioListener>().enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            HandleCameraLook();
        }
    }

    private void HandleCameraLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minYRot, maxYRot);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        player.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
