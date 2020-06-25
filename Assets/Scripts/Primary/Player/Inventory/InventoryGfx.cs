﻿using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class InventoryGfx : MonoBehaviour
{
    public TopToolTipHandler toolTipHandler;
    public GameObject inventoryUI, inventoryBkg, playerMenu, craftMenu, bounds, bounds2, bounds3, hotBarButtons, tint, playerViewCamera;
    public ControlControl controls;
    public Transform itemsParent, armorSlotsContainer, hotBarParent;
    public Slider splitSlider;
    public TextMeshProUGUI splitText;
    public Item toolTipItem;
    public ItemSlot[] itemSlots, armorSlots;
    public bool invOpen = false;
    FirstPersonController fps;
    CraftingMenu craftingMenu;
    Transform[] holds;

    private SelectedItemHandler selectedHandler;

    private Item[] items;
    private Item[] armor;
    private int[] blueprints;
    private ItemData[] allItems;

    private bool craftingActive;
    private bool armorActive;

    public RectTransform armorRect;
    public RectTransform craftingRect;

    private Vector2 armorTarget = new Vector3(-354F, -41.99303F);
    private Vector2 craftingTarget = new Vector3(2997, -41.99601F);

    private bool armorMove;
    private bool craftingMove;
    
    
    private void Start()
    {

        allItems = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
        craftingMenu = GetComponent<CraftingMenu>();
        ItemSlot[] itemSlotsTemp = itemsParent.GetComponentsInChildren<ItemSlot>(true);
        List<ItemSlot> hotBarSlotsTemp = hotBarParent.GetComponentsInChildren<ItemSlot>(true).ToList();
        armorSlots = armorSlotsContainer.GetComponentsInChildren<ItemSlot>(true);
        foreach (ItemSlot slot in itemSlotsTemp)
        {
            hotBarSlotsTemp.Add(slot);
        }
        itemSlots = hotBarSlotsTemp.ToArray();
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].slotNumber = i + 1;
        }
        for (int i = 0; i < armorSlots.Length; i++)
        {
            armorSlots[i].slotNumber = i + 34;
        }
    }
    private void Update()
    {
        UpdateMenus();
    }


    //Update Player Info
    public void Incoming(PlayerInfo playerInfo)
    {
        
        items = playerInfo.items;
        armor = playerInfo.armor;
        blueprints = playerInfo.blueprints;
        if(craftingMenu == null) 
        {
            craftingMenu = GetComponent<CraftingMenu>();
        }
        craftingMenu.GetResources(items, blueprints);
        UpdateUI();
        if (selectedHandler != null)
        {
            selectedHandler.UpdateSelectedSlot();
        }
    }

    //Button: Crafting Menu
    public void ButtonCraftingMenu()
    {
        craftingActive = !craftingActive;
        SlideMenu(true, craftingActive);
        if (armorActive) 
        {
            armorActive = false;
            SlideMenu(false, false);
        }
    }
    
    //Button: Armor Menu
    public void ButtonArmorMenu() 
    {
        armorActive = !armorActive;
        SlideMenu(false, armorActive);
        if (craftingActive)
        {
            craftingActive = false;
            SlideMenu(true, false);
        }
    }

    //Handover Selected Item
    public void SelectedItemHandover(SelectedItemHandler handler) 
    {
        selectedHandler = handler;
    }

    //Update Menu Positions
    private void UpdateMenus() 
    {
        if(armorMove) 
        {
            if (armorRect.anchoredPosition != armorTarget)
            {
                armorRect.anchoredPosition = Vector2.MoveTowards(armorRect.anchoredPosition, armorTarget, 6000 * Time.deltaTime);
            }
            else 
            {
                armorMove = false;
                if (armorActive) 
                {
                    tint.SetActive(true);
                }
                else
                {
                    tint.SetActive(false);
                }
            }
        }
        if(craftingMove) 
        {
            if (craftingRect.anchoredPosition != craftingTarget)
            {
                craftingRect.anchoredPosition = Vector2.MoveTowards(craftingRect.anchoredPosition, craftingTarget, 6000 * Time.deltaTime);
            }
            else 
            {
                craftingMove = false;
                if (craftingActive)
                {
                    tint.SetActive(true);
                }
                else 
                {
                    tint.SetActive(false);
                }
            }
        }
    }

    //Slide Menus 
    private void SlideMenu(bool craftingMenu, bool state) 
    {
        if (craftingMenu) 
        {
            Vector2 target;
            if (state) 
            {
                target = new Vector2(1943, -41.99601F);
            }
            else 
            {
                target = new Vector2(2997, -41.99601F);
            }
            craftingTarget = target;
            craftingMove = true;
        }
        else 
        {
            Vector2 target;
            if (state)
            {
                target = new Vector2(489.6F, -41.99303F);
            }
            else
            {
                target = new Vector2(-354F, -41.99303F);
            }
            armorTarget = target;
            armorMove = true;
        }
    }

    //Active Top Tip (item)
    public void ActivateToolTip(Item item) 
    {
        if (!toolTipHandler.gameObject.activeSelf)
        {
            toolTipHandler.gameObject.SetActive(true);
        }
        toolTipHandler.SetData(FindItemData(item.itemID), item);
    }

    //Select a slot
    public Item SelectSlot(int slot) 
    {
        Item item = null;
        for (int i = 0; i < 6; i++)
        {
            if (itemSlots[i].slotNumber == slot) 
            {
                itemSlots[i].Selected(true);
                item = itemSlots[i].item;
            }
            else 
            {
                itemSlots[i].Selected(false);
            }
        }
        return item;
    }

    //Button: Inventory Open/Close
    public void InvButton() 
    {
        

        if (invOpen)
        {
            invOpen = false;
            controls.Show();
            toolTipHandler.gameObject.SetActive(false);
            
            if(playerViewCamera != null) 
            {
                playerViewCamera.SetActive(false);
            }
        }
        else
        {
            controls.Hide();
            invOpen = true;
            
            if (playerViewCamera != null)
            {
                playerViewCamera.SetActive(true);
            }
        }
        hotBarButtons.SetActive(!invOpen);
        inventoryBkg.SetActive(invOpen);
        playerMenu.SetActive(invOpen);
        craftMenu.SetActive(invOpen);
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i > 5)
            {
                itemSlots[i].Toggle();
            }
        }
        for (int i = 0; i < armorSlots.Length; i++)
        {
            armorSlots[i].Toggle();  
        }
    }

    //Update the UI
    private void UpdateUI()
    {
        foreach (ItemSlot slot in itemSlots)
        {
            slot.ClearSlot();
        }
        foreach (ItemSlot slot in armorSlots)
        {
            slot.ClearSlot();
        }

        if (items != null)
        {
            foreach (Item item in items)
            {
                foreach (ItemSlot slot in itemSlots)
                {
                    if (item.currSlot == slot.slotNumber)
                    {
                        ItemData data = FindItemData(item.itemID);
                        if (data != null)
                        {
                            slot.AddItem(item, FindItemData(item.itemID));
                        }
                        break;
                    }
                }
            }
        }
        if (armor != null) 
        {
            foreach (Item item in armor)
            {
                foreach (ItemSlot slot in armorSlots)
                {
                    if (item.currSlot == slot.slotNumber)
                    {
                        ItemData data = FindItemData(item.itemID);
                        if(data != null)
                        {
                            slot.AddItem(item, FindItemData(item.itemID));
                        }
                        break;
                    }
                }
            }
        }
    }

    //Find ItemData by ID
    public ItemData FindItemData(int id) 
    {
        ItemData itemData = null;
        foreach (ItemData data in allItems) 
        {
            if(data.itemID == id) 
            {
                itemData = data;
                break;
            }   
        }
        return itemData;
    }
}