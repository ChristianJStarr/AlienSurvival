﻿using UnityEngine;
using TMPro;
public class MainMenuServerSlide : MonoBehaviour
{
    //What: The individual slide in the server list.
    //Where: Main Menu Scene / Server Menu
    
    public TextMeshProUGUI slideName, slidePing, slideType, slideMode, slidePlayers;
    public int serverPlayers, serverMaxPlayers;
    public ushort serverPort;
    public CoinManager coinManager;
    public GameObject roomNotify;
    public Server storedServer;

    //Update Values of this Slide
    public void RefreshValues(Server server) 
    {
        slideName.text = server.name;
        slidePing.text = server.ping + "ms";
        slideType.text = server.description;
        slideMode.text = server.mode;
        slidePlayers.text = "(" + server.player + "/" + server.maxPlayer + ")";
        storedServer = server;
    }

    //Button: Join this Room
    public void JoinThisRoom() 
    {
        ClickSound();
        ServerConnect serverConnect = FindObjectOfType<ServerConnect>();
        if(serverConnect != null) 
        {
            serverConnect.ConnectToServer(storedServer.serverIP, storedServer.serverPort);
        }
    }
    
    //Play UI Click Sound
    private void ClickSound() 
    {
        MusicManager sounds = FindObjectOfType<MusicManager>();
        if (sounds != null)
        {
            sounds.ButtonClick();
        }
    }
}
