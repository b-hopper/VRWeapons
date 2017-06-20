using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class Bolt_TrackObjectGrabAttach : VRTK_InteractableObject
{
    IBoltActions i;

    MonoBehaviour j;
    
    Transform thisBolt;

    float boltLerpValue, lerpValue;

    bool hasMoved;

    Vector3 boltMin, boltMax, startPos, currentPos;
    [SerializeField]
    Vector3 thisMin, thisMax;


    private void Start()
    {
        i = transform.parent.GetComponentInChildren<IBoltActions>();
        j = i as MonoBehaviour;
        thisBolt = j.transform;
        boltMin = i.GetMinValue();
        boltMax = i.GetMaxValue();
        startPos = transform.localPosition;
        currentPos = transform.localPosition;
    }

    protected override void Update()
    {
        float oldLerpValue = lerpValue;
        base.Update();
        if (IsGrabbed())
        {
            i.IsCurrentlyBeingManipulated(true);
            hasMoved = true;
            currentPos = new Vector3(startPos.x, startPos.y, transform.localPosition.z);
            transform.localPosition = currentPos;
            if (transform.localPosition.z < thisMax.z)
            {
                currentPos = new Vector3(transform.localPosition.x, transform.localPosition.y, thisMax.z);
                transform.localPosition = currentPos;
            }
            else if (transform.localPosition.z > thisMin.z)
            {
                currentPos = new Vector3(transform.localPosition.x, transform.localPosition.y, thisMin.z);
                transform.localPosition = currentPos;
            }
            lerpValue = VRWControl.InverseLerp(thisMin, thisMax, transform.localPosition);
            i.SetLerpValue(lerpValue);
        }
        else if (hasMoved)
        {
            i.IsCurrentlyBeingManipulated(false);
            transform.localPosition = currentPos;
            hasMoved = false;
        }
        else
        {
            lerpValue = i.GetLerpValue();
        }
        
        if (lerpValue != oldLerpValue)
        {
            transform.localPosition = Vector3.Lerp(thisMin, thisMax, lerpValue);
        }


        
    }
    
}
