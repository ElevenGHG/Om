using UnityEngine;
using System;

public class GameModeSwitch : MonoBehaviour
{
    public event Action OnSwitchToNormalMode;
    public event Action OnSwitchToCombatMode;

    public MonoBehaviour normalControl;
    public MonoBehaviour combatControl;

    public ThirdPersonController thirdPerson;
    public PlayerController player;

    public GameObject gun;

    void Start()
    {
        gun.SetActive(false);
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
            // Kampfmodus: Kamera hinter dem Spieler
            thirdPerson.ResetCameraView();
            OnSwitchToCombatMode?.Invoke();
            gun.SetActive(true);
        }
        else
        {
            // Normalmodus: Kamera schräg von oben
            player.ApplyCameraSettings();
            OnSwitchToNormalMode?.Invoke();
            gun.SetActive(false);
        }
    }
}