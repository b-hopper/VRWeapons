using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Interactable))]

public class VRWAttachment : MonoBehaviour, IAttachmentEvents {
    public UnityEvent OnTriggerPress;
    public UnityEvent OnTriggerLongPress;
    VRTK.Weapon thisWeap;
    public float longPressTimeNeeded = 1;
    Hand currentHand;
    SteamVR_Controller.Device device;
    SteamVR_TrackedObject trackedObj;

    VRWControl control;
    Valve.VR.EVRButtonId fireButton, dropMag;
    Collider thisCollider;

    bool isPressed, isActive, inCollider;
    float pressTime, totalPressTime;

    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);

    void Awake()
    {
        thisCollider = GetComponent<Collider>();
        control = FindObjectOfType<VRWControl>();
        isActive = true;
        
        fireButton = control.fireButton;
        dropMag = control.dropMag;
        
        thisWeap = GetComponentInParent<VRTK.Weapon>();

        if (control.VRTKMode)
        {
            if (GetComponent<Rigidbody>() == null)
            {
                gameObject.AddComponent<Rigidbody>();
            }
            GetComponent<Rigidbody>().isKinematic = true;
        }

    }

    void FixedUpdate()
    {
        totalPressTime = (Time.time - pressTime);
        if (!control.VRTKMode)
        {
            if (thisWeap != null && isActive && !thisWeap.held)
            {
                thisCollider.enabled = false;
                isActive = false;
            }
            else if (thisWeap == null || thisWeap.held)
            {
                thisCollider.enabled = true;
                isActive = true;
            }

            if (isPressed && currentHand.controller.GetPressUp(fireButton) && totalPressTime < longPressTimeNeeded)
            {
                isPressed = false;
                OnTriggerPress.Invoke();
            }

            if (isPressed && totalPressTime >= longPressTimeNeeded)
            {
                OnTriggerLongPress.Invoke();
                isPressed = false;
            }
        }
        else
        {
            if (thisWeap != null && isActive && !thisWeap.held)
            {
                thisCollider.enabled = false;
                isActive = false;
            }
            else if (thisWeap == null || thisWeap.held)
            {
                thisCollider.enabled = true;
                isActive = true;
            }

            if (isPressed && device.GetPressUp(fireButton) && totalPressTime < longPressTimeNeeded)
            {
                isPressed = false;
                OnTriggerPress.Invoke();
            }

            if (isPressed && totalPressTime >= longPressTimeNeeded)
            {
                OnTriggerLongPress.Invoke();
                isPressed = false;
            }
            if (trackedObj != null && !isPressed && !inCollider)
            {
                trackedObj = null;
                device = null;
            }
        }

    }

    void OnTriggerStay(Collider col)
    {
        if (control.VRTKMode)
        {
            inCollider = true;
            if (trackedObj == null || device == null)
            {
                if ((trackedObj = col.GetComponentInParent<SteamVR_TrackedObject>()) != null)
                {
                    device = SteamVR_Controller.Input((int)trackedObj.index);
                }
            }
            if (device != null && device.GetPressDown(fireButton) && col.gameObject != thisWeap.holdingDevice)
            {
                pressTime = Time.time;
                isPressed = true;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (control.VRTKMode && !isPressed)
        {
            trackedObj = null;
            device = null;
        }
        inCollider = false;
    }

    private void HandHoverUpdate(Hand hand)
    {
        currentHand = hand;
        if (hand.controller.GetPressDown(fireButton) && hand != thisWeap.firingHand)
        {
            pressTime = Time.time;
            isPressed = true;
        }
    }
}
