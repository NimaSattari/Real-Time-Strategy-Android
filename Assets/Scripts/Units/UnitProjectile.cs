using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rigid = null;
    [SerializeField] private int damageToDeal = 20;
    [SerializeField] private float destroyAfterSeconds = 1f;
    [SerializeField] private float launchForce = 10f;

    public int DamageToDeal
    {
        get
        {
            return damageToDeal;
        }
    }

    public Transform OriginTransform { get; [Server] set; }

    void Start()
    {
        rigid.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            if(networkIdentity.connectionToClient == connectionToClient) { return; }
        }
        if(other.TryGetComponent<Health>(out Health health))
        {
            if (!health.HasStartedDeath)
            {
                health.DealDamage(this);
            }
        }
        DestroySelf();
    }

    [Server] private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
