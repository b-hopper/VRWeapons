using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(VelocityEstimator))]

public class VRWInteractableWeapon : MonoBehaviour
{
    private Vector3 oldPosition;
    private Quaternion oldRotation;
    public ConfigurableJoint handJoint, thisJoint;
    Weapon thisWeap;
    Valve.VR.EVRButtonId grabButton, fireButton, dropMag;
    public GameObject disappearingHand, disappearingHandHighlight;
    Vector3 oldHandPos, handPos;
    VelocityEstimator velocityEstimator;
    SteamVR_Controller.Device device;
    SteamVR_TrackedObject trackedObj;

    VRWControl control;

    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);

    void Awake()
    {
        thisJoint = GetComponent<ConfigurableJoint>();
        thisWeap = GetComponent<Weapon>();
        control = FindObjectOfType<VRWControl>();
        velocityEstimator = GetComponent<VelocityEstimator>();

        // Assign rebindable buttons
        grabButton = control.grabButton;
        fireButton = control.fireButton;
        dropMag = control.dropMag;

        thisWeap.shotMask = control.shotMask;

    }

    ////// VRTK-friendly grabbing //////

    void OnTriggerStay(Collider col)
    {
        if (control.VRTKMode)
        {
            if (trackedObj == null || device == null || handJoint == null)
            {
                if ((trackedObj = col.GetComponentInParent<SteamVR_TrackedObject>()) != null)
                {
                    device = SteamVR_Controller.Input((int)trackedObj.index);
                    handJoint = trackedObj.GetComponent<ConfigurableJoint>();
                }
            }
            if (!control.showDeviceMainHand && disappearingHand == null && trackedObj != null)
            {
                disappearingHand = trackedObj.transform.FindChild("Model").gameObject;
            }
            if (device != null && !thisWeap.held && device.GetPressDown(grabButton) && (Time.time - thisWeap.dropTime > 0.2f) && !trackedObj.GetComponent<VRWFixedJointConnection>().isJointConnected)
            {

                transform.parent = trackedObj.transform;
                transform.localEulerAngles = thisWeap.adjustRotation;
                
                GetComponent<Rigidbody>().isKinematic = false;
                thisJoint.connectedAnchor = handJoint.anchor;
                thisJoint.connectedBody = trackedObj.GetComponent<Rigidbody>();
                trackedObj.GetComponent<VRWFixedJointConnection>().isJointConnected = true;
                control.SetAllJointAxesLocked(thisJoint);
                control.SetLayerInObjectAndChildren(this.gameObject, LayerMask.NameToLayer("HeldWeapon"));
                transform.parent = null;
                thisWeap.held = true;
                thisWeap.holdingDevice = trackedObj.gameObject;
                thisWeap.firingController = device;
                thisWeap.dropTime = Time.time;
                if (!control.showDeviceMainHand)
                {
                    disappearingHand.SetActive(false);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (control.VRTKMode && thisWeap.held)
        {
            if (device.GetPressDown(fireButton))
            {
                thisWeap.Fire(device);
            }
            else if (device.GetPressUp(fireButton) || !device.GetPress(fireButton))
            {
                thisWeap.StopFiring();
            }
            if (device.GetPressDown(grabButton) && (Time.time - thisWeap.dropTime > 0.2f))
            {
                trackedObj.GetComponent<VRWFixedJointConnection>().isJointConnected = false;
                control.SetAllJointAxesFree(thisJoint);
                thisJoint.connectedBody = null;
                thisWeap.held = false;
                thisWeap.holdingDevice = null;
                control.SetLayerInObjectAndChildren(this.gameObject, LayerMask.NameToLayer("Weapon"));
                thisWeap.dropTime = Time.time;
                if (!thisWeap.gripped)
                {
                    tossObject(this.GetComponent<Rigidbody>());
                }
                if (!control.showDeviceMainHand)
                {
                    disappearingHand.SetActive(true);
                }
                disappearingHand = null;
            }
            if (thisWeap.held && device.GetTouch(fireButton))
            {
                Vector2 triggerAngle = device.GetAxis(fireButton);
                thisWeap.TriggerPull(triggerAngle.x);
            }
            if (((device != null) && device.GetPressDown(dropMag)))
            {
                thisWeap.DropMag();
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (control.VRTKMode && !thisWeap.held)
        {
            trackedObj = null;
            device = null;
            handJoint = null;
        }
    }

    ////// SteamVR Interaction System below //////

    public void HandHoverUpdate(Hand hand)
    {
        if (handJoint == null)
        {
            handJoint = hand.GetComponent<ConfigurableJoint>();
        }
        if (!control.showDeviceMainHand && disappearingHand == null)
        {
            disappearingHand = hand.GetComponentInChildren<SpawnRenderModel>().gameObject;
            disappearingHandHighlight = hand.GetComponentInChildren<ControllerHoverHighlight>().gameObject;
        }
        if (((hand.controller != null) && hand.controller.GetPressDown(grabButton)) && (Time.time - thisWeap.dropTime > 0.2f))
        {
            if (hand.currentAttachedObject != gameObject && (!thisWeap.held/* || control.allowSwappingHands*/) && ((!thisWeap.canGrip && !thisWeap.canManip) || !thisWeap.held))
            {
                hand.AttachObject(gameObject, attachmentFlags);

                transform.parent = hand.transform;
                transform.localEulerAngles = thisWeap.adjustRotation;
                GetComponent<Rigidbody>().isKinematic = false;
                thisJoint.connectedAnchor = handJoint.anchor;
                thisJoint.connectedBody = hand.GetComponent<Rigidbody>();
                control.SetAllJointAxesLocked(thisJoint);
                control.SetLayerInObjectAndChildren(this.gameObject, LayerMask.NameToLayer("HeldWeapon"));
                transform.parent = null;
                thisWeap.held = true;
                thisWeap.holdingDevice = hand.gameObject;
                thisWeap.firingHand = hand;
                hand.HoverLock(GetComponent<Interactable>());
                if (!control.showDeviceMainHand)
                {
                    disappearingHand.SetActive(false);
                    disappearingHandHighlight.SetActive(false);
                }
                
                    
            }
            else if (thisWeap.holdingDevice == hand)
            {
                hand.DetachObject(gameObject);
                control.SetAllJointAxesFree(thisJoint);
                thisJoint.connectedBody = null;
                thisWeap.held = false;
                thisWeap.holdingDevice = null;
                control.SetLayerInObjectAndChildren(this.gameObject, LayerMask.NameToLayer("Weapon"));
                hand.HoverUnlock(GetComponent<Interactable>());
                thisWeap.dropTime = Time.time;
                

            }
        }
    }

    private void OnHandHoverEnd(Hand hand)
    {
        if (!thisWeap.held && hand.currentAttachedObject != thisWeap.gameObject)
        {
            handJoint = null;
            disappearingHand = null;
            disappearingHandHighlight = null;
        }
    }

    private void HandAttachedUpdate(Hand hand)
    {
        if ((hand.controller != null) && hand.controller.GetPressDown(grabButton))
        {
            hand.DetachObject(gameObject);
            hand.HoverUnlock(GetComponent<Interactable>());
            control.SetAllJointAxesFree(thisJoint);
            thisJoint.connectedBody = null;
            thisWeap.held = false;
            thisWeap.holdingDevice = null;
            control.SetLayerInObjectAndChildren(this.gameObject, LayerMask.NameToLayer("Weapon"));
            thisWeap.dropTime = Time.time;
            if (!control.showDeviceMainHand) {
                disappearingHand.SetActive(true);
                disappearingHandHighlight.SetActive(true);
            }
            handPos = hand.transform.position;
            if (!thisWeap.gripped)
            {
                tossObject(this.GetComponent<Rigidbody>(), hand);
            }
        }
        
        if (((hand.controller != null) && hand.controller.GetPressDown(fireButton)))
        {
            thisWeap.Fire(hand.controller);
        }
        if ((hand.controller == null) || ((hand.controller != null) && hand.controller.GetPressUp(fireButton)) || ((hand.controller != null) && !hand.controller.GetPress(fireButton)))
        {
            thisWeap.StopFiring();
        }

        //
        // Drop magazine if touchpad is pressed
        //
        if (((hand.controller != null) && hand.controller.GetPressDown(dropMag)))
        {
            thisWeap.DropMag();
        }

        //
        // Used for animating triggers, gets angle of trigger and passes to weapon
        //
        if (hand.controller.GetTouch(fireButton))
        {
            Vector2 triggerAngle = hand.controller.GetAxis(fireButton);
            thisWeap.TriggerPull(triggerAngle.x);
        }
        oldHandPos = hand.transform.position;

    }

    void tossObject(Rigidbody rigidbody)
    {
        Transform origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;

        if (origin != null)
        {
            rigidbody.velocity = origin.TransformVector(device.velocity) * control.throwForce;
            rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity) * control.throwForce;
        }
        else
        {
            rigidbody.velocity = device.velocity * control.throwForce;
            rigidbody.angularVelocity = device.angularVelocity * control.throwForce;
        }
    }

    void tossObject(Rigidbody rigidbody, Hand hand)
    {

        //////// Courtesy of Valve's Throwable script ////////

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 position = Vector3.zero;
        Vector3 velocity = Vector3.zero;
        Vector3 angularVelocity = Vector3.zero;
        if (hand.controller == null)
        {
            velocityEstimator.FinishEstimatingVelocity();
            velocity = velocityEstimator.GetVelocityEstimate();
            angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
            position = velocityEstimator.transform.position;
        }
        else
        {
            velocity = Player.instance.trackingOriginTransform.TransformVector(hand.controller.velocity);
            angularVelocity = Player.instance.trackingOriginTransform.TransformVector(hand.controller.angularVelocity);
            position = hand.transform.position;
        }

        Vector3 r = transform.TransformPoint(rb.centerOfMass) - position;
        rb.velocity = velocity + Vector3.Cross(angularVelocity, r);
        rb.angularVelocity = angularVelocity;

        // Make the object travel at the release velocity for the amount
        // of time it will take until the next fixed update, at which
        // point Unity physics will take over
        float timeUntilFixedUpdate = (Time.fixedDeltaTime + Time.fixedTime) - Time.time;
        transform.position += timeUntilFixedUpdate * velocity;
        float angle = Mathf.Rad2Deg * angularVelocity.magnitude;
        Vector3 axis = angularVelocity.normalized;
        transform.rotation *= Quaternion.AngleAxis(angle * timeUntilFixedUpdate, axis);

    }
}
