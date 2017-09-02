using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

namespace VRWeapons.InteractionSystems.VRTK
{

    [RequireComponent(typeof(VRTK_SnapDropZone))]
        
    public class MagDropZone : MonoBehaviour
    {
        Weapon thisWeap;
        Weapon_VRTK_InteractableObject thisWeapInteractable;
        VRTK_SnapDropZone dropZone;

        [Tooltip("If checked, this will disable the magazine's collider when it is inserted. Solves issues with grabbing magazines instead of the gun, in cases like pistols."), SerializeField]
        bool disableColliderOnMagIn;

        private void Start()
        {
            dropZone = GetComponent<VRTK_SnapDropZone>();
            dropZone.ObjectSnappedToDropZone += new SnapDropZoneEventHandler(ObjectSnapped);
            dropZone.ObjectUnsnappedFromDropZone += new SnapDropZoneEventHandler(ObjectUnsnapped);

            thisWeap = GetComponentInParent<Weapon>();
            thisWeapInteractable = GetComponentInParent<Weapon_VRTK_InteractableObject>();
        }

        void ObjectSnapped(object sender, SnapDropZoneEventArgs e)
        {
            IMagazine mag = e.snappedObject.GetComponent<IMagazine>();
            thisWeap.HandleMagIn(mag);
            SetCollidersEnabled(mag, false);


            mag.MagDropped += Mag_MagDropped;
        }

        void ObjectUnsnapped(object sender, SnapDropZoneEventArgs e)
        {
            IMagazine mag = e.snappedObject.GetComponent<IMagazine>();
            //Stop listening for mag drop event so we won't redundantly unsnap
            mag.MagDropped -= Mag_MagDropped;
            mag.MagOut(thisWeap);

            SetCollidersEnabled(mag, true);

            //This is necessary for the initial mag so it won't revert to child of weapon
            var interactable = e.snappedObject.GetComponent<VRTK_InteractableObject>();
            if (interactable != null)
            {
                interactable.SaveCurrentState();
            }

        }

        private void SetCollidersEnabled(IMagazine mag, bool enabled)
        {
            if (disableColliderOnMagIn)
            {
                mag.SetCollidersEnabled(enabled);
            }
            else //Ignore collision with weapon instead
            {
                foreach (var collider in mag.Colliders)
                {
                    thisWeap.IgnoreCollision(collider, !enabled);
                }
            }
        }

        private void Mag_MagDropped(object sender, System.EventArgs e)
        {
            if (sender is IMagazine)
            {
                (sender as IMagazine).MagDropped -= Mag_MagDropped;
            }
            dropZone.ForceUnsnap();
        }

    }
}