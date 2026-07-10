using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float speed = 10f;
    public float kohle = 100f;

    private float kohleDecreaseRate = 0.1f; // Rate at which kohle decreases based on speed

    public TextMeshProUGUI speedText;
    public TextMeshProUGUI kohleText;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        speedText.text = "Speed: " + speed;
        kohleText.text = $"Kohle: {Mathf.FloorToInt(kohle)}";

        kohle -= speed * Time.deltaTime * kohleDecreaseRate; // Decrease kohle based on speed over time

        if (kohle <= 0)
        {
            speed = 0;
            kohle = 0;
        }
    }
}