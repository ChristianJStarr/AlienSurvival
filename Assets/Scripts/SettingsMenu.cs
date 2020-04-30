﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Settings settings;
    public Slider ui, menu, ambient, effects, xSense, ySense, terrainDis, objectDis;
    public ToggleGroup textureQ, shadowQ, reflectionQ, objectQ;


    void Start() 
    {
        GetSettings();
    }
    private void GetSettings() 
    {
        ui.value = settings.uiVolume;
        menu.value = settings.musicVolume;
        ambient.value = settings.ambientVolume;
        effects.value = settings.effectsVolume;

        SetToggle(textureQ, settings.textureQuality);
        SetToggle(shadowQ, settings.shadowQuality);
        SetToggle(reflectionQ, settings.reflectionQuality);
        SetToggle(objectQ, settings.objectQuality);

        xSense.value = settings.xSensitivity;
        ySense.value = settings.ySensitivity;

        terrainDis.value = settings.terrainDistance;
        objectDis.value = settings.objectDistance;
    }

    private void SetToggle(ToggleGroup group, int value) 
    {
       foreach(Toggle item in group.GetComponentsInChildren<Toggle>()) 
        {
            group.SetAllTogglesOff();
            if(Convert.ToInt32(item.name) == value) 
            {
                Debug.Log(item.name);
                item.isOn = true;
                break;
            }
        }
    }


    //Called from Terrain Distance Slider in settings. Keeps Object Max Distance >= Terrain Distance
    public void TerrainDistance() 
    {
        objectDis.maxValue = terrainDis.value;
        if(objectDis.value > terrainDis.value) 
        {
            objectDis.value = terrainDis.value;
        }
    }

    public void ApplySettings() 
    {
        settings.uiVolume = ui.value;
        settings.musicVolume = menu.value;
        settings.ambientVolume = ambient.value;
        settings.effectsVolume = effects.value;

        settings.textureQuality = Convert.ToInt32(textureQ.ActiveToggles().FirstOrDefault().name);
        settings.shadowQuality = Convert.ToInt32(shadowQ.ActiveToggles().FirstOrDefault().name);
        settings.reflectionQuality = Convert.ToInt32(reflectionQ.ActiveToggles().FirstOrDefault().name);
        settings.objectQuality = Convert.ToInt32(objectQ.ActiveToggles().FirstOrDefault().name);

        settings.xSensitivity = xSense.value;
        settings.ySensitivity = ySense.value;

        settings.terrainDistance = terrainDis.value;
        settings.objectDistance = objectDis.value;

        MusicManager music = FindObjectOfType<MusicManager>();
        TouchPad touch = FindObjectOfType<TouchPad>();
        if(music != null) 
         {
            music.Change();
        }
        if(touch != null) 
        {
            touch.Change();
        }
    }
}
