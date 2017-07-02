using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class Weapon_VRTK_InteractableObject : VRTK_InteractableObject
{
    Weapon thisWeap;
    [Tooltip("Main collider of the weapon, used for grabbing. Assign collider to disable it on pickup.\n\nIf this collider is not assigned, bolt manipulation " +
        "may not function correctly."), SerializeField]
    public Collider weaponBodyCollider;

    [Tooltip("Second hand grip collider is used for 2-handed weapons. This collider will NOT be turned off when weapon is picked up."), SerializeField]
    Collider secondHandGripCollider;

    private void Start()
    {
        thisWeap = GetComponent<Weapon>();
        if (weaponBodyCollider == null)
        {
            Debug.LogWarning("No main collider found, please assign Weapon Body Collider in inspector.");
        }
    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        VRW_ControllerActions_VRTK f;
        f = e.interactingObject.GetComponent<VRW_ControllerActions_VRTK>();
        if (f != null)
        {
            f.CurrentHeldWeapon = thisWeap;             // Setting up for touchpad input
        }

        thisWeap.holdingDevice = e.interactingObject;

        base.OnInteractableObjectGrabbed(e);
        weaponBodyCollider.enabled = false;
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        VRW_ControllerActions_VRTK f;
        f = e.interactingObject.GetComponent<VRW_ControllerActions_VRTK>();
        if (f != null)
        {
            f.CurrentHeldWeapon = null;
        }

        thisWeap.holdingDevice = null;

        base.OnInteractableObjectUngrabbed(e);        
        weaponBodyCollider.enabled = true;
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
}
