﻿
using TMPro;
using MLAPI;
using UnityEngine;
using System.Collections;

public class PingCounter : MonoBehaviour
{
    public float refresh = 20;
    private int timer, avgPing;
    public TextMeshProUGUI countText;
    public Settings settings;
    private bool showPing = false;
    private GameServer gameServer;
    private ulong clientId;


    private void Start()
    {
        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient) 
        {
            clientId = NetworkingManager.Singleton.LocalClientId;
            gameServer = GameServer.singleton;
            showPing = settings.showPing; //Get showPing bool.
            countText.gameObject.SetActive(showPing); //Activate/Deactivate fpsPanel
            StartCoroutine(UpdateLoop());
        }
    }

    //Subscribe to EVENT
    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;//Subscribe to Settings Change Event.
    }

    //Unsubscribe from EVENT
    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;//unSubscribe to Settings Change Event.
    }

    //Change Settings.
    private void Change()
    {
        showPing = settings.showPing;
        countText.gameObject.SetActive(showPing);
    }

    //Update Loop for Calculating Ping
    private IEnumerator UpdateLoop()
    {
        while (true) 
        {
            gameServer.GetPlayerPing(clientId, returnValue =>
            {
                if (returnValue < 0) { returnValue = 0; }
                if (showPing) 
                {
                    avgPing += returnValue;
                    timer++;
                    returnValue = (avgPing / timer);
                    countText.text = returnValue + " ms";
                }
                if (returnValue < 101) 
                {
                    ChangePingIconColor(Color.white);
                }
                else if(returnValue > 100 && returnValue < 181)
                {
                    ChangePingIconColor(Color.yellow);
                }
                else if(returnValue > 180)
                {
                    ChangePingIconColor(new Color32(224, 40, 40, 255));
                }
            });

            yield return new WaitForSeconds(1F);
        }
    }

    //Change Color of Signal Icon
    private void ChangePingIconColor(Color color) 
    {
        if(countText.color != color) 
        {
            countText.color = color;
        }
    }
}
