﻿using MLAPI;
using UnityEngine;

public class PlayerControlObject : NetworkedBehaviour
{
    public ulong owner_clientId;
    public ulong owner_networkId;




    public CharacterController characterController;
    public Transform cameraObject;
    public Transform cameraPoint;


    //--------------ANIMATING----------------
    public Animator animator;
    public Vector2 lastAnimationVector;
    private bool ikActive = false;
    public float ikArmWeight = 0.75F;

    //----------------STATES-----------------
    public bool use = false;
    public bool crouching = false;
    public bool jumping = false;

    //----------MOVING / ROTATING------------
    public bool isGrounded = true;
    private bool needsMoveCorrection = false;
    public float gravity = 0;
    private float moveSpeed = 200;
    public float jumpHeight = .7F;
    public Vector3 moveTarget = Vector3.zero;
    public Vector2 lookTarget = Vector3.zero;
    public Vector3 correctionMove = Vector3.zero;
    public CollisionFlags collisionFlags;

    //------------HOLDING OBJECT-------------
    public int holdableId = 0;
    public int holdableState = 0;
    public HoldableObject holdableObject;
    public Transform handParent;
    //Hand Targets
    private Transform rightHand;
    private Transform leftHand;

    //------------SELECTED SLOT---------------
    public int selectedSlot = 0;
    public Item selectedItem;

    //----------------TIMES-------------------
    public float useDelayTime = 0;

    //------------RAGDOLL COLLIDERS-----------
    public CapsuleCollider[] ragdoll_capsules;
    public SphereCollider ragdoll_sphere;
    public BoxCollider[] ragdoll_boxes;


    private void Start() 
    {
        gravity = Physics.gravity.y;
        SetRagdollColliders(false);
    }
    //Convert this ControlObject to Snapshot_Player Format
    public Snapshot_Player ConvertToSnapshot()
    {
        return new Snapshot_Player()
        {
            networkId = NetworkId,
            location = transform.position,
            holdId = holdableId,
            holdState = holdableState,
            rotation = new Vector2(cameraObject.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y)
        };
    }




    //Start Called on Spawn
    public override void NetworkStart()
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)

#elif UNITY_EDITOR
        if (IsClient)
        {
            WorldSnapshotManager.RegisterObject(this);
        }
#else
        WorldSnapshotManager.RegisterObject(this);
#endif
    }

    //Destroy
    public void OnDestroy()
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)

#elif UNITY_EDITOR
        if (IsClient)
        {
            WorldSnapshotManager.RemoveObject(NetworkId);
        }
#else
        WorldSnapshotManager.RemoveObject(NetworkId);
#endif
    }

    private void FixedUpdate()
    {
        if (needsMoveCorrection) 
        {
            CorrectionMoveTask();
        }
    }

    //Teleport
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        ApplyCorrection(position);
        Rotate(new Vector2(rotation.eulerAngles.x, rotation.eulerAngles.y));
    }

    //Rotate this Object from Axis
    public void Rotate(Vector2 lookAxis)
    {
        Quaternion m_CharacterTargetRot = Quaternion.Euler(0f, lookAxis.y, 0f);
        if (transform.rotation != m_CharacterTargetRot)
        {
            transform.rotation = m_CharacterTargetRot;
        }
        Quaternion m_CameraTargetRot = Quaternion.Euler(lookAxis.x, 0f, 0f);
        if (cameraObject.localRotation != m_CameraTargetRot)
        {
            cameraObject.localRotation = m_CameraTargetRot;
        }
    }

    //Move this Object from Axis
    public void Move(Vector2 moveAxis, bool jump, bool crouch)
    {
        if (needsMoveCorrection) return;
        isGrounded = characterController.isGrounded;
        if (isGrounded)
        {
            if (jumping)
            {
                jumping = false;
            }
            if (crouch)
            {
                if (crouching) 
                {
                    crouching = false;
                }
                else if (!jump) 
                {
                    crouching = true;
                }
            }
        }
        Vector3 forward = transform.forward * ((moveSpeed * moveAxis.y) * Time.deltaTime);
        Vector3 right = transform.right * ((moveSpeed * moveAxis.x) * Time.deltaTime);
        Vector3 movement = (forward + right);
        //movement = movement.normalized;
        if (jump && isGrounded && !jumping)
        {
            jumping = true;
        }
        movement.y -= -gravity * 3 * Time.deltaTime;
        if (IsClient) DebugMenu.UpdateMovement(transform.position, transform.rotation.eulerAngles, characterController.velocity, forward, right, movement * Time.deltaTime);
        characterController.Move(movement * Time.deltaTime);
    }

    //Animate from Axis
    public void Animate(Vector2 animateAxis)
    {
        lastAnimationVector = animateAxis;
        if (animator != null)
        {
            if (animator.GetFloat("vertical") != animateAxis.y)
            {
                animator.SetFloat("vertical", animateAxis.y);
            }
            if (animator.GetFloat("horizontal") != animateAxis.x)
            {
                animator.SetFloat("horizontal", animateAxis.x);
            }
        }
    }

    //Apply Rotation Correction
    public void ApplyCorrection(Vector3 position) 
    {
        correctionMove = position;
        needsMoveCorrection = true;
    }

    private void CorrectionMoveTask()
    {
        if (correctionMove != Vector3.zero)
        {
            float distance = Vector3.Distance(correctionMove, transform.position);
            if (distance > 2) 
            {
                transform.position = correctionMove;
            }
            else if(distance > 0.01F)
            {
                transform.position = Vector3.Lerp(transform.position, correctionMove, 10 * Time.fixedDeltaTime);
                if (transform.position == correctionMove)
                {
                    correctionMove = Vector3.zero;
                    needsMoveCorrection = false;
                }
            }
            else 
            {
                correctionMove = Vector3.zero;
                needsMoveCorrection = false;
            }

        }
        else { needsMoveCorrection = false; }
    }

    




    //-------------Player Ragdoll---------------

    public void ToggleRagdoll(bool enable) 
    {
        SetRagdollColliders(enable);
        animator.enabled = !enable;
    }

    private void SetRagdollColliders(bool enable) 
    {
        //Legs & Arms
        for (int i = 0; i < ragdoll_capsules.Length; i++)
        {
            ragdoll_capsules[i].enabled = enable;
        }
        //Chest & Butt
        for (int i = 0; i < ragdoll_boxes.Length; i++)
        {
            ragdoll_boxes[i].enabled = enable;
        }
        //Head
        ragdoll_sphere.enabled = enable;
    }



    //------------Player Inverse K--------------

    public void SetHandIK(Transform hand, bool left) 
    {
        if (left) 
        {
            leftHand = hand;
        }
        else 
        {
            rightHand = hand;
        }
        ikActive = true;
    }

    void OnAnimatorIK(int index)
    {
        if (ikActive && index == 0 && animator)
        {
            if (holdableId != 0)
            {
                if (rightHand != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikArmWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikArmWeight);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHand.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHand.rotation);
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                }
                if (leftHand != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikArmWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikArmWeight);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHand.rotation);
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                }
            }
            else
            {
                ikActive = false;
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }
        }
    }

}