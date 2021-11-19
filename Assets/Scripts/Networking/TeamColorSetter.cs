using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour
{
    [SerializeField] private Renderer[] colorRenderers = new Renderer[0];
    [SyncVar(hook = nameof(HandleTeamColorUpdated))] private Color teamColor = new Color();
    public Material white, blue, red, green, purple;

    #region Server

    public override void OnStartServer()
    {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        teamColor = player.GetTeamColor();
    }

    #endregion

    #region Client

    private void HandleTeamColorUpdated(Color oldColor,Color newColor)
    {
        foreach(Renderer renderer in colorRenderers)
        {
            if(renderer.GetComponent<SkinnedMeshRenderer>() != null)
            {
                if (teamColor == Color.red)
                {
                    renderer.GetComponent<SkinnedMeshRenderer>().material = red;
                }
                else if (teamColor == Color.blue)
                {
                    renderer.GetComponent<SkinnedMeshRenderer>().material = blue;
                }
                else if (teamColor == Color.green)
                {
                    renderer.GetComponent<SkinnedMeshRenderer>().material = green;
                }
                else if (teamColor == Color.magenta)
                {
                    renderer.GetComponent<SkinnedMeshRenderer>().material = purple;
                }
                else
                {
                    renderer.GetComponent<SkinnedMeshRenderer>().material = white;
                }
            }
            else
            {
                renderer.material.SetColor("_BaseColor", newColor);
            }
        }
    }

    #endregion
}
