using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class MagDropZone : VRTK_SnapDropZone {
    VRWeapons.Weapon thisWeap;
    [Tooltip("Select a mag here to load mag on start.")]
    [SerializeField]
    GameObject startingMag;

    private void Start()
    {
        thisWeap = GetComponentInParent<VRWeapons.Weapon>();
        Debug.Log(thisWeap);

        if (startingMag != null) {
            ForceSnap(startingMag);
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
