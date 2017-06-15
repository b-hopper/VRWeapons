using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRW_Bolt : MonoBehaviour, IVRW_BoltActions {
    
    VRW_Weapon thisWeap;

    float boltLerpPos, boltMoveSpeed, lastBoltLerpPos;
    bool movingBack, movingForward;

    public bool boltMovesSeparate;
    public int slideTimeInFrames;

    public Transform boltGroup, bolt;
    public Vector3 GroupStartPosition;
    public Vector3 GroupEndPosition;
    Vector3 BoltStartPosition { set; get; }
    Vector3 BoltEndPosition { set; get; }


    private void Start()
    {
        thisWeap = GetComponentInParent<VRW_Weapon>();
        boltMoveSpeed = 1 / (float)slideTimeInFrames;
    }

    public void BoltBack()
    {
        movingBack = true;
    }

    private void FixedUpdate()
    {
        if (movingBack)
        {
            Debug.Log("Moving back, lerp val: " + boltLerpPos + ", bolt move speed: " + boltMoveSpeed);
            boltLerpPos += boltMoveSpeed;
            if (boltLerpPos >= 1)
            {
                boltLerpPos = 1;
                movingBack = false;
                if (thisWeap.autoRackForward)
                {
                    movingForward = true;
                }
            }
        }

        else if (movingForward)
        {
            Debug.Log("Moving forward, lerp val: " + boltLerpPos);
            boltLerpPos -= boltMoveSpeed;
            if (boltLerpPos <= 0)
            {
                boltLerpPos = 0;
                movingForward = false;
            }
        }

        if (lastBoltLerpPos != boltLerpPos)
        {
            if (boltMovesSeparate)
            {
                bolt.transform.localPosition = Vector3.Lerp(BoltStartPosition, BoltEndPosition, boltLerpPos);
            }
            else
            {
                boltGroup.transform.localPosition = Vector3.Lerp(GroupStartPosition, GroupEndPosition, boltLerpPos);
            }
        }

        lastBoltLerpPos = boltLerpPos;
    }

}
