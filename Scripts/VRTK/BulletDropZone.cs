using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class BulletDropZone : VRTK_SnapDropZone {
    
    MonoBehaviour thisMagGO;
    IMagazine thisMag;
    IBoltActions bolt;
        
    Collider thisCol;

    [Tooltip("Used for internal magazines - If checked, will disable this bullet drop zone unless bolt is back."), SerializeField]
    bool chamberMustBeOpenToReload;

    private void Start()
    {
        thisMagGO = (MonoBehaviour)GetComponentInParent<IMagazine>();
        thisMag = GetComponentInParent<IMagazine>();
        bolt = transform.parent.GetComponentInChildren<IBoltActions>();
        thisCol = GetComponent<Collider>();
        snapType = SnapTypes.UseParenting;                                  // Required to function. Otherwise round just falls off
    }

    public override void OnObjectSnappedToDropZone(SnapDropZoneEventArgs e)
    {
        base.OnObjectSnappedToDropZone(e);

        bool tmp = thisMag.PushBullet(e.snappedObject);
        ForceUnsnap();
        Debug.Log(tmp);
        if (tmp)
        {
            e.snappedObject.transform.parent = thisMagGO.transform;
            e.snappedObject.SetActive(false);
        }
    }

    public override void OnObjectUnsnappedFromDropZone(SnapDropZoneEventArgs e)
    {
        base.OnObjectUnsnappedFromDropZone(e);

        //thisMag.PopBullet();
    }

    protected override void Update()
    {
        base.Update();
        if (chamberMustBeOpenToReload)
        {
            if (bolt != null)
            {
                if (bolt.boltLerpVal <= 0.9f)
                {
                    thisCol.enabled = false;
                }
                else
                {
                    thisCol.enabled = true;
                }
            }
        }
    }

}
