using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];

    private void Start()
    {
        RTSnetworkmanager.ClientOnConnected += HandleClientConnected;
        RTSplayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSplayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }
    private void OnDestroy()
    {

        RTSnetworkmanager.ClientOnConnected -= HandleClientConnected;
        RTSplayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSplayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void ClientHandleInfoUpdated()
    {
        List<RTSplayer> players = ((RTSnetworkmanager)NetworkManager.singleton).Players;

        for (int i = 0; i < players.Count; i++)
        {
            playerNameTexts[i].text = players[i].GetDisplayName();
        }

        for (int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player ...";
        }

        startGameButton.interactable = players.Count >= 1;

    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }
    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGameButton.gameObject.SetActive(state);
    }
    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSplayer>().CmdStartGame();
    }
    public void LeaveLobby()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }


    }
}
