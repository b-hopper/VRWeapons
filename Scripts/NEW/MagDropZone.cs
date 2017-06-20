using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class MagDropZone : VRTK_SnapDropZone {
    VRWeapons.Weapon thisWeap;

    private void Start()
    {
        thisWeap = GetComponentInParent<VRWeapons.Weapon>();
    }

    public override void OnObjectSnappedToDropZone(SnapDropZoneEventArgs e)
    {
        base.OnObjectSnappedToDropZone(e);
        
        e.snappedObject.GetComponent<IMagazine>().MagIn(thisWeap);
        thisWeap.ChamberNewRound(e.snappedObject.GetComponent<IMagazine>().FeedRound());    // TEMPORARY, to be handled by bolt actions later
    }

    public override void OnObjectUnsnappedFromDropZone(SnapDropZoneEventArgs e)
    {
        base.OnObjectUnsnappedFromDropZone(e);

        thisWeap.Magazine = null;
    }

}
