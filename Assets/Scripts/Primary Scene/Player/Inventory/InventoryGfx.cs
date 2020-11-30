﻿using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

public class InventoryGfx : MonoBehaviour
{
    public UI_Tooltip ui_tooltip;
    public GameObject inventoryUI, inventoryBkg, bounds, bounds2, bounds3, hotBarButtons, tint, tint2, playerViewCamera, storageCrateSlotContainer;
    public ControlControl controls;
    public Transform itemsParent, armorSlotsContainer, hotBarParent;
    public Image buttonIcon;
    public Item toolTipItem;
    public ItemSlot[] itemSlots, armorSlots, storageCrateSlots;
    public bool invOpen = false;
    CraftingMenu craftingMenu;
    Transform[] holds;

    private Item[] items;
    private Item[] armor;
    private int[] blueprints;


    //Inventory UI Slide Menus
    private Vector2 leftTarget = new Vector3(-354F, -41.99303F);
    private Vector2 rightTarget = new Vector3(-618, -41.99601F);

    public InventorySlideModule[] rightSlideMenus;
    public RectTransform leftSlideMenus;

    private int activeSlideMenu;
    private bool rightMove = false;
    private bool leftMove = false;
    private bool leftActive = false;
    private bool rightActive = false;


    private bool checkingHover = false;
    //Extra UI Data
    private UIData storedUIData;

    private int uiType = 0;

    public Sprite closeIcon;
    private Sprite normalIcon;


    private void Start()
    {
        normalIcon = buttonIcon.sprite;
        craftingMenu = GetComponent<CraftingMenu>();
        ItemSlot[] itemSlotsTemp = itemsParent.GetComponentsInChildren<ItemSlot>(true);
        List<ItemSlot> hotBarSlotsTemp = hotBarParent.GetComponentsInChildren<ItemSlot>(true).ToList();
        armorSlots = armorSlotsContainer.GetComponentsInChildren<ItemSlot>(true);
        storageCrateSlots = storageCrateSlotContainer.GetComponentsInChildren<ItemSlot>(true);
        foreach (ItemSlot slot in itemSlotsTemp)
        {
            hotBarSlotsTemp.Add(slot);
        }
        itemSlots = hotBarSlotsTemp.ToArray();

        //Assign Slot_Numbers to Item Slots
        AssignSlotNumbersToSlots();
    }
    private void Update()
    {
        UpdateMenus();
    }


    public void Incoming(Item[] _items, Item[] _armor, int[] _blueprints) 
    {
        items = _items;
        armor = _armor;
        blueprints = _blueprints;
        if (craftingMenu == null)
        {
            craftingMenu = GetComponent<CraftingMenu>();
        }
        craftingMenu.GetResources(items, blueprints);
        UpdateUI();
    }

    //Assign Slot Numbers To Slots
    private void AssignSlotNumbersToSlots() 
    {
        int slotNumber = 1;
        for (int i = 0; i < itemSlots.Length; i++) //33 0-32
        {
            itemSlots[i].slotNumber = slotNumber; // 1-33
            slotNumber++;
        }
        for (int i = 0; i < armorSlots.Length; i++) //5 0-4
        {
            armorSlots[i].slotNumber = slotNumber; //34-38
            slotNumber++;
        }
        for (int i = 0; i < storageCrateSlots.Length; i++) //18 0-17
        {
            storageCrateSlots[i].slotNumber = slotNumber; //39-56
            slotNumber++;
        }
    }

    //Check the Hover Object
    public void CheckHoverObject()
    {
        if (HoveringOverLeftMenu())
        {
            if (!leftActive) 
            {
                ToggleSlideMenu(true);
            }
        }
        else if (HoveringOverRightMenu())
        {
            if (!rightActive) 
            {
                ToggleSlideMenu(false);
            }
        }
        else if (leftActive)
        {
            if (!checkingHover)
            {
                StartCoroutine(CheckHoverObjectWait());
            }
        }
        else if (rightActive) 
        {
            if (!checkingHover)
            {
                StartCoroutine(CheckHoverObjectWait());
            }
        }
    }

