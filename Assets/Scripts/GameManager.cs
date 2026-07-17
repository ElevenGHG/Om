using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float speed = 10f;
    public float kohle = 100f;
    public float kohleSpawnRate = 5;

    public GameObject kohlePrefabe;

    private float kohleDecreaseRate = 0.1f; // Rate at which kohle decreases based on speed

    public TextMeshProUGUI speedText;
    public TextMeshProUGUI kohleText;
    public TextMeshProUGUI gameOverText;

    void Awake()
    {
        Instance = this;
        gameOverText.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(SpawnKohle());
    }

    IEnumerator SpawnKohle()
    {
        yield return new WaitForSeconds(kohleSpawnRate);
        Vector2 kohleSPawnPos = new Vector2(Random.Range(-75, 75), Random.Range(-3, 3));
        Instantiate(kohlePrefabe, kohleSPawnPos, Quaternion.identity);
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
            gameOverText.gameObject.SetActive(true);
        }
    }
}