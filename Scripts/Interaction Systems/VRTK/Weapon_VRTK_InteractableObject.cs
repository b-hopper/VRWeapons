using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace VRWeapons.InteractionSystems.VRTK
{

    public class Weapon_VRTK_InteractableObject : VRTK_InteractableObject
    {
        Weapon thisWeap;

        VRTK_ControllerReference currentController;

        GameObject currentControllerGO;

        VRWControl control;

        [Tooltip("Strength of haptics on weapon fire, 0 to 1."), SerializeField, Range(0f, 1f)]
        float hapticStrength = 1;

        [Tooltip("Duration of haptic effects per shot, in seconds."), SerializeField]
        float hapticDuration = 0.2f;

        [Tooltip("Time between haptic pulses, in seconds."), SerializeField]
        float hapticPulseInterval = 0.01f;


        private void Start()
        {
            control = FindObjectOfType<VRWControl>();
            thisWeap = GetComponent<Weapon>();
            if (thisWeap.weaponBodyCollider == null)
            {
                Debug.LogWarning("No main collider found, please assign Weapon Body Collider in inspector.");
            }

            thisWeap.shotHaptics += ThisWeap_shotHaptics;

            if (thisWeap.secondHandGripCollider != null)
            {
                Physics.IgnoreCollision(thisWeap.weaponBodyCollider, thisWeap.secondHandGripCollider);
                thisWeap.SetColliderEnabled(thisWeap.secondHandGripCollider, false);
            }

            CheckForControllerAliases();
        }

        private void ThisWeap_shotHaptics()
        {
            if (currentController != null)
            {
                VRTK_ControllerHaptics.TriggerHapticPulse(currentController, hapticStrength, hapticDuration, hapticPulseInterval);
            }
            else
            {
                Debug.LogError("Failed to trigger haptics, current controller not found");
            }
        }

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {
            VRW_ControllerActions_VRTK f;
            f = e.interactingObject.GetComponent<VRW_ControllerActions_VRTK>();
            if (e.interactingObject != GetSecondaryGrabbingObject())
            {
                currentController = VRTK_ControllerReference.GetControllerReference(e.interactingObject);
                currentControllerGO = e.interactingObject;
            }


            if (control.disableControllersOnPickup)
            {
                VRTK_ControllerReference.GetControllerReference(e.interactingObject).model.SetActive(false);
            }

            if (f != null && e.interactingObject != GetSecondaryGrabbingObject())
            {
                f.CurrentHeldWeapon = thisWeap;             // Setting up for touchpad input
            }

            if (thisWeap.holdingDevice == null)
            {
                thisWeap.holdingDevice = e.interactingObject;
                thisWeap.isHeld = true;
            }

            if (e.interactingObject == GetSecondaryGrabbingObject())
            {
                thisWeap.secondHandDevice = e.interactingObject;
                thisWeap.secondHandGripped = true;
            }

            base.OnInteractableObjectGrabbed(e);
            thisWeap.SetColliderEnabled(thisWeap.weaponBodyCollider, false);
            thisWeap.SetColliderEnabled(thisWeap.secondHandGripCollider, true);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            VRW_ControllerActions_VRTK f;
            f = e.interactingObject.GetComponent<VRW_ControllerActions_VRTK>();

            if (e.interactingObject == currentControllerGO)
            {
                thisWeap.SetColliderEnabled(thisWeap.weaponBodyCollider, true);
                thisWeap.SetColliderEnabled(thisWeap.secondHandGripCollider, false);
            }

            if (e.interactingObject == GetGrabbingObject())
            {
                currentController = null;
                currentControllerGO = null;
            }
            if (f != null)
            {
                f.CurrentHeldWeapon = null;
            }

            if (control.disableControllersOnPickup)
            {
                VRTK_ControllerReference.GetControllerReference(e.interactingObject).model.SetActive(true);
            }

            if (e.interactingObject == thisWeap.holdingDevice)
            {
                thisWeap.holdingDevice = null;
                thisWeap.isHeld = false;
            }
            else
            {
                thisWeap.secondHandDevice = null;
            }

            base.OnInteractableObjectUngrabbed(e);
        }

        public override void StartUsing(VRTK_InteractUse usingObject)
        {
            if (usingObject.gameObject == currentControllerGO)
            {
                base.StartUsing(usingObject);
                thisWeap.StartFiring(usingObject.gameObject);
            }
        }

        public override void StopUsing(VRTK_InteractUse previousUsingObject)
        {
            if (previousUsingObject.gameObject == currentControllerGO)
            {
                base.StopUsing(previousUsingObject);
                thisWeap.StopFiring(previousUsingObject.gameObject);
            }
        }

        void CheckForControllerAliases()
        {
            VRTK_SDKManager tmp = FindObjectOfType<VRTK_SDKManager>();
            if (tmp != null)
            {
                if (tmp.scriptAliasLeftController != null)
                {
                    if (tmp.scriptAliasLeftController.GetComponent<VRW_ControllerActions_VRTK>() == null)
                    {
                        tmp.scriptAliasLeftController.AddComponent<VRW_ControllerActions_VRTK>();
                        Debug.LogWarning("No VRW_ControllerActions_VRTK found on " + tmp.scriptAliasLeftController + ". Adding component. Please add component in editor.");
                    }
                }
                else
                {
                    Debug.LogError("No left controller alias found. Please assign one in the VRTK SDK Manager.");
                }
                if (tmp.scriptAliasRightController != null)
                {
                    if (tmp.scriptAliasRightController.GetComponent<VRW_ControllerActions_VRTK>() == null)
                    {
                        tmp.scriptAliasRightController.AddComponent<VRW_ControllerActions_VRTK>();
                        Debug.LogWarning("No VRW_ControllerActions_VRTK found on " + tmp.scriptAliasRightController + ". Adding component. Please add component in editor.");
                    }
                }
                else
                {
                    Debug.LogError("No right controller alias found. Please assign one in the VRTK SDK Manager.");
                }
            }
        }

        public void SetWeaponBodyCollider(Collider collider)
        {
            if (collider != null)
            {
                thisWeap.weaponBodyCollider = collider;
            }
        }
    }
}