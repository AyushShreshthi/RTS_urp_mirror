using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSnetworkmanager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    private bool isGameInProgress = false;

    public List<RTSplayer> Players { get; } = new List<RTSplayer>();

    #region Server

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (!isGameInProgress) return;

        conn.Disconnect();
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        RTSplayer player = conn.identity.GetComponent<RTSplayer>();

        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();

        isGameInProgress = false;
    }
    public void StartGame()
    {
        if (Players.Count < 2) return;

        isGameInProgress = true;

        ServerChangeScene("GameScene");
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        RTSplayer player = conn.identity.GetComponent<RTSplayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");

        player.SetTeamColor(new Color(
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f)
                ));

        player.SetPartyOwner(Players.Count == 1);

        //GameObject unitSpawnerInstance = Instantiate(unitSpawnerPrefab,
        //            conn.identity.transform.position,
        //            conn.identity.transform.rotation);

        //NetworkServer.Spawn(unitSpawnerInstance, conn);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Game"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(RTSplayer player in Players)
            {
               GameObject baseInstance = 
                    Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);

                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }

    #endregion

    #region Client

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        ClientOnConnected?.Invoke();
    }
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }
    #endregion

}
