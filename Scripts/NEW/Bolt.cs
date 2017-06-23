using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VRWeapons
{
    public class Bolt : MonoBehaviour, IBoltActions
    {

        Weapon thisWeap;
        IEjectorActions Ejector;

        float boltLerpPos, boltMoveSpeed, lastBoltLerpPos;
        bool movingBack, movingForward, isManip, canChamberNewRound, justPlayedSoundForward = true,
            justPlayedSoundBack, doNotPlaySound, justEjected;

        public bool boltMovesSeparate;
        public int slideTimeInFrames;

        Rigidbody chamberedRoundRB;
        Transform chamberedRoundT;

        [Tooltip("Location of round on bolt face. Should be child of bolt. Align round with desired location, then set it inactive."), SerializeField]
        Transform chamberedRoundSnapT;

        public Transform boltGroup, bolt;
        public Vector3 GroupStartPosition;
        public Vector3 GroupEndPosition;
        Vector3 BoltStartPosition { set; get; }
        Vector3 BoltEndPosition { set; get; }


        private void Start()
        {
            thisWeap = GetComponentInParent<Weapon>();
            boltMoveSpeed = 1 / (float)slideTimeInFrames;
        }

        public void SetEjector(IEjectorActions newEjector)
        {
            Ejector = newEjector;
        }

        public void BoltBack()
        {
            movingBack = true;
        }

        public IBulletBehavior ChamberNewRound()
        {
            if (thisWeap.Magazine != null)
            {
                // Getting Rigidbody and Transform to snap into position and eject correctly
                chamberedRoundRB = thisWeap.Magazine.GetRoundRigidBody();
                chamberedRoundT = thisWeap.Magazine.GetRoundTransform();

                // Setting round in correct position on bolt face
                if (chamberedRoundT != null)
                {
                    chamberedRoundT.parent = transform;
                    chamberedRoundT.localEulerAngles = chamberedRoundSnapT.localEulerAngles;
                    chamberedRoundT.localPosition = chamberedRoundSnapT.localPosition;
                }

                // Setting up the chambered round to prepare for firing
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
                doNotPlaySound = true;
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
                    doNotPlaySound = false;
                    boltLerpPos = 0;
                    movingForward = false;
                    if (canChamberNewRound)
                    {
                        thisWeap.chamberedRound = ChamberNewRound();
                        justEjected = false;
                        canChamberNewRound = false;
                    }
                }

            }
            
            if (boltLerpPos >= 0.9f)
            {
                if (!justEjected)
                {
                    if (Ejector != null && chamberedRoundT != null && chamberedRoundRB != null)
                    {
                        Ejector.Eject(chamberedRoundT, chamberedRoundRB);
                    }
                }

                justEjected = true;
                canChamberNewRound = true;
            }
            

            if (!isManip && boltLerpPos > 0 && thisWeap.autoRackForward && !movingBack)
            {
                movingForward = true;
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
                if (!doNotPlaySound && !justPlayedSoundBack && boltLerpPos > 0.9f)
                {
                    thisWeap.PlaySound(Weapon.AudioClips.SlideBack);
                    justPlayedSoundBack = true;
                    justPlayedSoundForward = false;
                }
                else if (!doNotPlaySound && !justPlayedSoundForward && boltLerpPos < 0.1f )
                {
                    thisWeap.PlaySound(Weapon.AudioClips.SlideForward);
                    justPlayedSoundForward = true;
                    justPlayedSoundBack = false;
                }
            }

            lastBoltLerpPos = boltLerpPos;
        }

        public void SetLerpValue(float val)
        {
            boltLerpPos = val;
        }

        public void IsCurrentlyBeingManipulated(bool val) { isManip = val; }
        public float GetLerpValue() { return boltLerpPos; }
        public Vector3 GetMinValue() { return GroupStartPosition; }
        public Vector3 GetMaxValue() { return GroupEndPosition; }
    }
}