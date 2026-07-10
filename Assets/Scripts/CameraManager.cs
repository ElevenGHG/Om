using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera topDownCamera;
    public Camera firstPersonCamera;

    void Start()
    {
        topDownCamera.enabled = true;
        firstPersonCamera.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            topDownCamera.enabled = !topDownCamera.enabled;
            firstPersonCamera.enabled = !firstPersonCamera.enabled;
        }
    }
}
