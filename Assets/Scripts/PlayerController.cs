using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        transform.Translate(speed * Time.deltaTime * new Vector3(horizontalInput, 0, verticalInput));
    }
}
