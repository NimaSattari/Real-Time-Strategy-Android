using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 2f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] Animator animator;

    private float lastFireTime;

    private void Update()
    {
        Targetable target = targeter.Target;
        if (target == null) { return; }
        if (!CanFireAtTarget()) { return; }
        animator.SetBool("Attack", true);
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        /*if (Time.time > (1 / fireRate) + lastFireTime)
        {
            Quaternion projectileRotation = Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpawnPoint.position);
            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);
            NetworkServer.Spawn(projectileInstance, connectionToClient);
            lastFireTime = Time.time;
        }*/
    }
    public void NewEvent()
    {
        if(targeter.Target == null)
        {
            animator.SetBool("Attack", false);
            return;
        }
        if (targeter.Target.GetComponent<Health>().currentHealth <= 0)
        {
            targeter.ClearTarget();
            animator.SetBool("Attack", false);
        }
        Quaternion projectileRotation = Quaternion.LookRotation(targeter.Target.GetAimAtPoint().position - projectileSpawnPoint.position);
        GameObject projectileInstance =
            Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);

        UnitProjectile projectile = projectileInstance.GetComponent<UnitProjectile>();
        projectile.OriginTransform = transform; NetworkServer.Spawn(projectileInstance, connectionToClient);
        lastFireTime = Time.time;
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.Target.transform.position - transform.position).sqrMagnitude <= fireRange * fireRange;
    }
}
