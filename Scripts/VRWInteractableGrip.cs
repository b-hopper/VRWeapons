using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]

public class VRWInteractableGrip : MonoBehaviour
{
    ConfigurableJoint handJoint, weapJoint;
    VRTK.Weapon thisWeap;
    Valve.VR.EVRButtonId grabButton;
    bool mustHoldDownGrip, gripped, isActive;
    Hand grippingHand;
    GameObject disappearingHand, disappearingHandHighlight;
    float dropTime;
    Collider thisCollider;
    SteamVR_Controller.Device device;
    SteamVR_TrackedObject trackedObj;

    VRWControl control;

    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);

    void Awake()
    {
        isActive = true;
        thisWeap = GetComponentInParent<VRTK.Weapon>();
        weapJoint = thisWeap.GetComponent<ConfigurableJoint>();
        control = FindObjectOfType<VRWControl>();
        thisCollider = GetComponent<Collider>();

        // Assign rebindable buttons
        grabButton = control.grabButton;

        mustHoldDownGrip = control.mustHoldDownGrip;
        
        if (control.VRTKMode)
        {
            thisCollider.isTrigger = false;
            if (GetComponent<Rigidbody>() == null)
            {
                gameObject.AddComponent<Rigidbody>();
            }
            GetComponent<Rigidbody>().isKinematic = true;
        }


    }

    void FixedUpdate()
    {
        if (!control.VRTKMode)
        {
            if (isActive && !thisWeap.held)
            {
                thisCollider.enabled = false;
                isActive = false;
            }
            else if (thisWeap.held)
            {
                thisCollider.enabled = true;
                isActive = true;
            }

            if (gripped && (grippingHand.controller.GetPressDown(grabButton) || (grippingHand.controller.GetPressUp(grabButton) && mustHoldDownGrip)))
            {
                gripped = false;
                thisWeap.gripped = false;
                SetAllJointAxesFree(handJoint);
                handJoint.connectedBody = null;
                if (thisWeap.held)
                {
                    SetAllJointAxesLocked(weapJoint);
                }
                thisWeap.canGrip = false;
                thisWeap.offhandJoint = null;
                if (!control.showDeviceOffHand)
                {
                    disappearingHand.SetActive(true);
                    disappearingHandHighlight.SetActive(true);
                }
                dropTime = Time.time;

            }
        }
        else
        {
            if (isActive && !thisWeap.held)
            {
                thisCollider.enabled = false;
                isActive = false;
            }
            else if (thisWeap.held)
            {
                thisCollider.enabled = true;
                isActive = true;
            }

            if (gripped && (device.GetPressDown(grabButton) || (device.GetPressUp(grabButton) && mustHoldDownGrip)))
            {
                gripped = false;
                thisWeap.gripped = false;
                control.SetAllJointAxesFree(handJoint);
                handJoint.connectedBody = null;
                if (thisWeap.held)
                {
                    control.SetAllJointAxesLocked(weapJoint);
                }
                thisWeap.canGrip = false;
                thisWeap.offhandJoint = null;

                if (!control.showDeviceOffHand)
                {
                    disappearingHand.SetActive(true);
                }
                disappearingHand = null;
                dropTime = Time.time;

            }

        }
        if (gripped)
        {
            handJoint.connectedAnchor = this.transform.localPosition;
        }
    }

    ////// VRTK-Friendly grab below //////

    void OnTriggerStay(Collider col)
    {
        if (control.VRTKMode)
        {
            if (trackedObj == null || device == null)
            {
                if ((trackedObj = col.GetComponentInParent<SteamVR_TrackedObject>()) != null)
                {
                    device = SteamVR_Controller.Input((int)trackedObj.index);
                }
            }

            if (!control.showDeviceOffHand && disappearingHand == null && trackedObj != null)
            {
                disappearingHand = trackedObj.transform.FindChild("Model").gameObject;
            }
            if (thisWeap.held) { thisWeap.canGrip = true; }

            handJoint = trackedObj.GetComponent<ConfigurableJoint>();
            if (((device != null) && device.GetPressDown(grabButton)) && thisWeap.held && !gripped && (Time.time - dropTime > 0.2f))
            {
                handJoint.projectionMode = JointProjectionMode.PositionAndRotation;
                handJoint.projectionDistance = 0.25f;
                handJoint.autoConfigureConnectedAnchor = false;
                handJoint.connectedAnchor = transform.localPosition;
                handJoint.connectedBody = thisWeap.GetComponent<Rigidbody>();
                handJoint.connectedAnchor = transform.localPosition;
                thisWeap.offhandJoint = handJoint;

                control.DeviceJointSetupOnGrip(handJoint);
                control.WeaponJointSetupOnGrip(weapJoint);
                if (!control.showDeviceOffHand)
                {
                    disappearingHand.SetActive(false);
                }
                
                gripped = true;
                thisWeap.gripped = true;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (control.VRTKMode && !gripped)
        {
            trackedObj = null;
            device = null;
        }
        thisWeap.canGrip = false;
    }


    ////// SteamVR Interaction System below ////// 

    private void HandHoverUpdate(Hand hand)
    {
        if (!control.showDeviceOffHand && disappearingHand == null)
        {
            disappearingHand = hand.GetComponentInChildren<SpawnRenderModel>().gameObject;
            disappearingHandHighlight = hand.GetComponentInChildren<ControllerHoverHighlight>().gameObject;
        }
        if (thisWeap.held) { thisWeap.canGrip = true; }
        handJoint = hand.GetComponent<ConfigurableJoint>();
        if (((hand.controller != null) && hand.controller.GetPressDown(grabButton)) && hand.otherHand.currentAttachedObject == thisWeap.gameObject && !gripped && (Time.time - dropTime > 0.2f))
        {
            handJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            handJoint.projectionDistance = 0.25f;
            handJoint.autoConfigureConnectedAnchor = false;
            handJoint.connectedAnchor = transform.localPosition;
            handJoint.connectedBody = thisWeap.GetComponent<Rigidbody>();
            handJoint.connectedAnchor = transform.localPosition;
            thisWeap.offhandJoint = handJoint;

            control.DeviceJointSetupOnGrip(handJoint);
            control.WeaponJointSetupOnGrip(weapJoint);

            if (!control.showDeviceOffHand)
            {
                disappearingHand.SetActive(false);
                disappearingHandHighlight.SetActive(false);
            }

            //hand.HoverLock(GetComponent<Interactable>());
            gripped = true;
            thisWeap.gripped = true;
            grippingHand = hand;


        }
        
    }

    private void OnHandHoverEnd(Hand hand)
    {
        if (!gripped)
        {
            thisWeap.canGrip = false;
        }
    }
    
    #region Useful functions

    public static float V3InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    public Vector3 V3Clamp(Vector3 value, Vector3 Min, Vector3 Max)
    {
        if (Min.x < Max.x)
        {
            value.x = Mathf.Clamp(value.x, Min.x, Max.x);
        }
        else
        {
            value.x = Mathf.Clamp(value.x, Max.x, Min.x);
        }
        if (Min.y < Max.y)
        {
            value.y = Mathf.Clamp(value.y, Min.y, Max.y);
        }
        else
        {
            value.y = Mathf.Clamp(value.y, Max.y, Min.y);
        }
        if (Min.z < Max.z)
        {
            value.z = Mathf.Clamp(value.z, Min.z, Max.z);
        }
        else
        {
            value.z = Mathf.Clamp(value.z, Max.z, Min.z);
        }
        return value;
    }

    void DeviceJointSetupOnGrip(ConfigurableJoint jt)
    {
        jt.xMotion = ConfigurableJointMotion.Locked;
        jt.yMotion = ConfigurableJointMotion.Locked;
        jt.zMotion = ConfigurableJointMotion.Locked;

        jt.angularXMotion = ConfigurableJointMotion.Free;
        jt.angularYMotion = ConfigurableJointMotion.Free;
        jt.angularZMotion = ConfigurableJointMotion.Free;
    }

    void SetAllJointAxesFree(ConfigurableJoint jt)
    {
        jt.xMotion = ConfigurableJointMotion.Free;
        jt.yMotion = ConfigurableJointMotion.Free;
        jt.zMotion = ConfigurableJointMotion.Free;

        jt.angularXMotion = ConfigurableJointMotion.Free;
        jt.angularYMotion = ConfigurableJointMotion.Free;
        jt.angularZMotion = ConfigurableJointMotion.Free;
    }

    public void SetAllJointAxesLocked(ConfigurableJoint jt)
    {
        jt.xMotion = ConfigurableJointMotion.Locked;
        jt.yMotion = ConfigurableJointMotion.Locked;
        jt.zMotion = ConfigurableJointMotion.Locked;

        jt.angularXMotion = ConfigurableJointMotion.Locked;
        jt.angularYMotion = ConfigurableJointMotion.Locked;
        jt.angularZMotion = ConfigurableJointMotion.Locked;
    }

    void SetLayerInObjectAndChildren(GameObject item, int layer)
    {
        int children = item.transform.childCount;
        if (children != 0)
        {
            for (int i = 0; i < children; i++)
            {
                SetLayerInObjectAndChildren(item.transform.GetChild(i).gameObject, layer);
            }
        }

        if (item.tag != "ReloadPoint")
        {
            item.layer = layer;
        }
    }

    #endregion


}
