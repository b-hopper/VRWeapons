using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(VelocityEstimator))]

public class magazine : MonoBehaviour
{
    public int ammo;
    public float magEjectForce;
    public VRTK.Weapon weapon;
    public int weaponType;
    public bool ableToUse = true;
    public Vector3 adjustPos, adjustRot, magEjectDir;
    Hand holdingHand;

    public bool held;
    float dropTime;

    Collider thisCol;
    VRWControl control;
    Valve.VR.EVRButtonId grabButton;
    Rigidbody rBody;
    VelocityEstimator velocityEstimator;
    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);
    public GameObject[] roundsInMag;
    int physicalRoundCount;
    SteamVR_Controller.Device device;
    SteamVR_TrackedObject trackedObj;

    void Awake()
    {
        velocityEstimator = GetComponent<VelocityEstimator>();
        physicalRoundCount = roundsInMag.Length;

        control = FindObjectOfType<VRWControl>();
        rBody = GetComponent<Rigidbody>();
        thisCol = GetComponent<Collider>();
        // Assign rebindable buttons
        grabButton = control.grabButton;
    }

    void FixedUpdate()
    {
        if (ammo < physicalRoundCount && roundsInMag[ammo] != null)
        {
            Destroy(roundsInMag[ammo]);
        }
        if (control.VRTKMode)
        {
            if (held && device.GetPressDown(grabButton) && (Time.time - dropTime > 0.2f))
            {
                transform.parent = null;
                rBody.isKinematic = false;
                held = false;
                tossObject(rBody);
                StartCoroutine(dropProtect());
                dropTime = Time.time;
            }
            if (held && transform.parent != trackedObj.transform)
            {
                held = false;
                trackedObj = null;
                device = null;
            }
        }
        else
        {
            if (held && !ableToUse)
            {
                held = false;
                holdingHand.DetachObject(this.gameObject);
                holdingHand.HoverUnlock(GetComponent<Interactable>());
                holdingHand = null;
                StartCoroutine(dropProtect());
            }
        }
    }

    public void dropMag()
    {
        transform.parent = null;
        rBody.AddForce(magEjectDir.normalized * magEjectForce, ForceMode.Impulse);
        this.gameObject.layer = LayerMask.NameToLayer("Weapon");
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = false;
        }
        StartCoroutine(dropProtect());
        held = false;

    }

    IEnumerator dropProtect()
    {
        yield return new WaitForSeconds(0.1f);
        thisCol.enabled = true;
        yield return new WaitForSeconds(0.9f);
        ableToUse = true;
        yield break;
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
            if (!held && device != null && device.GetPressDown(grabButton) && (Time.time - dropTime > 0.2f) && trackedObj.GetComponentInChildren<magazine>() == null)
            {
                transform.parent = trackedObj.transform;
                transform.localEulerAngles = adjustRot;
                transform.localPosition = adjustPos;
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

    ////// SteamVR Interaction System below //////

    private void HandHoverUpdate(Hand hand)
    {
        if (((hand.controller != null) && hand.controller.GetPressDown(grabButton)))
        {
            if (hand.currentAttachedObject != gameObject)
            {
                held = true;
                hand.HoverLock(GetComponent<Interactable>());

                hand.AttachObject(gameObject, attachmentFlags);

                rBody.isKinematic = true;
                holdingHand = hand;

                transform.localPosition = adjustPos;
                transform.localEulerAngles = adjustRot;
            }
            else
            {
                held = false;
                hand.DetachObject(gameObject);

                holdingHand = null;

                rBody.isKinematic = false;
                tossObject(rBody, hand);
                hand.HoverUnlock(GetComponent<Interactable>());

            }

        }
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
