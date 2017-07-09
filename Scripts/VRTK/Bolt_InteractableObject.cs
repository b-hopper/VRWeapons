using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class Bolt_InteractableObject : VRTK_InteractableObject
{
    IBoltActions bolt; 

    Vector3 controllerLocation, offset;

    float boltLerpValue, lerpValue;

    Weapon_VRTK_InteractableObject thisWeapIntObj;
    Weapon thisWeap;

    bool hasMoved, thisObjectIsGrabbed, needsOffset = true;

    Vector3 startPos, startRot;
    [SerializeField]
    Vector3 boltClosedPosition, boltOpenPosition;

    [SerializeField]
    bool isSecondHandGrip;


    private void Start()
    {
        thisWeapIntObj = GetComponentInParent<Weapon_VRTK_InteractableObject>();
        bolt = transform.parent.GetComponentInChildren<IBoltActions>();
        startPos = transform.localPosition;
        startRot = transform.localEulerAngles;
        thisWeap = GetComponentInParent<Weapon>();

        Physics.IgnoreCollision(GetComponent<Collider>(), thisWeap.weaponBodyCollider, true);

        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true;                                                                   // Don't want the manipulator to fall down off the bolt!
        }
    }
    
    protected override void Update()
    {
        float oldLerpValue = lerpValue;

        if (isSecondHandGrip && thisWeapIntObj.GetSecondaryGrabbingObject() != null)                                        // Used for weapons where grip point is also slide manipulator
        {                                                                                                                   // Manually moves position of slide manipulator to match grabbing object
            transform.position = thisWeapIntObj.GetSecondaryGrabbingObject().transform.position;
            thisObjectIsGrabbed = true;
        }
        else if (IsGrabbed())
        {
            ClampControllerToTrack();
            thisObjectIsGrabbed = true;                                                                                     // Have to set a flag, because can't rely on VRTK's grab mechanisms if the 
        }                                                                                                                   // bolt manipulator is also the second grab point. VRTK only allows one
        else                                                                                                                // object to be grabbed at a time, far as I know.
        {
            thisObjectIsGrabbed = false;
        }

        if (thisObjectIsGrabbed)
        {
            bolt.IsCurrentlyBeingManipulated(true);
            hasMoved = true;

            ClampControllerToTrack();                                                                                       // Clamps to make sure it stays on the tracks it has been assigned in inspector

            lerpValue = VRWControl.V3InverseLerp(boltClosedPosition, boltOpenPosition, transform.localPosition);            // Final lerp value of bolt, which is then passed...
            
            bolt.boltLerpVal = lerpValue;                                                                                   // ... to the bolt itself.
        }

        else if (hasMoved)
        {
            bolt.IsCurrentlyBeingManipulated(false);
            hasMoved = false;
            ClampControllerToTrack();
        }

        else
        {
            lerpValue = bolt.boltLerpVal;
        }
        
        if (lerpValue != oldLerpValue)                                                                                      // Moves the manipulator to be in the correct position (with the bolt object). Only 
        {                                                                                                                   // moves it if the lerp value has changed - doesn't need adjustment otherwise.
            transform.localPosition = Vector3.Lerp(boltClosedPosition, boltOpenPosition, lerpValue);
        }

        base.Update();

        transform.localEulerAngles = startRot;                                                                              // Rotation isn't an issue, but to avoid making the collider feel like it's in a 
    }                                                                                                                       // weird position, make sure the rotation is set back to how it originally was.
                                                                                                                            // Before this, the bolt manipulator was rotated with the controller.
    void ClampControllerToTrack()
    {
        transform.localPosition = VRWControl.V3Clamp(transform.localPosition, boltOpenPosition, boltClosedPosition);
    }
}
