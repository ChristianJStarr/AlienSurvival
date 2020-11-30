﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableObject : MonoBehaviour
{
    public int id = 0;
    public int state = 0;

    public Animator animator;
    

    private void Start() 
    {
        animator = GetComponent<Animator>();
    }


    public void Use()
    {
        //To be overridden;
    }



}
