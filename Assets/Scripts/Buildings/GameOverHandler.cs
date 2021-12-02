using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action<string> ClientOnGameOver;
    public static event Action ServerOnGameOver;

    private List<UnitBase> bases = new List<UnitBase>();

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDeSpawned += ServerHandleBaseDeSpawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDeSpawned -= ServerHandleBaseDeSpawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDeSpawned(UnitBase unitBase)
    {
        if (this == null) { return; }

        bases.Remove(unitBase);
        if(bases.Count != 1) { return; }
        string playerId = bases[0].playerName;
        RpcGameOver($"{playerId}");
        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        AudioManagerMainMenu.instance.PlayWinSound();
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
