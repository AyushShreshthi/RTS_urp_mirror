using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamColorSet : NetworkBehaviour
{
    [SerializeField] private Renderer[] colorRenderers = new Renderer[0];

    [SyncVar(hook =nameof(HandleTeamColorUpdated))]
    private Color teamColor = new Color();



    #region Server

    public override void OnStartServer()
    {
        RTSplayer player = connectionToClient.identity.GetComponent<RTSplayer>();
        
        teamColor = player.GetTeamColor();


    }

    #endregion

    #region Client

    private void HandleTeamColorUpdated(Color oldColor, Color newColor)
    {
        foreach(Renderer renderer in colorRenderers)
        {
            renderer.material.SetColor("_BaseColor", newColor);
        }
    }

    #endregion
}
