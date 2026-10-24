using UnityEngine;

public class GameModeSwitch : MonoBehaviour
{
    public MonoBehaviour normalControl;
    public MonoBehaviour combatControl;

    public ThirdPersonController thirdPerson;
    public PlayerController player;

    void Start()
    {
        normalControl.enabled = true;
        combatControl.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SwitchMode();
        }
    }

    public void SwitchMode()
    {
        bool normalIsActive = normalControl.enabled;

        normalControl.enabled = !normalIsActive;
        combatControl.enabled = normalIsActive;

        if (normalIsActive)
        {
            // Wechsel in den Combat-Modus
            player.ApplyCameraSettings();
        }
        else
        {
            // Wechsel zurück in den Third-Person-Modus
            thirdPerson.ResetCameraView();
        }
    }
}