    //Wait for Check Hover Objecet
    private IEnumerator CheckHoverObjectWait() 
    {
        checkingHover = true;
        yield return new WaitForSeconds(.4F);
        if (!HoveringOverLeftMenu())
        {
            if (leftActive)
            {
                ToggleSlideMenu(true);
            }
        }
        if (!HoveringOverRightMenu())
        {
            if (rightActive)
            {
                ToggleSlideMenu(false);
            }
        }
        checkingHover = false;
    }

    //If Hovering Over Right Menu
    private bool HoveringOverRightMenu()
    {
        bool isHovering = false;
        if (rightSlideMenus != null && rightSlideMenus.Length > 0)
        {
            foreach (InventorySlideModule item in rightSlideMenus)
            {
                if (!isHovering && item.gameObject.activeSelf && item.allowActivateOnHover)
                {
                    isHovering = RectTransformUtility.RectangleContainsScreenPoint(item.rect, Input.mousePosition);
                }
            }
        }
        return isHovering;
    }

    //If Hovering Over Left Menu
    private bool HoveringOverLeftMenu()
    {
        return (RectTransformUtility.RectangleContainsScreenPoint(leftSlideMenus, Input.mousePosition));
    }

    //Button: Slide Menu
    public void ToggleSlideMenu(bool left)
    {
        if (left) 
        {
            SlideMenu(left, !leftActive);
            if (rightActive) 
            {
                SlideMenu(!left, false);
            }
        }
        else //right 
        {
            SlideMenu(left, !rightActive);
            if (leftActive)
            {
                SlideMenu(!left, false);
            }
        }
    }

