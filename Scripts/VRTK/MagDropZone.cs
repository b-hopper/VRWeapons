using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class MagDropZone : VRTK_SnapDropZone
{
    Weapon thisWeap;

    private void Start()
    {
        thisWeap = GetComponentInParent<Weapon>();
    }

    public override void OnObjectSnappedToDropZone(SnapDropZoneEventArgs e)
    {
        IMagazine mag = e.snappedObject.GetComponent<IMagazine>();
        mag.MagIn(thisWeap);
        mag.MagDropped += Mag_MagDropped;
        base.OnObjectSnappedToDropZone(e);
    }

    public override void OnObjectUnsnappedFromDropZone(SnapDropZoneEventArgs e)
    {
        IMagazine mag = e.snappedObject.GetComponent<IMagazine>();
        //Stop listening for mag drop event so we won't redundantly unsnap
        mag.MagDropped -= Mag_MagDropped;
        mag.MagOut(thisWeap);

        //This is necessary for the initial mag so it won't revert to child of weapon
        var interactable = e.snappedObject.GetComponent<VRTK_InteractableObject>();
        if(interactable != null)
        { 
            interactable.SaveCurrentState();
        }

        base.OnObjectUnsnappedFromDropZone(e);
    }

    private void Mag_MagDropped(object sender, System.EventArgs e)
    {
        if (sender is IMagazine)
        {
            (sender as IMagazine).MagDropped -= Mag_MagDropped;
        }
        ForceUnsnap();
    }

}