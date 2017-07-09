using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

[RequireComponent(typeof(VRTK_SnapDropZone))]

public class MuzzleDropZone : MonoBehaviour {

    VRTK_SnapDropZone dropZone;
    Weapon thisWeap;
    IMuzzleActions startMuzzle;

    private void Start()
    {
        dropZone = GetComponent<VRTK_SnapDropZone>();
        thisWeap = GetComponentInParent<Weapon>();
        startMuzzle = thisWeap.GetComponentInChildren<IMuzzleActions>();

        dropZone.ObjectSnappedToDropZone += new SnapDropZoneEventHandler(ObjectSnapped);
        dropZone.ObjectUnsnappedFromDropZone += new SnapDropZoneEventHandler(ObjectUnsnapped);
    }

    void ObjectSnapped(object sender, SnapDropZoneEventArgs e)
    {
        IMuzzleActions tmp = e.snappedObject.GetComponent<IMuzzleActions>();
        thisWeap.SetMuzzle(tmp);
        tmp.SetNewWeapon(thisWeap);
    }

    void ObjectUnsnapped(object sender, SnapDropZoneEventArgs e)
    {
        thisWeap.SetMuzzle(startMuzzle);        
    }
}
