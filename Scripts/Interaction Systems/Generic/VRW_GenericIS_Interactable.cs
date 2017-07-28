using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons.InteractionSystems.Generic
{
    [RequireComponent(typeof(Rigidbody))]
    public class VRW_GenericIS_Interactable : MonoBehaviour
    {
        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;

        float dropTime;
        bool isHeld, wasPreviouslyKinematic;

        Rigidbody thisRB;

        Transform previousParent, currentParent;

        [SerializeField] Valve.VR.EVRButtonId grabButton = Valve.VR.EVRButtonId.k_EButton_Grip;

        [SerializeField] Vector3 grabbedPosition, grabbedRotation;

        [Tooltip("If checked, will not adjust position when grabbed."), SerializeField]
        bool precisionGrab;

        [Tooltip("If checked, user must hold down grip button to continue holding magazine. Otherwise, grabbing is toggled with each press."), SerializeField]
        bool holdButtonToGrab;

        private void Start()
        {
            thisRB = GetComponent<Rigidbody>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isHeld)
            {
                if (device == null || trackedObj == null)
                {
                    if ((trackedObj = other.GetComponentInParent<SteamVR_TrackedObject>()) != null)
                    {
                        device = SteamVR_Controller.Input((int)trackedObj.index);
                    }                 
                }
                if (device != null && device.GetPressDown(grabButton) && Time.time - dropTime > 0.2f)
                {
                    previousParent = transform.parent;
                    wasPreviouslyKinematic = thisRB.isKinematic;
                    transform.parent = trackedObj.transform;

                    currentParent = transform.parent;

                    if (!precisionGrab)
                    {
                        transform.localPosition = grabbedPosition;
                        transform.localEulerAngles = grabbedRotation;
                    }

                    thisRB.isKinematic = true;

                    isHeld = true;
                    dropTime = Time.time;
                }

            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isHeld)
            {
                device = null;
                trackedObj = null;
            }
        }

        private void Update()
        {
            if (isHeld)
            {
                if (device != null && ((device.GetPressDown(grabButton) && Time.time - dropTime > 0.2f )|| (device.GetPressUp(grabButton) && holdButtonToGrab)) )
                {
                    Drop();
                }
                if (transform.parent != currentParent)
                {
                    device = null;
                    trackedObj = null;
                    isHeld = false;
                }
            }
            
        }

        void Drop()
        {
            transform.parent = previousParent;
            thisRB.isKinematic = wasPreviouslyKinematic;
            tossObject(thisRB);

            device = null;
            trackedObj = null;
            isHeld = false;
            dropTime = Time.time;
        }

        void tossObject(Rigidbody rigidbody)
        {
            Transform origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;

            if (origin != null)
            {
                rigidbody.velocity = origin.TransformVector(device.velocity);
                rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity);
            }
            else
            {
                rigidbody.velocity = device.velocity;
                rigidbody.angularVelocity = device.angularVelocity;
            }
        }

    }
}