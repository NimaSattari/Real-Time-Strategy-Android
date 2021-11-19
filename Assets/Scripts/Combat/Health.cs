using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    public int currentHealth;

    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthUpdated;
    public event Action<Transform> ServerOnTakeDamage;
    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }
    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId)
    {
        if (connectionToClient.connectionId != connectionId) { return; }
        DealDamage(currentHealth);
    }

    [Server] public void DealDamage(UnitProjectile projectile)
    {
        if (currentHealth == 0) { return; }
        if (projectile.OriginTransform) ServerOnTakeDamage?.Invoke(projectile.OriginTransform);

        DealDamage(projectile.DamageToDeal);
        if (currentHealth != 0) { return; }
        GetComponent<Unit>().DeSelect();
        ServerOnDie?.Invoke();
    }
    [Server] public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0) { return; }

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if(currentHealth != 0) { return; }
        GetComponent<Unit>().DeSelect();
        ServerOnDie?.Invoke();
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth,int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}
