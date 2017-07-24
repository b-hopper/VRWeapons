using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons.InteractionSystems.Generic
{
    [RequireComponent(typeof(Rigidbody))]

    public class VRWGenericInteractableWeapon : MonoBehaviour
    {
        Weapon thisWeap;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;

        [SerializeField] Valve.VR.EVRButtonId grabButton = Valve.VR.EVRButtonId.k_EButton_Grip;
        [SerializeField] Valve.VR.EVRButtonId fireButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
        [SerializeField] Valve.VR.EVRButtonId dropMagButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;

        Rigidbody thisRB;

        bool isHeld, wasPreviouslyKinematic, isColliding;

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
            if (device == null || trackedObj == null)
            {
                Debug.Log("Check");
                if ((trackedObj = other.GetComponentInParent<SteamVR_TrackedObject>()) != null)
                {
                    device = SteamVR_Controller.Input((int)trackedObj.index);
                }
                isColliding = true;
            }
            if (device != null && !isHeld && device.GetPressDown(grabButton) && Time.time - dropTime > 0.2f)
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
            }
            else if (!isColliding && !isHeld)
            {
                device = null;
                trackedObj = null;
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
    }
}
