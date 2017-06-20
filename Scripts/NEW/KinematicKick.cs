using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons
{
    public class KinematicKick : MonoBehaviour, IKickActions
    {
        Weapon thisWeap;
        Vector3 originalPos, originalRot, targetPos, targetRot, currentPos, currentRot;
        float lerpVal;
        bool fixPosition, isKicking, originalPosSet;
        int shotsFiredSinceReset;

        [Tooltip("Amount (and direction) the weapon moves positionally when fired. Logarithmically tapers down with each shot.")]
        [SerializeField]
        Vector3 amountToMove = new Vector3(0, 0.05f, -0.025f);
        [Tooltip("Amount (and direction) the weapon rotates when fired. Logarithmically tapers down with each shot.")]
        [SerializeField]
        Vector3 amountToRotate = new Vector3(-5, 0, 0);
        [Tooltip("How quickly the weapon recoils. 1 is instant, 0 is no movement.")]
        [SerializeField]
        float recoilLerpSpeed = 0.334f;
        [Tooltip("How quickly the weapon recovers. 1 is instant, 0 is no movement.")]
        [SerializeField]
        float recoverLerpSpeed = 0.05f;

        private void Start()
        {
            thisWeap = GetComponent<Weapon>();
        }

        public void Kick()
        {
            if (!originalPosSet)
            {
                originalPos = transform.localPosition;  // Only set original position if it's back to zero.
                originalRot = transform.localEulerAngles;
                originalPosSet = true;
            }

            shotsFiredSinceReset++;
            currentPos = transform.localPosition;
            targetPos = new Vector3(currentPos.x + (amountToMove.x * 1 / shotsFiredSinceReset),
                currentPos.y + (amountToMove.y * 1 / shotsFiredSinceReset),
                currentPos.z + (amountToMove.z * 1 / shotsFiredSinceReset));

            currentRot = transform.localEulerAngles;
            targetRot = new Vector3(currentRot.x + (amountToRotate.x * 1 / shotsFiredSinceReset),
                currentRot.y + (amountToRotate.y * 1 / shotsFiredSinceReset),
                currentRot.z + (amountToRotate.z * 1 / shotsFiredSinceReset));

            isKicking = true;
        }

        private void Update()
        {
            if (isKicking)
            {
                DoPositionalRecoil();
                DoRotationalRecoil();
            }
            else if (fixPosition)
            {
                DoPositionalRecovery();
                DoRotationalRecovery();
            }
        }

        void DoPositionalRecoil()
        {
            if (lerpVal >= 1)
            {
                lerpVal = 1;
                fixPosition = true;
                isKicking = false;
            }
            transform.localPosition = Vector3.Lerp(currentPos, targetPos, lerpVal);
            lerpVal += recoilLerpSpeed;
        }

        void DoPositionalRecovery()
        { 
            if (!thisWeap.IsWeaponFiring())
            {
                shotsFiredSinceReset = 0;
                Debug.Log("Shots fired since reset... reset");
            }
            if (lerpVal <= 0)
            {
                lerpVal = 0;
                fixPosition = false;
                targetPos = originalPos;
            }
            transform.localPosition = Vector3.Lerp(originalPos, transform.localPosition, lerpVal);
            lerpVal -= recoverLerpSpeed;
        }

        void DoRotationalRecoil()
        {
            transform.localEulerAngles = Vector3.Lerp(currentRot, targetRot, lerpVal);
        }

        void DoRotationalRecovery()
        {
            transform.localEulerAngles = Vector3.Lerp(originalRot, transform.localEulerAngles, lerpVal);
        }
    }
}