using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons.InteractionSystems.Generic
{
    [RequireComponent(typeof(Rigidbody))]
    public class VRW_GenericIS_BoltInteractable : MonoBehaviour
    {
        IBoltActions bolt;

        Collider col;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;

        VRW_GenericIS_InteractableWeapon weaponInteractable;

        Weapon thisWeap;

        bool thisObjectIsGrabbed, hasMoved;

        float lerpValue, dropTime;

        [SerializeField]
        public Vector3 boltOpenPosition, boltClosedPosition;

        [SerializeField]
        bool mustHoldGrabButton;

        private void Start()
        {
            thisWeap = GetComponentInParent<Weapon>();
            col = GetComponent<Collider>();
            Physics.IgnoreCollision(col, thisWeap.weaponBodyCollider);
            if (thisWeap.secondHandGripCollider != null)
            {
                Physics.IgnoreCollision(col, thisWeap.secondHandGripCollider);
            }
            bolt = transform.parent.GetComponentInChildren<IBoltActions>();
            GetComponent<Rigidbody>().isKinematic = true;
            weaponInteractable = GetComponentInParent<VRW_GenericIS_InteractableWeapon>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (!thisObjectIsGrabbed)
            {
                if (device == null || trackedObj == null)
                {
                    if ((trackedObj = other.GetComponentInParent<SteamVR_TrackedObject>()) != null)
                    {
                        device = SteamVR_Controller.Input((int)trackedObj.index);
                    }
                    if (trackedObj == weaponInteractable.trackedObj)
                    {
                        device = null;
                        trackedObj = null;
                    }
                }
                if (device != null && device.GetPressDown(weaponInteractable.grabButton) && Time.time - dropTime > 0.2f)
                {
                    thisObjectIsGrabbed = true;
                    dropTime = Time.time;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!thisObjectIsGrabbed)
            {
                device = null;
                trackedObj = null;
            }
        }

        private void Update()
        {
            float oldLerpValue = lerpValue;

            if (thisObjectIsGrabbed)
            {
                {
                    bolt.IsCurrentlyBeingManipulated(true);
                    hasMoved = true;

                    transform.position = trackedObj.transform.position;

                    ClampControllerToTrack();                                                                                       // Clamps to make sure it stays on the tracks it has been assigned in inspector

                    lerpValue = VRWControl.V3InverseLerp(boltClosedPosition, boltOpenPosition, transform.localPosition);            // Final lerp value of bolt, which is then passed...

                    bolt.boltLerpVal = lerpValue;                                                                                   // ... to the bolt itself.
                }
                if (device == null ||
                    (device.GetPressUp(weaponInteractable.grabButton) && mustHoldGrabButton) ||
                    (device.GetPressDown(weaponInteractable.grabButton) && Time.time - dropTime > 0.2f))
                {
                    thisObjectIsGrabbed = false;
                }
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
            if (lerpValue != oldLerpValue)                                                                                      // Moves the manipulator to be in the correct position (with the bolt object). Only 
            {                                                                                                                   // moves it if the lerp value has changed - doesn't need adjustment otherwise.
                transform.localPosition = Vector3.Lerp(boltClosedPosition, boltOpenPosition, lerpValue);
            }
        }

        void ClampControllerToTrack()
        {
            transform.localPosition = VRWControl.V3Clamp(transform.localPosition, boltOpenPosition, boltClosedPosition);
        }

    }
}