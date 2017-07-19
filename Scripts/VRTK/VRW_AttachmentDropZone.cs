using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

[RequireComponent(typeof(VRTK_SnapDropZone))]

public class VRW_AttachmentDropZone : MonoBehaviour {

    VRTK_SnapDropZone dropZone;
    Weapon thisWeap;

    private void Start()
    {
        dropZone = GetComponent<VRTK_SnapDropZone>();
        dropZone.ObjectSnappedToDropZone += new SnapDropZoneEventHandler(ObjectSnapped);
        dropZone.ObjectUnsnappedFromDropZone += new SnapDropZoneEventHandler(ObjectUnsnapped);
        thisWeap = GetComponentInParent<Weapon>();
    }

    void ObjectSnapped(object sender, SnapDropZoneEventArgs e)
    {
        thisWeap.IgnoreCollision(e.snappedObject.GetComponentInChildren<Collider>(), true);
    }

    void ObjectUnsnapped(object sender, SnapDropZoneEventArgs e)
    {
        thisWeap.IgnoreCollision(e.snappedObject.GetComponentInChildren<Collider>(), false);
    }
}
