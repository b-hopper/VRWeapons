using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons
{
    [System.Serializable]
    public class SimpleMagazine : MonoBehaviour, IMagazine
    {
        public event EventHandler MagDropped;

        IBulletBehavior roundType;

        Rigidbody rb;

        [SerializeField]
        int maxRounds;

        [Tooltip("If toggled, magazine is able to be removed from the weapon. Turn off if weapon is using an internal magazine."), SerializeField]
        bool canBeDetached = true;
        
        public bool CanMagBeDetached { get { return canBeDetached; } set { canBeDetached = value; } }

        int currentRoundCount;

        private void Start()
        {
            roundType = GetComponent<IBulletBehavior>();
            rb = GetComponent<Rigidbody>();
            currentRoundCount = maxRounds;

            if (roundType == null)
            {
                Debug.LogWarning("IBulletBehaviour not found", this);
            }
            if(rb == null)
            {
                Debug.LogWarning("Rigidbody not found", this);
            }
        }

        public IBulletBehavior FeedRound()
        {
            IBulletBehavior tmp = null;

            if (PopBullet())
            {
                tmp = roundType;
            }
            return tmp;
        }

        public bool PushBullet(GameObject newRound)
        {
            bool val = false;
            if (currentRoundCount < maxRounds)
            {
                currentRoundCount++;
                val = true;
            }
            return val;
        }

        public bool PopBullet()
        {
            bool tmp = false;
            if (currentRoundCount > 0)
            {
                tmp = true;
                currentRoundCount--;
            }
            return tmp;
        }

        public int GetCurrentRoundCount()
        {
            return currentRoundCount;
        }

        public Rigidbody GetRoundRigidBody()
        {
            return null;
        }

        public Transform GetRoundTransform()
        {
            return null;
        }

        public void MagIn(Weapon weap)
        {
            weap.Magazine = this;
            weap.PlaySound(Weapon.AudioClips.MagIn);
            transform.parent = weap.transform;
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }

        public void MagOut(Weapon weap)
        {
            if (canBeDetached)
            {
                weap.Magazine = null;
                weap.PlaySound(Weapon.AudioClips.MagOut);
                transform.parent = null;
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
                OnMagDropped();
            }
        }

        private void OnMagDropped()
        {
            if (MagDropped != null)
            {
                MagDropped(this, EventArgs.Empty);
            }
        }
    }
}