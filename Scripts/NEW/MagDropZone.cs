using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class MagDropZone : VRTK_SnapDropZone {
    VRWeapons.Weapon thisWeap;

    private void Start()
    {
        GameObject mag;
        thisWeap = GetComponentInParent<VRWeapons.Weapon>();
        Debug.Log(thisWeap);

        if ((thisWeap.GetComponentInChildren<Magazine_VRTK_InteractableObject>()) != null) {
            mag = thisWeap.GetComponentInChildren<Magazine_VRTK_InteractableObject>().gameObject;
            ForceSnap(mag);
        }
    }

    public override void OnObjectSnappedToDropZone(SnapDropZoneEventArgs e)
    {
        IMagazine mag = e.snappedObject.GetComponent<IMagazine>();
        base.OnObjectSnappedToDropZone(e);
        
        mag.MagIn(thisWeap);
    }

    public override void OnObjectUnsnappedFromDropZone(SnapDropZoneEventArgs e)
    {
        base.OnObjectUnsnappedFromDropZone(e);

        thisWeap.Magazine = null;
    }

}
