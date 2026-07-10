using UnityEngine;

public class MoveBackground : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * GameManager.Instance.speed * Time.deltaTime); // Move the background left
        if (transform.position.x < -300)
        {
            Destroy(gameObject); // Destroy the background when it goes off-screen
        }
    }
}
