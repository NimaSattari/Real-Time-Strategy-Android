using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] Animator animator;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 3f;

    public bool HasWaypoint { get; [Server] set; }

    public Vector3 MyWaypoint { get; [Server] set; }

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        animator.SetFloat("Move", agent.velocity.magnitude);
        Targetable target = targeter.Target;
        if (target != null)
        {
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                SetMyWaypoint(target.transform.position);
            }
            else if (agent.hasPath)
            {
                ClearMyWaypoint();
            }
            return;
        }
        if (!agent.hasPath) { return; }
        if (agent.remainingDistance > agent.stoppingDistance) { return; }
        ClearMyWaypoint();
    }

    [Command] public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server] public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            return;
        }
        SetMyWaypoint(position);
    }

    [Server]
    public void SetMyWaypoint(Vector3 position)
    {
        HasWaypoint = true;
        MyWaypoint = position;
        agent.SetDestination(position);
    }

    [Server]
    public void ClearMyWaypoint()
    {
        HasWaypoint = false;
        agent.ResetPath();
    }

    [Server]
    private void ServerHandleGameOver()
    {
        ClearMyWaypoint();
    }

    #endregion
}
