using UnityEngine;
using UnityEngine.UIElements;

public class BackGround : MonoBehaviour
{
    public GameObject BackGroundPrefab;
    
    void Start()
    {
        Instantiate(BackGroundPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        Instantiate(BackGroundPrefab, new Vector3(300, 0, 0), Quaternion.identity);
        Instantiate(BackGroundPrefab, new Vector3(-300, 0, 0), Quaternion.identity);
    }

    void Update()
    {
        if (GameObject.FindGameObjectsWithTag("Background").Length < 3)
        {
            Instantiate(BackGroundPrefab, new Vector3(300, 0, 0), Quaternion.identity);
        }
    }
}
