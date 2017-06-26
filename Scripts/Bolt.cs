using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VRWeapons
{
    public class Bolt : MonoBehaviour, IBoltActions
    {

        Weapon thisWeap;
        IEjectorActions Ejector;
        IObjectPool spentShellPool;

        public float boltLerpVal { set; get; }
        float boltMoveSpeed, lastboltLerpVal;
        bool movingBack, movingForward, isManip, canChamberNewRound, justPlayedSoundForward = true,
            justPlayedSoundBack, doNotPlaySound, justEjected;

        Rigidbody chamberedRoundRB;
        Transform chamberedRoundT;

        [HideInInspector]
        public bool justManip;
        
        [Tooltip("Used for cases where the bolt object is separate from the charging handle, and only the bolt should move when weapon" +
            " is fired."), SerializeField]
        bool boltMovesSeparate;

        [Tooltip("How many frames it takes for bolt to move from fully closed to fully open, and vice versa."), SerializeField]
        int slideTimeInFrames;

        [Tooltip("Weapon will start chambered if this is toggled."), SerializeField]
        bool startChambered;

        [Tooltip("Location of round on bolt face. Should be child of bolt. Align round with desired location, then set it inactive."), SerializeField]
        Transform chamberedRoundSnapT;

        [Tooltip("Used when charging handle is separate from actual bolt. Bolt should be a child of the charging handle, in this case."), SerializeField]
        Transform boltGroup;

        [Tooltip("Bolt's transform. Assign only this, if charging handle does not move separately from bolt."), SerializeField]
        Transform bolt;

        [Tooltip("Position of bolt group when bolt is fully closed. This should be the position value of the parent charging handle, if " +
            "bolt moves separately."), SerializeField]
        public Vector3 GroupStartPosition;

        [Tooltip("Position of bolt group when bolt is fully open. This should be the position value of the parent charging handle, if " +
            "bolt moves separately."), SerializeField]
        public Vector3 GroupEndPosition;

        [Tooltip("Position of bolt when fully closed. If bolt moves separate from charging handle, this is the position value of the child " +
            "bolt object."), SerializeField]
        Vector3 BoltStartPosition;

        [Tooltip("Position of bolt when fully open. If bolt moves separate from charging handle, this is the position value of the child " +
            "bolt object."), SerializeField]
        Vector3 BoltEndPosition;



        private void Start()
        {
            thisWeap = GetComponentInParent<Weapon>();
            spentShellPool = GetComponent<IObjectPool>();

            if (startChambered)
            {
                movingBack = true;
            }

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
                    chamberedRoundT.gameObject.SetActive(true);
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
            if (movingBack || startChambered)
            {
                doNotPlaySound = true;
                boltLerpVal += boltMoveSpeed;


                if (boltLerpVal >= 1)
                {
                    startChambered = false;
                    boltLerpVal = 1;
                    movingBack = false;
                    if (thisWeap.autoRackForward)
                    {
                        movingForward = true;
                    }
                }
            }

            else if (movingForward)
            {
                boltLerpVal -= boltMoveSpeed;
                if (boltLerpVal <= 0)
                {
                    boltLerpVal = 0;
                    movingForward = false;
                }
            }
            
            if (boltLerpVal <= 0.05f)
            {
                doNotPlaySound = false;
                justManip = false;
                boltLerpVal = 0;
                if (canChamberNewRound)
                {
                    thisWeap.chamberedRound = ChamberNewRound();
                    justEjected = false;
                    canChamberNewRound = false;
                }
            }

            else if (boltLerpVal >= 0.9f)
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
            

            if (!isManip && boltLerpVal > 0 && thisWeap.autoRackForward && !movingBack)
            {
                movingForward = true;
            }

            if (lastboltLerpVal != boltLerpVal)
            {
                if (boltMovesSeparate && justManip)
                {
                    boltGroup.transform.localPosition = Vector3.Lerp(GroupStartPosition, GroupEndPosition, boltLerpVal);
                }
                else if (boltMovesSeparate && !justManip)
                {
                    boltGroup.transform.localPosition = GroupStartPosition;
                    bolt.transform.localPosition = Vector3.Lerp(BoltStartPosition, BoltEndPosition, boltLerpVal);
                }
                else
                {
                    bolt.transform.localPosition = Vector3.Lerp(BoltStartPosition, BoltEndPosition, boltLerpVal);
                }
                if (!doNotPlaySound && !justPlayedSoundBack && boltLerpVal > 0.9f)
                {
                    thisWeap.PlaySound(Weapon.AudioClips.SlideBack);
                    justPlayedSoundBack = true;
                    justPlayedSoundForward = false;
                }
                else if (!doNotPlaySound && !justPlayedSoundForward && boltLerpVal < 0.1f )
                {
                    thisWeap.PlaySound(Weapon.AudioClips.SlideForward);
                    justPlayedSoundForward = true;
                    justPlayedSoundBack = false;
                }
            }

            lastboltLerpVal = boltLerpVal;
        }


        public void ReplaceRoundWithEmptyShell(GameObject go)
        {
            go.transform.parent = chamberedRoundT.parent;
            go.transform.localPosition = chamberedRoundT.localPosition;
            go.transform.localEulerAngles = chamberedRoundT.localEulerAngles;
            DestroyImmediate(chamberedRoundT.gameObject);
            chamberedRoundT = go.transform;
            chamberedRoundRB = go.GetComponent<Rigidbody>();
            chamberedRoundRB.isKinematic = true;
        }


        public void IsCurrentlyBeingManipulated(bool val)
        {
            justManip = true;
            isManip = val;
        }
        public Vector3 GetMinValue() { return GroupStartPosition; }
        public Vector3 GetMaxValue() { return GroupEndPosition; }
    }
}