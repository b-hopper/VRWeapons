using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class BulletDropZone : VRTK_SnapDropZone {
    MonoBehaviour thisMagGO;
    IMagazine thisMag;

    private void Start()
    {
        thisMagGO = (MonoBehaviour)GetComponentInParent<IMagazine>();
        thisMag = GetComponentInParent<IMagazine>();
    }

    public override void OnObjectSnappedToDropZone(SnapDropZoneEventArgs e)
    {
        base.OnObjectSnappedToDropZone(e);

        IBulletBehavior newRound = e.snappedObject.GetComponent<IBulletBehavior>();
        bool tmp = thisMag.PushBullet(newRound);
        ForceUnsnap();
        if (tmp)
        {
            e.snappedObject.transform.parent = thisMagGO.transform;
        }
        //e.snappedObject.SetActive(false);
    }

    public override void OnObjectUnsnappedFromDropZone(SnapDropZoneEventArgs e)
    {
        base.OnObjectUnsnappedFromDropZone(e);

        //thisMag.PopBullet();
    }

}