    //Update Menu Positions
    private void UpdateMenus() 
    {
            if (leftMove)
            {
                if (leftSlideMenus.anchoredPosition != leftTarget)
                {
                    leftSlideMenus.anchoredPosition = Vector2.MoveTowards(leftSlideMenus.anchoredPosition, leftTarget, 6000 * Time.deltaTime);
                }
                else
                {
                    leftMove = false;
                }
            }
            if (rightMove)
            {
                RectTransform rect = rightSlideMenus[activeSlideMenu].rect;
                if (rect.anchoredPosition != rightTarget)
                {
                    rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition, rightTarget, 6000 * Time.deltaTime);
                }
                else
                {
                    rightMove = false;
                }
            }
            if (leftActive || rightActive)
            {
                tint2.SetActive(true);
            }
            else
            {
                tint2.SetActive(false);
            }
    }

    //Set Right Menu
    private void SetRightMenu(int menuId, bool active) 
    {
        activeSlideMenu = menuId;
        foreach (InventorySlideModule module in rightSlideMenus)
        {
            if (module.gameObject.activeSelf) 
            {
                module.gameObject.SetActive(false);
            }
        }
        rightSlideMenus[menuId].gameObject.SetActive(active);
    }

    //Slide Menus 
    private void SlideMenu(bool left, bool state) 
    {
        if (!left) //Right
        {
            InventorySlideModule module = rightSlideMenus[uiType];
            if (state) 
            {
                rightTarget = module.onPos;
            }
            else 
            {
                rightTarget = module.offPos;
            }
            rightMove = true;
            rightActive = state;
        }
        else //Left
        {
            
            if (state)
            {
                leftTarget = new Vector2(507, -38);
            }
            else
            {
                leftTarget = new Vector2(-354.1534F, -38);
            }
            leftMove = true;
            leftActive = state;
        }
    }

    //Active Top Tip (item)
    public void ActivateToolTip(Item item) 
    {
        if (!ui_tooltip.gameObject.activeSelf)
        {
            ui_tooltip.gameObject.SetActive(true);
        }
        ui_tooltip.SetData(ItemDataManager.Singleton.GetItemData(item.itemID), item);
    }

    //Set Slot to Focused
    public void SetSlotFocused(int slot) 
    {
        for (int i = 0; i < 6; i++)
        {
            itemSlots[i].Selected(itemSlots[i].slotNumber == slot);
        }
    }

    //Get Item from Slot
    public Item GetItemFromSlot(int slot) 
    {
        for (int i = 0; i < items.Length; i++)
        {
            if(items[i].currSlot == slot) 
            {
                return items[i];
            }
        }
        return null;
    }


    //Button: Inventory Open/Close
    public void InvButton(string data = null) 
    {
        uiType = 0;
        UIData uiData = null;
        if (!String.IsNullOrEmpty(data))
        {
            uiData = JsonUtility.FromJson<UIData>(data);
            if (uiData != null)
            {
                uiType = uiData.type;
                invOpen = false;
            }
        }



        SetRightMenu(uiType, !invOpen);
        activeSlideMenu = uiType;
        if (invOpen)
        {
            invOpen = false;
            controls.Show();
            ui_tooltip.gameObject.SetActive(false);
            SlideMenu(true, false);
            SlideMenu(false, false);
            tint.SetActive(false);
            if(playerViewCamera != null) 
            {
                playerViewCamera.SetActive(false);
            }
            buttonIcon.sprite = normalIcon;
        }
        else
        {
            controls.Hide();
            invOpen = true;
            
            if (playerViewCamera != null)
            {
                playerViewCamera.SetActive(true);
            }
            buttonIcon.sprite = closeIcon;
        }
        tint.SetActive(invOpen);
        hotBarButtons.SetActive(!invOpen);
        inventoryBkg.SetActive(invOpen);
        leftSlideMenus.gameObject.SetActive(invOpen);
        
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

        //Show StorageCrate
        if(uiType == 1) 
        {
            OpenStorageCrateUI(uiData);   
        }
        //Show Smelting
        if(uiType == 2)
        {
            OpenSmeltUI(uiData);
        }
    }

    //Close the Inventory if OPEN
    public void CloseInventory() 
    {
        if (invOpen) 
        {
            InvButton();
        }
    }

    //Open the Inventory if CLOSED
    public void OpenInventory() 
    {
        if (!invOpen) 
        {
            InvButton();
        }
    }


    //Update Extra UI Data
    public void UpdateExtraUIData(string data)
    {
        UIData uiData = JsonUtility.FromJson<UIData>(data);

        if(uiData.type == 1) //Storage
        {
            UpdateStorageCrateUI(uiData.itemArray);
        }
        else if (uiData.type == 2) //Smelt
        {
            UpdateSmeltUI(uiData);
        }
        else if (uiData.type == 3)
        {

        }
    }

    //Open UI Storage Crate
    public void OpenStorageCrateUI(UIData crateData) 
    {
        ToggleSlideMenu(false);
        UpdateStorageCrateUI(crateData.itemArray);
    }

    //Update UI Storage Crate
    private void UpdateStorageCrateUI(Item[] datas)
    {
        ClientInventoryTool.ClearSlots(storageCrateSlots);
        if (datas != null)
        {
            ClientInventoryTool.SortSlots(datas, storageCrateSlots);
        }
    }

    //Open UI Smelting
    private void OpenSmeltUI(UIData smeltData) 
    {
        
    }
   
    //Update UI Smelting
    private void UpdateSmeltUI(UIData smeltData) 
    {
    
    }

    //Update the Standard Inventory UI
    private void UpdateUI()
    {
        //Inventory Slots
        ClientInventoryTool.ClearSlots(itemSlots);
        if (items != null)
        {
            ClientInventoryTool.SortSlots(items, itemSlots);
        }

        //Armor Slots
        ClientInventoryTool.ClearSlots(armorSlots);
        if (armor != null) 
        {
            ClientInventoryTool.SortSlots(armor, armorSlots);
        }
    }


    public void Hide() 
    {
        inventoryUI.SetActive(false);
    }
    public void Show() 
    {
        inventoryUI.SetActive(true);
    }
}


public class ClientInventoryTool
{
    //TOOL: Sort Slots
    public static void SortSlots(Item[] items, ItemSlot[] slots)
    {
        foreach (Item item in items)
        {
            foreach (ItemSlot slot in slots)
            {
                if (item.currSlot == slot.slotNumber)
                {
                    ItemData data = ItemDataManager.Singleton.GetItemData(item.itemID);
                    if (data != null)
                    {
                        slot.AddItem(item, data);
                    }
                    break;
                }
            }
        }
    }

    //TOOL: Clear Slots
    public static void ClearSlots(ItemSlot[] slots) 
    {
        if(slots != null && slots.Length > 0) 
        {
            foreach (ItemSlot slot in slots)
            {
                slot.ClearSlot();
            }
        }
    }

}


public class UIData 
{
    public int type = 0;
    public Item[] itemArray = null;
}