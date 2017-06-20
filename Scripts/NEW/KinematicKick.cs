using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons
{
    public class KinematicKick : MonoBehaviour, IKickActions
    {
        Weapon thisWeap;
        Vector3 originalPos, originalRot, targetPos, targetRot, currentPos, currentRot;
        float rotLerpVal, posLerpVal;
        bool fixPosition, isKicking, originalPosSet;
        int shotsFiredSinceReset;

        [Tooltip("Amount (and direction) the weapon moves positionally when fired. Logarithmically tapers down with each shot.")]
        [SerializeField]
        Vector3 amountToMove;
        [Tooltip("Amount (and direction) the weapon rotates when fired. Logarithmically tapers down with each shot.")]
        [SerializeField]
        Vector3 amountToRotate;
        [Tooltip("How quickly the weapon recoils.")]
        [SerializeField]
        float recoilLerpSpeed;
        [Tooltip("How quickly the weapon recovers.")]
        [SerializeField]
        float recoverLerpSpeed;

        private void Start()
        {
            thisWeap = GetComponent<Weapon>();
        }

        public void Kick()
        {
            if (!originalPosSet)
            {
                originalPos = transform.localPosition;  // Only set original position if it's back to zero.
                originalPosSet = true;
            }

            shotsFiredSinceReset++;
            currentPos = transform.localPosition;
            targetPos = new Vector3(targetPos.x + (amountToMove.x * 1/shotsFiredSinceReset),
                targetPos.y + (amountToMove.y * 1 / shotsFiredSinceReset),
                targetPos.z + (amountToMove.z * 1 / shotsFiredSinceReset));

            isKicking = true;
        }

        private void Update()
        {
            if (isKicking)
            {
                transform.localPosition = Vector3.Lerp(currentPos, targetPos, posLerpVal);
                Debug.Log(posLerpVal);
                posLerpVal += recoilLerpSpeed;
                if (posLerpVal >= 1)
                {
                    fixPosition = true;
                    isKicking = false;
                }
            }
            else if (fixPosition)
            {
                Debug.Log("Fixing position");
                transform.localPosition = Vector3.Lerp(originalPos, transform.localPosition, posLerpVal);
                posLerpVal -= recoverLerpSpeed;
                if (posLerpVal <= 0)
                {
                    fixPosition = false;
                    shotsFiredSinceReset = 0;
                    targetPos = originalPos;
                }
            }
        }
    }
}