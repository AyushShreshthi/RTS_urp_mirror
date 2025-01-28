using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using System;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab = null;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImae = null;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7;
    [SerializeField] private float unitSpawnDuraction = 5;

    [SyncVar(hook =nameof(ClienthandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    private void Update()
    {
        if (isServer)
        {
            ProduceUnits();
        }
        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }
    #region Server
    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }
    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ProduceUnits()
    {
        if (queuedUnits == 0) return;

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuraction) return;

        GameObject unitInstance = Instantiate(unitPrefab.gameObject,
                                                spawnPoint.position,
                                                spawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffest = UnityEngine.Random.insideUnitSphere * spawnMoveRange;
        spawnOffest.y = spawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(spawnPoint.position + spawnOffest);

        queuedUnits--;
        unitTimer = 0;
    }
    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnunit()
    {
        if (queuedUnits == maxUnitQueue) return;

        RTSplayer player = connectionToClient.identity.GetComponent<RTSplayer>();

        if (player.GetResources() < unitPrefab.GetresourceCost()) return;

        queuedUnits++;

        player.SetResources(player.GetResources() - unitPrefab.GetresourceCost());
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        print(gameObject.name);
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (!hasAuthority) return;

        CmdSpawnunit();
    }
    private void ClienthandleQueuedUnitsUpdated(int oldUnits,int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }

    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuraction;

        if (newProgress < unitProgressImae.fillAmount)
        {
            unitProgressImae.fillAmount = newProgress;
        }
        else
        {
            unitProgressImae.fillAmount = Mathf.SmoothDamp(
                unitProgressImae.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f);
        }
    }
    #endregion
}
