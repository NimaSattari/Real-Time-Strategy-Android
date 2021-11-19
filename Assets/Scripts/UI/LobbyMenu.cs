using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] GameObject lobbyUI;
    [SerializeField] Button startGameButton;
    [SerializeField] GameObject mapChanger;
    [SerializeField] TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] string mapName = "Map_1";

    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnented;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }
    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnented;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void HandleClientConnented()
    {
        lobbyUI.SetActive(true);
    }
    private void ClientHandleInfoUpdated()
    {
        List<RTSPlayer> players = ((RTSNetworkManager)NetworkManager.singleton).Players;
        for(int i = 0; i < players.Count; i++)
        {
            playerNameTexts[i].text = players[i].GetDisplayName();
        }
        for(int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting...";
        }
        //Change Count To 2
        startGameButton.interactable = players.Count >= 1;
    }
    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGameButton.gameObject.SetActive(state);
        mapChanger.gameObject.SetActive(state);
    }

    public void SetMap(string map)
    {
        mapName = map;
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame(mapName);
    }

    public void LeaveLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(0);
        }
    }
}
