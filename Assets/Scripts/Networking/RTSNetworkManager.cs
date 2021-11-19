using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;
using Steamworks;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    bool isGameInProgress = false;

    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    #region Server

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (!isGameInProgress) { return; }
        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Remove(player);
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();
        isGameInProgress = false;
    }

    public void StartGame(string mapName)
    {
        //Change Count To 2
        if (Players.Count < 1) { return; }
        isGameInProgress = true;
        ServerChangeScene(mapName);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");
        //player.SetDisplayName(SteamUser.GetSteamID().ToString());
        if(Players.Count == 1)
        {
            player.SetTeamColor(Color.blue);
        }
        else if(Players.Count == 2)
        {
            player.SetTeamColor(Color.red);
        }
        else if (Players.Count == 3)
        {
            player.SetTeamColor(Color.green);
        }
        else if (Players.Count == 4)
        {
            player.SetTeamColor(Color.magenta);
        }
        else
        {
            player.SetTeamColor(Color.white);
        }
        player.SetPartyOwner(Players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Map"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
            foreach(RTSPlayer player in Players)
            {
                GameObject baseInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
#if UNITY_IOS || UNITY_ANDROID
                player.SetActiveMobileInputs(true);
#endif
            }
        }
#if UNITY_IOS || UNITY_ANDROID
        else
        {
            foreach (RTSPlayer player in Players)
            {
                player.SetActiveMobileInputs(false);
            }
        }
#endif
    }

    #endregion

    #region Client

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        ClientOnConnected?.Invoke();
    }
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion
}
