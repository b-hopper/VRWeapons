using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;


namespace VRWeapons
{
    public class Bolt : MonoBehaviour, IBoltActions
    {

        VRWeapons.Weapon thisWeap;

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
            thisWeap = GetComponentInParent<VRWeapons.Weapon>();
            boltMoveSpeed = 1 / (float)slideTimeInFrames;
        }

        public void BoltBack()
        {
            movingBack = true;
        }

        public IBulletBehavior ChamberNewRound()
        {
            if (thisWeap.Magazine != null)
            {
                return thisWeap.Magazine.FeedRound();
            }
            else
            {
                return null;
            }
        }

        private void FixedUpdate()
        {
            if (movingBack)
            {
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
                boltLerpPos -= boltMoveSpeed;
                if (boltLerpPos <= 0)
                {
                    boltLerpPos = 0;
                    movingForward = false;
                    thisWeap.chamberedRound = ChamberNewRound();
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
}