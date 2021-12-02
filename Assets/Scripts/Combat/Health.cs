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

    [SerializeField] GameObject buildingExplosion, humanExplosion, buildingDamage, humanDamage;
    #region Server

    public bool HasStartedDeath { get; [Server] set; }

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
        if(TryGetComponent(out Unit unit))
        {
            unit.DeSelect();
        }
        ServerOnDie?.Invoke();
    }
    [Server] public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0) { return; }

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if(currentHealth != 0) { return; }
/*        if (TryGetComponent(out Unit unit))
        {
            unit.DeSelect();
        }*/
        ServerOnDie?.Invoke();
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth,int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
        if (TryGetComponent(out Unit unit) && currentHealth != 0)
        {
            AudioManagerMainMenu.instance.PlayHHurtSound();
            if(currentHealth != maxHealth)
            {
                GameObject humanDamageInstance = Instantiate(humanDamage.gameObject, this.transform.position, humanDamage.transform.rotation);
                NetworkServer.Spawn(humanDamageInstance, connectionToClient);
            }
        }
        else if(!TryGetComponent(out Unit unit1) && currentHealth != 0)
        {
            AudioManagerMainMenu.instance.PlayBHurtSound();
            if (currentHealth != maxHealth)
            {
                GameObject buildingDamageInstance = Instantiate(buildingDamage.gameObject, this.transform.position, buildingDamage.transform.rotation);
                NetworkServer.Spawn(buildingDamageInstance, connectionToClient);
            }
        }
        else if (TryGetComponent(out Unit unit2) && currentHealth <= 0)
        {
            AudioManagerMainMenu.instance.PlayHDieSound();
            GameObject humanExplosionInstance = Instantiate(humanExplosion.gameObject, this.transform.position, humanExplosion.transform.rotation);
            NetworkServer.Spawn(humanExplosionInstance, connectionToClient);
        }
        else if (!TryGetComponent(out Unit unit3) && currentHealth <= 0)
        {
            AudioManagerMainMenu.instance.PlayBDieSound();
            GameObject buildingExplosionInstance = Instantiate(buildingExplosion.gameObject, this.transform.position, buildingExplosion.transform.rotation);
            NetworkServer.Spawn(buildingExplosionInstance, connectionToClient);
        }
    }

    #endregion
}
