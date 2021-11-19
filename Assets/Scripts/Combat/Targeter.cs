using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    private Targetable target;
    [SerializeField] Animator animator;

    public Targetable Target
    {
        get
        {
            return target;
        }

        [Server]
        set
        {
            target = value;
        }
    }
    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    [Command] public void CmdSetTarget(GameObject targetGameObject)
    {
        if(!targetGameObject.TryGetComponent<Targetable>(out Targetable newtarget)) { return; }
        target = newtarget;
    }

    [Server] public void ClearTarget()
    {
        target = null;
    }

    [Server]
    private void ServerHandleGameOver()
    {
        ClearTarget();
    }
}
