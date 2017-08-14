using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

namespace VRWeapons.InteractionSystems.VRTK
{

    [RequireComponent(typeof(VRTK_SnapDropZone))]

    public class BulletDropZone : MonoBehaviour
    {
        IBoltActions bolt;
        IMagazine thisMag;

        Weapon thisWeapIfInternal;      // Only used if magazine is an internal magazine

        VRTK_SnapDropZone dropZone;

        MonoBehaviour thisMagGO;

        Collider thisCol;

        [Tooltip("Used for internal magazines - If checked, will disable this bullet drop zone unless bolt is back."), SerializeField]
        bool chamberMustBeOpenToReload;

        [SerializeField]
        bool chamberRoundOnInsert;

        [Tooltip("Check this box if rounds should disable on insert. Do not check for 1-shot weapons like RPGs where the rounds are visible."), SerializeField]
        bool disableOnInsert;

        private void Start()
        {
            if (GetComponentInParent<Weapon>() != null)
            {
                thisWeapIfInternal = GetComponentInParent<Weapon>();
            }
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
            bool tmp = thisMag.TryPushBullet(e.snappedObject);
            dropZone.ForceUnsnap();
            if (tmp)
            {
                e.snappedObject.transform.parent = thisMagGO.transform;
                if (disableOnInsert)
                {
                    e.snappedObject.SetActive(false);
                }
                else
                {
                    Rigidbody rb = e.snappedObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }
                }
                if (thisWeapIfInternal != null)
                {
                    thisWeapIfInternal.PlaySound(Weapon.AudioClips.MagIn);
                }
                if (chamberRoundOnInsert)
                {
                    if(thisWeapIfInternal != null)
                    {
                        thisWeapIfInternal.ChamberNewRound(thisMag.FeedRound());
                    }
                    else
                    {
                        Debug.Log("Round not chambered - no weapon found!");
                    }
                }
            }
            else
            {
                Debug.Log("Magazine " + this + " full!");
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
}