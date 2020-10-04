﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatteryIcon : MonoBehaviour
{
    float batteryLevel = 1;
    bool isCharging = false;
    private bool showBattery = false;
    public Settings settings;

    [SerializeField] Image bkgIcon;
    [SerializeField] RectTransform rect;

    public GameObject batteryObject;


    private void Update() 
    {
        if (isCharging != (BatteryStatus.Charging == SystemInfo.batteryStatus))
        {
            isCharging = (BatteryStatus.Charging == SystemInfo.batteryStatus);
            if (batteryLevel != SystemInfo.batteryLevel)
            {
                batteryLevel = SystemInfo.batteryLevel;
                UpdateBatteryIcon();
            }
        }
        else if (batteryLevel != SystemInfo.batteryLevel)
        {
            batteryLevel = SystemInfo.batteryLevel;
            UpdateBatteryIcon();
        }
    }


    private void UpdateBatteryIcon() 
    {
        bkgIcon.color = GetColorFromLevel();
        rect.localScale = new Vector3(1,1,1);
    }


    private Color32 GetColorFromLevel()
    {
        
        if (!isCharging) 
        {
            if (batteryLevel < 0.20)
            {
                return new Color32(224, 40, 40, 255);
            }
            else if (batteryLevel < 0.35)
            {
                return new Color32(224, 224, 40, 255);
            }
            else
            {
                return new Color32(134, 224, 40, 255);
            }
        }
        else 
        {
            return new Color32(134, 224, 40, 255);
        }
    }



    private void Start()
    {
        Change();
    }


    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;//Subscribe to Settings Change Event.
    }

    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;//unSubscribe to Settings Change Event.
    }

    //Change Settings.
    private void Change()
    {
        showBattery = settings.showBattery;
        if (showBattery && batteryObject.activeSelf)
        {
            //A ok
        }
        else if (showBattery)
        {
            batteryObject.SetActive(true);
        }
        else
        {
            batteryObject.SetActive(false);
        }
    }


}
