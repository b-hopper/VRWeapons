using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

[RequireComponent(typeof(VRTK_SnapDropZone))]

public class BulletDropZone : MonoBehaviour {
    IBoltActions bolt;
    IMagazine thisMag;

    VRTK_SnapDropZone dropZone;

    MonoBehaviour thisMagGO;

    Collider thisCol;

    [Tooltip("Used for internal magazines - If checked, will disable this bullet drop zone unless bolt is back."), SerializeField]
    bool chamberMustBeOpenToReload;

    private void Start()
    {
        dropZone = GetComponent<VRTK_SnapDropZone>();
        dropZone.ObjectSnappedToDropZone += new SnapDropZoneEventHandler(ObjectSnapped);
        bolt = transform.parent.GetComponentInChildren<IBoltActions>();
        thisMag = GetComponentInParent<IMagazine>();
        thisMagGO = (MonoBehaviour)GetComponentInParent<IMagazine>();
        thisCol = GetComponent<Collider>();
        thisCol.enabled = true;
    }

    void ObjectSnapped(object sender, SnapDropZoneEventArgs e)
    {
        bool tmp = thisMag.PushBullet(e.snappedObject);
        dropZone.ForceUnsnap();
        Debug.Log("Check");
        if (tmp)
        {
            e.snappedObject.transform.parent = thisMagGO.transform;
            e.snappedObject.SetActive(false);
        }
    }

    private void Update()
    {
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
