using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]

public class VRWRound : MonoBehaviour {

    public int weaponType, amountPerRound = 1;
    bool ableToUse;
    ConfigurableJoint handJoint;
    Rigidbody rBody;
    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);
    Valve.VR.EVRButtonId grabButton;
    VelocityEstimator velocityEstimator;
    VRWControl control;
    SteamVR_Controller.Device device;
    SteamVR_TrackedObject trackedObj;
    public bool held;
    float dropTime;

    void Awake()
    {
        velocityEstimator = GetComponent<VelocityEstimator>();
        control = FindObjectOfType<VRWControl>();
        rBody = GetComponent<Rigidbody>();
        
        grabButton = control.grabButton;
    }

    IEnumerator dropProtect()
    {
        yield return new WaitForSeconds(0.1f);
        GetComponent<Collider>().enabled = true;
        yield return new WaitForSeconds(0.9f);
        ableToUse = true;
        yield break;
    }

    public void SetAbleToUse(bool value)
    {
        if (value)
        {
            IEnumerator drop = dropProtect();
            StartCoroutine(drop);
        }
        else
        {
            ableToUse = value;
        }
    }

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
            if (!held && device != null && device.GetPressDown(grabButton) && (Time.time - dropTime > 0.2f))
            {
                transform.parent = trackedObj.transform;
                transform.localEulerAngles = Vector3.zero;
                transform.localPosition = Vector3.zero;
                rBody.isKinematic = true;
                held = true;
                dropTime = Time.time;
            }
        }
    }


    void OnTriggerExit(Collider col)
    {
        if (control.VRTKMode && !held)
        {
            trackedObj = null;
            device = null;
        }
    }

    void FixedUpdate()
    {
        if (control.VRTKMode)
        {
            if (held && device.GetPress(grabButton) && (Time.time - dropTime > 0.2f))
            {
                transform.parent = null;
                rBody.isKinematic = false;
                held = false;
                tossObject(rBody);
                dropTime = Time.time;
            }
            if (held && transform.parent != trackedObj.transform)
            {
                held = false;
                trackedObj = null;
                device = null;
            }
        }
    }

    private void HandHoverUpdate(Hand hand)
    {
        handJoint = hand.GetComponent<ConfigurableJoint>();
        if (((hand.controller != null) && hand.controller.GetPressDown(grabButton)))
        {
            if (hand.currentAttachedObject != gameObject)
            {
                held = true;
                hand.HoverLock(GetComponent<Interactable>());

                hand.AttachObject(gameObject, attachmentFlags);

                rBody.isKinematic = true;
                
            }
            else
            {
                held = false;
                hand.DetachObject(gameObject);

                rBody.isKinematic = false;
                tossObject(rBody, hand);
                hand.HoverUnlock(GetComponent<Interactable>());

            }

        }
    }

    #region Useful functions

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

    #endregion
}
