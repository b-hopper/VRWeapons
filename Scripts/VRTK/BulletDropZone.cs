using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class BulletDropZone : VRTK_SnapDropZone {
    IBoltActions bolt;
    IMagazine thisMag;

    MonoBehaviour thisMagGO;

    Collider thisCol;

    [Tooltip("Used for internal magazines - If checked, will disable this bullet drop zone unless bolt is back."), SerializeField]
    bool chamberMustBeOpenToReload;

    private void Start()
    {
        bolt = transform.parent.GetComponentInChildren<IBoltActions>();
        thisMag = GetComponentInParent<IMagazine>();
        thisMagGO = (MonoBehaviour)GetComponentInParent<IMagazine>();
        thisCol = GetComponent<Collider>();
        thisCol.enabled = true;
    }

    public override void OnObjectSnappedToDropZone(SnapDropZoneEventArgs e)
    {
        bool tmp = thisMag.PushBullet(e.snappedObject);
        ForceUnsnap();
        if (tmp)
        {
            e.snappedObject.transform.parent = thisMagGO.transform;
            e.snappedObject.SetActive(false);
        }
        base.OnObjectSnappedToDropZone(e);
    }

    protected override void Update()
    {
        if (chamberMustBeOpenToReload && Time.time > 1)             // This was causing some problems, something in VRTK's snap drop zone stopped it from working if the 
        {                                                           // collider wasn't enabled on start. So, this makes sure it's not disabled on start.
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
