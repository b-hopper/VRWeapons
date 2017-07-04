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
        base.OnObjectSnappedToDropZone(e);
    }

    public override void OnObjectUnsnappedFromDropZone(SnapDropZoneEventArgs e)
    {
        IMagazine mag = e.snappedObject.GetComponent<IMagazine>();
        mag.MagOut(thisWeap);
        base.OnObjectUnsnappedFromDropZone(e);
    }

}