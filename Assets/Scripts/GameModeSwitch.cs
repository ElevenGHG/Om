
using UnityEngine;

public class GameModeSwitch : MonoBehaviour
{
    public MonoBehaviour normalControl;
    public MonoBehaviour combatControl;

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
    }
}
