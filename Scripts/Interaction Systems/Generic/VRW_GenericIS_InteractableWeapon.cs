using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons.InteractionSystems.Generic
{
    [RequireComponent(typeof(Rigidbody))]

    public class VRW_GenericIS_InteractableWeapon : MonoBehaviour
    {
        Weapon thisWeap;

        SteamVR_Controller.Device device, secondHandDevice;
        [HideInInspector]
        public SteamVR_TrackedObject trackedObj, secondHandTrackedObj;

        [SerializeField] public Valve.VR.EVRButtonId grabButton = Valve.VR.EVRButtonId.k_EButton_Grip;
        [SerializeField] Valve.VR.EVRButtonId fireButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
        [SerializeField] Valve.VR.EVRButtonId dropMagButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;

        [Tooltip("If checked, second hand grip button must be held down to remain gripping."), SerializeField]
        public bool holdButtonTo2HandGrip;

        Rigidbody thisRB;

        bool isHeld, secondHandGripped, wasPreviouslyKinematic, isColliding;

        Transform previousParent;

        float dropTime;

        private void Start()
        {
            thisWeap = GetComponent<Weapon>();
            thisWeap.shotHaptics += new VRWControl.TriggerHaptics(ShotHaptics);
            thisRB = GetComponent<Rigidbody>();
            DisableWeaponColliders(isHeld);
        }

        void ShotHaptics()
        {
            Debug.Log("Haptics haptics");
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
                    isColliding = true;
                }
                if (device != null && device.GetPressDown(grabButton) && Time.time - dropTime > 0.2f)
                {
                    wasPreviouslyKinematic = thisRB.isKinematic;
                    thisRB.isKinematic = true;
                    previousParent = transform.parent;
                    transform.parent = trackedObj.transform;
                    transform.localPosition = thisWeap.grabPoint.localPosition;
                    transform.localEulerAngles = thisWeap.grabPoint.localEulerAngles;
                    isHeld = true;
                    dropTime = Time.time;

                    DisableWeaponColliders(isHeld);
                }
            }
            else
            {
                if (secondHandDevice == null || secondHandTrackedObj == null)
                {
                    if ((secondHandTrackedObj = other.GetComponentInParent<SteamVR_TrackedObject>()) != null)
                    {
                        secondHandDevice = SteamVR_Controller.Input((int)secondHandTrackedObj.index);
                    }
                    if (device == secondHandDevice)
                    {
                        secondHandDevice = null;
                        secondHandTrackedObj = null;
                    }
                    isColliding = true;
                }
                if (secondHandDevice != null && secondHandDevice.GetPressDown(grabButton) && Time.time - dropTime > 0.2f && !secondHandGripped && secondHandDevice != device)
                {
                    secondHandGripped = true;
                    dropTime = Time.time;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            isColliding = false;
        }

        private void Update()
        {
            if (isHeld)
            {
                if (device.GetPressDown(fireButton))
                {
                    thisWeap.StartFiring(trackedObj.gameObject);
                }
                else if (device.GetPressUp(fireButton))
                {
                    thisWeap.StopFiring(trackedObj.gameObject);
                }
                if (device.GetTouch(fireButton))
                {
                    thisWeap.SetTriggerAngle(device.GetAxis(fireButton).x);
                }
                if (device.GetPressDown(dropMagButton))
                {
                    thisWeap.DropMagazine();
                }
                if (device.GetPressDown(grabButton) && Time.time - dropTime > 0.2f)
                {
                    thisRB.isKinematic = wasPreviouslyKinematic;
                    transform.parent = previousParent;

                    tossObject(thisRB);

                    isHeld = false;

                    DisableWeaponColliders(isHeld);

                    dropTime = Time.time;
                    device = null;
                    trackedObj = null;
                }
                if (secondHandGripped)
                {
                    ZLockedAim();
                    if ((secondHandDevice.GetPressDown(grabButton) || (secondHandDevice.GetPressUp(grabButton) && holdButtonTo2HandGrip)) && Time.time - dropTime > 0.2f)
                    {
                        secondHandGripped = false;
                        Realign();
                    }
                }
            }
            else if (!isColliding && !isHeld)
            {
                device = null;
                trackedObj = null;
                secondHandDevice = null;
                secondHandTrackedObj = null;
            }
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

        void DisableWeaponColliders(bool isGripped)
        {
            thisWeap.weaponBodyCollider.enabled = !isGripped;
            if (thisWeap.secondHandGripCollider != null)
            {
                thisWeap.secondHandGripCollider.enabled = isGripped;
            }
        }

        protected virtual void ZLockedAim()                     // BORROWED FROM VRTK'S VRTK_ControlDirectionGrabAction SCRIPT, THANKS STONE FOX//
        {
            Vector3 forward = (secondHandTrackedObj.transform.position - trackedObj.transform.position).normalized;

            // calculate rightLocked rotation
            Quaternion rightLocked = Quaternion.LookRotation(forward, Vector3.Cross(-trackedObj.transform.right, forward).normalized);

            // delta from current rotation to the rightLocked rotation
            Quaternion rightLockedDelta = Quaternion.Inverse(transform.rotation) * rightLocked;

            float rightLockedAngle;
            Vector3 rightLockedAxis;

            // forward direction and roll
            rightLockedDelta.ToAngleAxis(out rightLockedAngle, out rightLockedAxis);

            if (rightLockedAngle > 180f)
            {
                // remap ranges from 0-360 to -180 to 180
                rightLockedAngle -= 360f;
            }

            // make any negative values into positive values;
            rightLockedAngle = Mathf.Abs(rightLockedAngle);

            // calculate upLocked rotation
            Quaternion upLocked = Quaternion.LookRotation(forward, trackedObj.transform.forward);

            // delta from current rotation to the upLocked rotation
            Quaternion upLockedDelta = Quaternion.Inverse(transform.rotation) * upLocked;

            float upLockedAngle;
            Vector3 upLockedAxis;

            // forward direction and roll
            upLockedDelta.ToAngleAxis(out upLockedAngle, out upLockedAxis);

            // remap ranges from 0-360 to -180 to 180
            if (upLockedAngle > 180f)
            {
                upLockedAngle -= 360f;
            }

            // make any negative values into positive values;
            upLockedAngle = Mathf.Abs(upLockedAngle);

            // assign the one that involves less change to roll
            transform.rotation = (upLockedAngle < rightLockedAngle ? upLocked : rightLocked);
        }

        void Realign()
        {
            transform.localEulerAngles = thisWeap.grabPoint.localEulerAngles;
            transform.localPosition = thisWeap.grabPoint.localPosition;
        }
    }
}
