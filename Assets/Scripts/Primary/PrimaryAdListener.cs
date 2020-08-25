﻿using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

public class PrimaryAdListener : MonoBehaviour, IUnityAdsListener
{
    private GameServer gameServer;
    private string gameId = "3507995";
    private string myPlacementId = "rewardedVideo";
    private bool testMode = true;
    private string authKey;

    void Start()
    {
        gameServer = GameServer.singleton;
        authKey = PlayerPrefs.GetString("authKey");
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, testMode);
    }
    public void ShowAd()
    {
        Advertisement.Show(myPlacementId);
    }
    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (SceneManager.GetActiveScene().name == "Primary") 
        {
            if (showResult == ShowResult.Finished)
            {
                gameServer.RequestToRespawn(authKey);
            }
            else if (showResult == ShowResult.Skipped)
            {
                gameServer.RequestToRespawn(authKey);
            }
            else if (showResult == ShowResult.Failed)
            {
                gameServer.RequestToRespawn(authKey);
            }
        }
    }

    public void OnUnityAdsReady(string placementId)
    {
        // If the ready Placement is rewarded, show the ad:
        if (placementId == myPlacementId)
        {
            //Advertisement.Show(myPlacementId);
        }
    }

    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        // Optional actions to take when the end-users triggers an ad.
    }
}