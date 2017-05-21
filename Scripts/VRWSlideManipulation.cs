using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]

public class VRWSlideManipulation : MonoBehaviour
{
    public Transform startPosition;
    public Transform endPosition;
    public LinearMapping linearMapping;
    public bool repositionGameObject = true;
    public bool maintainMomemntum = true;
    public float momemtumDampenRate = 5.0f;

    private float initialMappingOffset;
    private int numMappingChangeSamples = 5;
    private float[] mappingChangeSamples;
    private float prevMapping = 0.0f;
    private float mappingChangeRate;
    private int sampleCount = 0;

    Vector3 originalPosition;
    Hand manipulatingHand;
    public GameObject slideObj;
    Weapon thisWeap;
    bool slideMovesOnFiring, autoRackForward, justManip, isManip, isActive, clearDevices;
    float slideLeeway, lastLinMapValue;
    VRWControl control;
    Valve.VR.EVRButtonId grabButton, fireButton, dropMag;
    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);
    Collider thisCollider;
    SteamVR_Controller.Device device;
    SteamVR_TrackedObject trackedObj;

    void Awake()
    {
        thisCollider = GetComponent<BoxCollider>();
        isActive = true;
        originalPosition = transform.localPosition;
        control = FindObjectOfType<VRWControl>();
        thisWeap = GetComponentInParent<Weapon>();

        slideMovesOnFiring = thisWeap.slideMovesOnFiring;
        autoRackForward = thisWeap.autoRackForward;
        slideLeeway = thisWeap.slideLeeway;

        grabButton = control.grabButton;
        fireButton = control.fireButton;

        mappingChangeSamples = new float[numMappingChangeSamples];
        thisWeap.chamberClosed = true;

        if (thisWeap.slideTime < 1)
        {
            thisWeap.slideTime = 1;
        }
        if (thisWeap.slideLeeway <= 0)
        {
            thisWeap.slideLeeway = 0.01f;
        }
        if (control.VRTKMode)
        {
            if (GetComponent<Rigidbody>() == null)
            {
                gameObject.AddComponent<Rigidbody>();       // Requires Rigidbody to detect trigger collisions, bypassed by SteamVRIS so only needed for VRTK
            }
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    //-------------------------------------------------
    void Start()
    {
        if (linearMapping == null)
        {
            linearMapping = GetComponent<LinearMapping>();
        }

        if (linearMapping == null)
        {
            linearMapping = gameObject.AddComponent<LinearMapping>();
        }

        initialMappingOffset = linearMapping.value;

        if (repositionGameObject)
        {
            UpdateLinearMapping(transform);
        }
    }

    void FixedUpdate()
    {
        if (clearDevices && !isManip)
        {
            device = null;
            trackedObj = null;
            clearDevices = false;
        }
        if (!control.VRTKMode)
        {
            if (isActive && !thisWeap.held && thisCollider != null)
            {
                thisCollider.enabled = false;
                isActive = false;
            }
            else if (!isActive && thisWeap.held)
            {
                thisCollider.enabled = true;
                isActive = true;
            }
            if (manipulatingHand != null && (isManip && (manipulatingHand.controller.GetPress(grabButton) || manipulatingHand.controller.GetPress(fireButton))) && thisWeap.holdingDevice != manipulatingHand.gameObject && thisWeap.held)
            {
                UpdateLinearMapping(manipulatingHand.transform);
                isManip = true;
            }
            if (manipulatingHand != null && (manipulatingHand.controller.GetPressUp(fireButton) || manipulatingHand.controller.GetPressUp(grabButton)))
            {
                isManip = false;
            }
        } else
        {
            if (isActive && !thisWeap.held && thisCollider != null)
            {
                thisCollider.enabled = false;
                isActive = false;
            }
            else if (!isActive && thisWeap.held)
            {
                thisCollider.enabled = true;
                isActive = true;
            }
            if (device != null && (isManip && (device.GetPress(grabButton) || device.GetPress(fireButton))) && thisWeap.holdingDevice != trackedObj.gameObject && thisWeap.held)
            {
                UpdateLinearMapping(trackedObj.transform);
                isManip = true;
            }
            if (device != null && (device.GetPressUp(fireButton) || device.GetPressUp(grabButton)))
            {
                isManip = false;
            }
        }


        ////// Checks for slide functionality //////

        if (slideObj != null)
        {
            if (!isManip && autoRackForward && !thisWeap.isAttachment)
            {
                if (linearMapping.value > slideLeeway && !thisWeap.justFired && !thisWeap.moving)
                {
                    StartCoroutine(thisWeap.RackForward(linearMapping.value, true));
                }
            }
            if (thisWeap.chamberOpen && linearMapping.value < slideLeeway && !thisWeap.firing)
            {
                thisWeap.chamberOpen = false;
                if (!thisWeap.chambered)
                {
                    thisWeap.Chamber();
                }
                thisWeap.chamberClosed = true;
                if (justManip)
                {
                    thisWeap.PlaySound(thisWeap.soundSource, 4);
                }
                justManip = false;
            }

            if (thisWeap.chamberClosed && linearMapping.value > (1 - slideLeeway) && !thisWeap.firing && isManip)
            {
                thisWeap.chamberOpen = true;
                thisWeap.chamberClosed = false;
                thisWeap.PlaySound(thisWeap.soundSource, 3);
                justManip = true;
                thisWeap.Eject();
                thisWeap.chambered = false;
                

            }
            if (isManip && thisWeap.justFired)
            {
                thisWeap.justFired = false;
            }
        }
        if (linearMapping.value != lastLinMapValue)
        {
            SetSlidePosition(linearMapping.value);
        }

        lastLinMapValue = linearMapping.value;

        ////// End checks //////


    }

    void SetSlidePosition (float value)
    { 
        if (thisWeap.slideRotTriggerValue != 0)
        {
            float rotLerpValue = Mathf.InverseLerp(0, thisWeap.slideRotTriggerValue, value);
            float posLerpValue = Mathf.InverseLerp(thisWeap.slideRotTriggerValue, 1, value);

            if (rotLerpValue < 1)
            {
                slideObj.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(thisWeap.slideRotStart), Quaternion.Euler(thisWeap.slideRotEnd), rotLerpValue);
                slideObj.transform.localPosition = thisWeap.slideStart;
            }
            else
            {
                slideObj.transform.localEulerAngles = thisWeap.slideRotEnd;
                slideObj.transform.localPosition = Vector3.Lerp(thisWeap.slideStart, thisWeap.slideEnd, posLerpValue);
            }
        }
        else if (!thisWeap.isBoltSeparate)
        {
            slideObj.transform.localPosition = Vector3.Lerp(thisWeap.slideStart, thisWeap.slideEnd, value);
        }
        else if (thisWeap.isBoltSeparate)
        {
            if (thisWeap.separateBoltOpen)
            {
                if (linearMapping.value < 1 - slideLeeway)
                {
                    Vector3 separatePos = transform.TransformPoint(thisWeap.separateBolt.transform.localPosition);
                    slideObj.transform.localPosition = Vector3.Lerp(thisWeap.slideStart, thisWeap.slideEnd, value);
                    thisWeap.separateBolt.transform.localPosition = transform.InverseTransformPoint(separatePos);
                }
                else
                {
                    thisWeap.separateBoltOpen = false;
                    slideObj.transform.localPosition = Vector3.Lerp(thisWeap.slideStart, thisWeap.slideEnd, value);
                }
            }
            else
            {
                thisWeap.separateBoltOpen = false;
                slideObj.transform.localPosition = Vector3.Lerp(thisWeap.slideStart, thisWeap.slideEnd, value);
            }
        }
        
        
    }

    ////// VRTK-Friendly grab below //////

    void OnTriggerStay(Collider col)
    {
        if (control.VRTKMode)
        {
            if (col.tag == "ReloadPoint")
            {
                return;
            }
            if (trackedObj == null || device == null)
            {
                if ((trackedObj = col.GetComponentInParent<SteamVR_TrackedObject>()) != null)
                {
                    device = SteamVR_Controller.Input((int)trackedObj.index);
                }
            }
            if (trackedObj.gameObject == thisWeap.holdingDevice)
            {
                trackedObj = null;
            }
            thisWeap.canManip = true;
            if ((!thisWeap.canGrip || thisWeap.ableToGripAndManip) && trackedObj != null && thisWeap.held)
            {
                if (device.GetPressDown(grabButton) || device.GetPressDown(fireButton))
                {
                    isManip = true;

                    initialMappingOffset = linearMapping.value - CalculateLinearMapping(trackedObj.transform);
                    sampleCount = 0;
                    mappingChangeRate = 0.0f;
                }
                if (device.GetPressUp(grabButton) || device.GetPressUp(fireButton))
                {
                    CalculateMappingChangeRate();
                    isManip = false;
                }
                if (device.GetPress(grabButton) || device.GetPress(fireButton))
                {
                    isManip = true;
                    UpdateLinearMapping(trackedObj.transform);
                }
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (control.VRTKMode && !isManip)
        {
            trackedObj = null;
            device = null;
        }
        clearDevices = true;
        thisWeap.canManip = false;
    }

    ////// SteamVR Interaction System below //////

    private void HandHoverUpdate(Hand hand)
    {
        manipulatingHand = hand;
        thisWeap.canManip = true;
        if ((!thisWeap.canGrip || thisWeap.ableToGripAndManip) && thisWeap.holdingDevice != hand.gameObject && thisWeap.held)
        {
            if (((hand.controller != null) && (hand.controller.GetPressDown(grabButton)) || hand.controller.GetPressDown(fireButton)))
            {
                //hand.HoverLock(GetComponent<Interactable>());
                isManip = true;

                initialMappingOffset = linearMapping.value - CalculateLinearMapping(hand.transform);
                sampleCount = 0;
                mappingChangeRate = 0.0f;

            }
            if (((hand.controller != null) && (hand.controller.GetPressUp(grabButton)) || hand.controller.GetPressUp(fireButton)))
            {
                //hand.HoverUnlock(GetComponent<Interactable>());

                CalculateMappingChangeRate();
                isManip = false;
            }
            //////////////////////
            if (hand.controller.GetPress(grabButton) || hand.controller.GetPress(fireButton))
            {
                isManip = true;
                UpdateLinearMapping(hand.transform);
            }
        }

    }

    private void OnHandHoverEnd(Hand hand)
    {
        thisWeap.canManip = false;
    }

    #region Linear Drive
    //-- SteamVR's Linear Drive functions ---------------------------------------------
    private void CalculateMappingChangeRate()
    {
        //Compute the mapping change rate
        mappingChangeRate = 0.0f;
        int mappingSamplesCount = Mathf.Min(sampleCount, mappingChangeSamples.Length);
        if (mappingSamplesCount != 0)
        {
            for (int i = 0; i < mappingSamplesCount; ++i)
            {
                mappingChangeRate += mappingChangeSamples[i];
            }
            mappingChangeRate /= mappingSamplesCount;
        }
    }


    //-------------------------------------------------
    private void UpdateLinearMapping(Transform tr)
    {
        prevMapping = linearMapping.value;
        linearMapping.value = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(tr));

        mappingChangeSamples[sampleCount % mappingChangeSamples.Length] = (1.0f / Time.deltaTime) * (linearMapping.value - prevMapping);
        sampleCount++;

        if (repositionGameObject)
        {
            transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
        }
    }


    //-------------------------------------------------
    private float CalculateLinearMapping(Transform tr)
    {
        Vector3 direction = endPosition.position - startPosition.position;
        float length = direction.magnitude;
        direction.Normalize();

        Vector3 displacement = tr.position - startPosition.position;

        return Vector3.Dot(displacement, direction) / length;
    }


    //-------------------------------------------------
    void Update()
    {
        if (maintainMomemntum && mappingChangeRate != 0.0f)
        {
            //Dampen the mapping change rate and apply it to the mapping
            mappingChangeRate = Mathf.Lerp(mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime);
            linearMapping.value = Mathf.Clamp01(linearMapping.value + (mappingChangeRate * Time.deltaTime));

            if (repositionGameObject)
            {
                transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
            }
        }
    }
}
#endregion


