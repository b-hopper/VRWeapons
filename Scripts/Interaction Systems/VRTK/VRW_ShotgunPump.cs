using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace VRWeapons.InteractionSystems.VRTK
{
    public class VRW_ShotgunPump : MonoBehaviour
    {
        [HideInInspector]
        public Vector3 boltOpenPosition, boltClosedPosition;

        Vector3 startRot;

        float lerpValue, oldLerpValue;
        bool thisObjectIsGrabbed, hasMoved;

        Weapon_VRTK_InteractableObject weapIntObj;

        IBoltActions bolt;

        private void Start()
        {
            weapIntObj = GetComponentInParent<Weapon_VRTK_InteractableObject>();
            bolt = transform.parent.GetComponentInChildren<IBoltActions>();
            startRot = transform.localEulerAngles;
        }

        private void Update()
        {
            oldLerpValue = lerpValue;

            if (weapIntObj.GetSecondaryGrabbingObject() != null)
            {
                transform.position = weapIntObj.GetSecondaryGrabbingObject().transform.position;
                thisObjectIsGrabbed = true;
            }
            else
            {
                thisObjectIsGrabbed = false;
            }

            if (thisObjectIsGrabbed)
            {
                bolt.IsCurrentlyBeingManipulated(true);
                ClampControllerToTrack();
                lerpValue = VRWControl.V3InverseLerp(boltClosedPosition, boltOpenPosition, transform.localPosition);
                bolt.boltLerpVal = lerpValue;
            }

            else if (hasMoved)
            {
                bolt.IsCurrentlyBeingManipulated(false);
                hasMoved = false;
                ClampControllerToTrack();
            }

            else
            {
                lerpValue = bolt.boltLerpVal;
            }

            if (oldLerpValue != lerpValue)
            {
                transform.localPosition = Vector3.Lerp(boltClosedPosition, boltOpenPosition, lerpValue);
            }
            transform.localEulerAngles = startRot;
        }

        void ClampControllerToTrack()
        {
            transform.localPosition = VRWControl.V3Clamp(transform.localPosition, boltOpenPosition, boltClosedPosition);
        }
    }
}