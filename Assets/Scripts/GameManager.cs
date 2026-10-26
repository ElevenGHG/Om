using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameModeSwitch gameModeSwitch;

    public float speed = 10f;
    public float kohle = 100f;

    private float kohleDecreaseRate = 0.1f;

    public TextMeshProUGUI speedText;
    public TextMeshProUGUI kohleText;
    public TextMeshProUGUI gameOverText;
    public Image crosshair;

    private void Awake()
    {
        Instance = this;

        gameOverText.gameObject.SetActive(false);

        gameModeSwitch.OnSwitchToCombatMode += hidenormalmodeUI;
        gameModeSwitch.OnSwitchToNormalMode += hidecombatmodeUI;
    }

    private void Start()
    {
        hidecombatmodeUI();
    }

    private void hidenormalmodeUI()
    {
        speedText.gameObject.SetActive(false);
        kohleText.gameObject.SetActive(false);
        crosshair.gameObject.SetActive(true);
    }

    private void hidecombatmodeUI()
    {
        speedText.gameObject.SetActive(true);
        kohleText.gameObject.SetActive(true);
        crosshair.gameObject.SetActive(false);
    }

    private void Update()
    {
        speedText.text = "Speed: " + speed;
        kohleText.text = $"Kohle: {Mathf.FloorToInt(kohle)}";

        kohle -= speed * Time.deltaTime * kohleDecreaseRate;

        if (kohle <= 0)
        {
            speed = 0;
            kohle = 0;
            gameOverText.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (gameModeSwitch != null)
        {
            gameModeSwitch.OnSwitchToCombatMode -= hidenormalmodeUI;
            gameModeSwitch.OnSwitchToNormalMode -= hidecombatmodeUI;
        }
    }
}