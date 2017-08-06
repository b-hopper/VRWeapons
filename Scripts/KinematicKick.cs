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

        MonoBehaviour muzzleGO;

        [Tooltip("How much to kick back when the weapon is fired. Logarithmically tapers down with each shot."), SerializeField, Range(0, 1)]
        float positionalKickStrength = 0.05f;
        
        [Tooltip("Amount (and direction) the weapon rotates when fired. Logarithmically tapers down with each shot."), SerializeField]
        Vector3 amountToRotate = new Vector3(-5, 0, 0);

        [Tooltip("How quickly the weapon recoils. 1 is instant, 0 is no movement."), SerializeField, Range(0f, 1f)]
        float recoilLerpSpeed = 0.334f;

        [Tooltip("How quickly the weapon recovers. 1 is instant, 0 is no movement."), SerializeField, Range(0f, 1f)]
        float recoverLerpSpeed = 0.05f;

        [Tooltip("Decreases the amount of kick when 2-hand gripped by multiplication. 0 = no kick, 1 = full kick."), SerializeField, Range(0f, 1f)]
        float twoHandGripKickReduction = 0.5f;
        
        private void Start()
        {
            thisWeap = GetComponent<Weapon>();

            muzzleGO = thisWeap.gameObject.GetComponentInChildren<IMuzzleActions>() as MonoBehaviour;
        }

        public void Kick()
        {
            float tmpKickReduction = 1;
            if (!originalPosSet)
            {
                originalPos = transform.localPosition;
                originalRot = transform.localEulerAngles;
                originalPosSet = true;
            }

            if (thisWeap.secondHandGripped)
            {
                tmpKickReduction *= twoHandGripKickReduction;
            }

            shotsFiredSinceReset++;
            currentPos = transform.localPosition;
            
            targetPos = ((transform.localPosition - transform.forward) * positionalKickStrength) / shotsFiredSinceReset;

            if (!thisWeap.secondHandGripped)
            {
                currentRot = transform.localEulerAngles;

                targetRot = new Vector3(currentRot.x + (amountToRotate.x / shotsFiredSinceReset),
                    currentRot.y + (amountToRotate.y / shotsFiredSinceReset),
                    currentRot.z + (amountToRotate.z / shotsFiredSinceReset));
            }
            isKicking = true;
        }

        private void Update()
        {
            if (isKicking)
            {
                DoPositionalRecoil();
                if (!thisWeap.secondHandGripped)
                {
                    DoRotationalRecoil();
                }
            }
            else if (fixPosition)
            {
                DoPositionalRecovery();
                if (!thisWeap.secondHandGripped)
                {
                    DoRotationalRecovery();
                }
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
            transform.Translate(transform.InverseTransformDirection(targetPos) * recoilLerpSpeed);
            lerpVal += recoilLerpSpeed;
        }

        void DoPositionalRecovery()
        { 
            if (!thisWeap.IsWeaponFiring())
            {
                shotsFiredSinceReset = 0;
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