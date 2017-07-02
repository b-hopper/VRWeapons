using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class Bolt_InteractableObject : VRTK_InteractableObject
{
    IBoltActions bolt;

    MonoBehaviour boltGO;    

    Vector3 controllerLocation, offset;

    float boltLerpValue, lerpValue;

    Weapon_VRTK_InteractableObject thisWeapIntObj;

    bool hasMoved, thisObjectIsGrabbed, needsOffset = true;

    Vector3 boltMin, boltMax, startPos, currentPos, startRot;
    [SerializeField]
    Vector3 boltClosedPosition, boltOpenPosition;

    [SerializeField]
    bool isSecondHandGrip;


    private void Start()
    {
        thisWeapIntObj = GetComponentInParent<Weapon_VRTK_InteractableObject>();
        bolt = transform.parent.GetComponentInChildren<IBoltActions>();
        boltGO = bolt as MonoBehaviour;
        boltMin = bolt.GetMinValue();
        boltMax = bolt.GetMaxValue();
        startPos = transform.localPosition;
        currentPos = transform.localPosition;
        startRot = transform.localEulerAngles;

        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    protected override void Update()
    {
        float oldLerpValue = lerpValue;
        base.Update();

        if (isSecondHandGrip && thisWeapIntObj.GetSecondaryGrabbingObject() != null)
        {
            if (needsOffset)
            {
                offset = thisWeapIntObj.GetSecondaryGrabbingObject().transform.position - transform.position;
                needsOffset = false;
            }
            controllerLocation = transform.InverseTransformPoint(thisWeapIntObj.GetSecondaryGrabbingObject().transform.position) - offset;
            thisObjectIsGrabbed = true;
        }
        else if (IsGrabbed())
        {
            thisObjectIsGrabbed = true;
            controllerLocation = transform.localPosition;
        }
        else
        {
            controllerLocation = transform.localPosition;
            thisObjectIsGrabbed = false;
            needsOffset = true;
        }

        if (thisObjectIsGrabbed)
        {
            bolt.IsCurrentlyBeingManipulated(true);
            hasMoved = true;

            controllerLocation = V3Clamp(controllerLocation, boltOpenPosition, boltClosedPosition);

            transform.localPosition = controllerLocation;


            lerpValue = VRWControl.V3InverseLerp(boltClosedPosition, boltOpenPosition, controllerLocation);
            
            bolt.boltLerpVal = lerpValue;
            Debug.Log(lerpValue);
        }
        else if (hasMoved)
        {
            bolt.IsCurrentlyBeingManipulated(false);
            transform.localPosition = currentPos;
            hasMoved = false;
        }
        else
        {
            lerpValue = bolt.boltLerpVal;
        }
        
        if (lerpValue != oldLerpValue)
        {
            transform.localPosition = Vector3.Lerp(boltClosedPosition, boltOpenPosition, lerpValue);
        }
        transform.localEulerAngles = startRot;
    }    

    Vector3 V3Clamp(Vector3 value, Vector3 min, Vector3 max)
    {
        Vector3 tmp = value;
        tmp = new Vector3(Mathf.Clamp(tmp.x, min.x, max.x), Mathf.Clamp(tmp.y, min.y, max.y), Mathf.Clamp(tmp.z, min.z, max.z));
        return tmp;
    }
}
