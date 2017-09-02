using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons
{
    [System.Serializable]
    public class SimpleMagazine : MagazineBase, IMagazine
    {
        public event EventHandler MagDropped;

        IBulletBehavior roundType;

        Rigidbody rb;

        [SerializeField]
        protected int maxRounds;

        [Tooltip("If toggled, magazine is able to be removed from the weapon. Turn off if weapon is using an internal magazine."), SerializeField]
        bool canBeDetached = true;

        [SerializeField]
        bool infiniteAmmo;
        
        public bool CanMagBeDetached { get { return canBeDetached; } set { canBeDetached = value; } }
        
        int currentRoundCount;

        protected override void Awake()
        {
            base.Awake();

            roundType = GetComponent<IBulletBehavior>();
            if (roundType == null)
            {
                Debug.LogWarning("IBulletBehaviour not found", this);
            }

            currentRoundCount = maxRounds;
        }

        protected virtual void Start()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    Debug.LogWarning("Rigidbody not found", this);
                }
            }
        }

        public IBulletBehavior FeedRound()
        {
            IBulletBehavior tmp = null;

            if (infiniteAmmo || PopBullet())
            {
                tmp = roundType;
            }
            return tmp;
        }

        public virtual bool TryPushBullet(GameObject newRound)
        {
            bool val = false;
            if (currentRoundCount < maxRounds)
            {
                currentRoundCount++;
                val = true;
            }
            return val;
        }

        public virtual bool PopBullet()
        {
            bool tmp = false;
            if (currentRoundCount > 0)
            {
                tmp = true;
                currentRoundCount--;
            }
            return tmp;
        }

        public virtual int GetCurrentRoundCount()
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
            if (rb != null && canBeDetached)
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