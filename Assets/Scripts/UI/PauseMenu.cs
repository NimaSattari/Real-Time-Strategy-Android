using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject menuCanvas;
    Controls controls;

    private void Start()
    {
        controls = new Controls();

        controls.Player.Pause.performed += Pause;

        controls.Enable();
    }

    public void Pause(InputAction.CallbackContext ctx)
    {
        if (menuCanvas.activeInHierarchy)
        {
            menuCanvas.SetActive(false);
        }
        else
        {
            menuCanvas.SetActive(true);
        }
    }
    public void Resume()
    {
        menuCanvas.SetActive(false);
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }
}
