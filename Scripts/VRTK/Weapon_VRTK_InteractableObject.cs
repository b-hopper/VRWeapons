using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class Weapon_VRTK_InteractableObject : VRTK_InteractableObject
{
    Weapon thisWeap;

    VRTK_ControllerReference currentController;
    
    [Tooltip("Main collider of the weapon, used for grabbing. Assign collider to disable it on pickup.\n\nIf this collider is not assigned, bolt manipulation " +
        "may not function correctly."), SerializeField]
    private Collider weaponBodyCollider;
    
    [Tooltip("Strength of haptics on weapon fire, 0 to 1."), SerializeField, Range(0f, 1f)]
    float hapticStrength = 1;

    [Tooltip("Duration of haptic effects per shot, in seconds."), SerializeField]
    float hapticDuration = 0.2f;

    [Tooltip("Time between haptic pulses, in seconds."), SerializeField]
    float hapticPulseInterval = 0.01f;


    private void Start()
    {
        thisWeap = GetComponent<Weapon>();
        if (weaponBodyCollider == null)
        {
            Debug.LogWarning("No main collider found, please assign Weapon Body Collider in inspector.");
        }

        thisWeap.shotHaptics += ThisWeap_shotHaptics;

        CheckForControllerAliases();
    }

    private void ThisWeap_shotHaptics()
    {
        if(currentController != null)
        { 
            VRTK_ControllerHaptics.TriggerHapticPulse(currentController, hapticStrength, hapticDuration, hapticPulseInterval);
        }
        else
        {
            Debug.LogError("Failed to trigger haptics, current controller not found");
        }
    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        VRW_ControllerActions_VRTK f;
        f = e.interactingObject.GetComponent<VRW_ControllerActions_VRTK>();
        if (e.interactingObject != GetSecondaryGrabbingObject())
        {
            currentController = VRTK_ControllerReference.GetControllerReference(e.interactingObject);
        }
        
        if (f != null)
        {
            f.CurrentHeldWeapon = thisWeap;             // Setting up for touchpad input
        }

        thisWeap.holdingDevice = e.interactingObject;

        base.OnInteractableObjectGrabbed(e);
        SetColliderEnabled(false);
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        VRW_ControllerActions_VRTK f;
        f = e.interactingObject.GetComponent<VRW_ControllerActions_VRTK>();
        if (e.interactingObject == GetGrabbingObject())
        {
            currentController = null;
        }
        if (f != null)
        {
            f.CurrentHeldWeapon = null;
        }

        thisWeap.holdingDevice = null;

        SetColliderEnabled(true);
        base.OnInteractableObjectUngrabbed(e);
    }

    public override void StartUsing(VRTK_InteractUse usingObject)
    {
        base.StartUsing(usingObject);
        thisWeap.StartFiring(usingObject.gameObject);
    }

    public override void StopUsing(VRTK_InteractUse previousUsingObject)
    {
        base.StopUsing(previousUsingObject);
        thisWeap.StopFiring(previousUsingObject.gameObject);
    }

    void CheckForControllerAliases()
    {
        VRTK_SDKManager tmp = FindObjectOfType<VRTK_SDKManager>();
        if (tmp != null)
        {
            if (tmp.scriptAliasLeftController != null)
            {
                if (tmp.scriptAliasLeftController.GetComponent<VRW_ControllerActions_VRTK>() == null)
                {
                    tmp.scriptAliasLeftController.AddComponent<VRW_ControllerActions_VRTK>();
                    Debug.LogWarning("No VRW_ControllerActions_VRTK found on " + tmp.scriptAliasLeftController + ". Adding component. Please add component in editor.");
                }
            }
            else
            {
                Debug.LogError("No left controller alias found. Please assign one in the VRTK SDK Manager.");
            }
            if (tmp.scriptAliasRightController != null)
            {
                if (tmp.scriptAliasRightController.GetComponent<VRW_ControllerActions_VRTK>() == null)
                {
                    tmp.scriptAliasRightController.AddComponent<VRW_ControllerActions_VRTK>();
                    Debug.LogWarning("No VRW_ControllerActions_VRTK found on " + tmp.scriptAliasRightController + ". Adding component. Please add component in editor.");
                }
            }
            else
            {
                Debug.LogError("No right controller alias found. Please assign one in the VRTK SDK Manager.");
            }
        }
    }

    public void SetWeaponBodyCollider(Collider collider)
    {
        weaponBodyCollider = collider;
    }

    public void SetColliderEnabled(bool isEnabled)
    {
        if (weaponBodyCollider != null)
        {
            weaponBodyCollider.enabled = isEnabled;
        }
    }
}
