﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectedItemHandler : MonoBehaviour
{
    public Item selectedItem;
    public ItemData selectedItemData;
    private InventoryGfx inventory;

    private GameObject holdItem;
    private List<GameObject> holdItemCache;

    public Transform aimTransform;
    public Animator animator;
    public Transform handAnchor;
    private GameObject rightHand;
    private GameObject leftHand;
    private ControlControl controls;
    private GameServer gameServer;
    private PlayerActionManager actionManager;
    private PlayerInfoManager infoManager;

    private bool ikActive = false;
    private int selectedSlot = 1;
    private bool isHoldingUse = false;

    private void Start()
    {
        actionManager = PlayerActionManager.singleton;
        infoManager = PlayerInfoManager.singleton;
        gameServer = GameServer.singleton;
        holdItemCache = new List<GameObject>();
        inventory = FindObjectOfType<InventoryGfx>();
        controls = inventory.GetComponent<ControlControl>();

    }
    private void Update()
    {
        if (Input.GetButtonDown("Use")) 
        {
            Use();
            isHoldingUse = true;
        }
        if (Input.GetButtonUp("Use"))
        {
            isHoldingUse = false;
        }
    }


    //Update the Selected Slot
    public void UpdateSelectedSlot() 
    {
        SelectSlot(selectedSlot);
    }

    //Use Selected Item
    public void Use() 
    {
        if (selectedItem == null || selectedItem.itemID == 0)
        {
            animator.SetTrigger("Punch");
        }
        else if (inventory != null && selectedItem.itemID == inventory.SelectSlot(selectedSlot).itemID)
        {

            Animator holdAnimator = holdItem.GetComponent<Animator>();
            ItemData data = gameServer.GetItemDataById(selectedItem.itemID);
            if (data.useRequire.Length > 0)
            {
                string[] strings = data.useRequire.Split('-');
                int itemId = Convert.ToInt32(strings[0]);
                int itemAmount = Convert.ToInt32(strings[1]);
                if (itemId > 0 && itemAmount > 0)
                {
                    infoManager.GetIfEnoughItems(itemId, itemAmount, returnValue =>
                    {
                        if (returnValue) 
                        {
                            if (holdAnimator != null)
                            {
                                holdAnimator.SetTrigger("Use");
                                actionManager.UseSelectedItem(data, aimTransform);
                            }
                        }
                    });
                }
            }
            else 
            {
                if (holdAnimator != null)
                {
                    holdAnimator.SetTrigger("Use");
                }
            }
        }
    }

    //Select a Slot (int)
    public void SelectSlot(int slot)
    {

        if(inventory == null) 
        {
            inventory = FindObjectOfType<InventoryGfx>();
        }
        if (selectedSlot != slot) 
        {
            inventory.SelectedItemHandover(this);
            selectedSlot = slot;
            selectedItem = inventory.SelectSlot(slot);
            if(selectedItem != null) 
            {
                if (selectedItem.isHoldable)
                {
                    HoldItem();
                }
                else
                {
                    if (holdItem != null)
                    {
                        holdItem.SetActive(false);
                        holdItemCache.Add(holdItem);
                        leftHand = null;
                        rightHand = null;
                    }
                }
            }
            else 
            {
                
                if(controls != null) 
                {
                    controls.SwapUse(0);
                }
                if (holdItem != null)
                {
                    holdItem.SetActive(false);
                    holdItemCache.Add(holdItem);
                    leftHand = null;
                    rightHand = null;
                }
            }
        }
        else 
        {
            if (selectedItem != null)
            {
                if (selectedItem.isHoldable)
                {
                    HoldItem();
                }
                else
                {
                    if (holdItem != null)
                    {
                        holdItem.SetActive(false);
                        holdItemCache.Add(holdItem);
                        leftHand = null;
                        rightHand = null;
                    }
                }
            }
            else
            {

                if (controls != null)
                {
                    controls.SwapUse(0);
                }
                if (holdItem != null)
                {
                    holdItem.SetActive(false);
                    holdItemCache.Add(holdItem);
                    leftHand = null;
                    rightHand = null;
                }
            }
        }
    }

    //Hold Item Function
    private void HoldItem() 
    {
        if (holdItem != null)
        {
            holdItem.SetActive(false);
            holdItemCache.Add(holdItem);
            leftHand = null;
            rightHand = null;
        }
        selectedItemData = inventory.FindItemData(selectedItem.itemID);
        if(controls != null) 
        {
            controls.SwapUse(selectedItemData.useType);
        }
        bool selected = false;
        foreach (GameObject obj in holdItemCache)
        {
            if (obj.name == selectedItemData.holdableObject.name + "(Clone)")
            {
                obj.SetActive(true);
                selected = true;
                UpdateTargets();
                holdItem = obj;
                break;
            }
        }

        if (handAnchor != null && selected == false)
        {
            holdItem = Instantiate(selectedItemData.holdableObject, handAnchor);
            UpdateTargets();
        }
    }

    //Update Animator Targets
    private void UpdateTargets()
    {
        leftHand = GameObject.FindGameObjectWithTag("LHandTarget");
        rightHand = GameObject.FindGameObjectWithTag("RHandTarget");
        ikActive = true;
    }
    
    //On Animator Ik
    void OnAnimatorIK(int index)
    {
        if (index == 2) 
        {
            if (animator)
            {
                if (ikActive)
                {

                    if (rightHand != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHand.transform.position);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHand.transform.rotation);
                    }
                    else
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    }
                    if (leftHand != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.transform.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHand.transform.rotation);
                    }
                    else
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                    }
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                }
            }
        }
        
    }
}
