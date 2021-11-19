using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] public int id = 0;
    [SerializeField] private int resourceCost = 20;
    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeSelected = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private Sprite unitImage = null;
    [SerializeField] public UnitUI unitUI = null;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDeSpawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDeSpawned;

    public void SetUnitUI(UnitUI uI)
    {
        unitUI = uI;
    }

    public int GetResourceCost()
    {
        return resourceCost;
    }

    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }
    public Targeter GetTargeter()
    {
        return targeter;
    }

    public Sprite GetImage()
    {
        return unitImage;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
        health.ServerOnTakeDamage += ServerHandleTakeDamage;
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        ServerOnUnitDeSpawned?.Invoke(this);
        health.ServerOnTakeDamage -= ServerHandleTakeDamage;
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    public void ServerHandleTakeDamage(Transform enemyTransform)
    {
        // if i don't have a task and i am not en route
        if (!targeter.Target && !unitMovement.HasWaypoint)
        {
            // then go towards the enemy to attack
            targeter.Target = enemyTransform.GetComponent<Targetable>();
        }
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!hasAuthority) { return; }
        AuthorityOnUnitDeSpawned?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if (!hasAuthority) { return; }
        onSelected?.Invoke();
    }

    [Client]
    public void DeSelect()
    {
        if (!hasAuthority) { return; }
        onDeSelected?.Invoke();
        if (unitUI.gameObject != null)
        {
            Destroy(unitUI.gameObject);
        }
    }

    #endregion
}
