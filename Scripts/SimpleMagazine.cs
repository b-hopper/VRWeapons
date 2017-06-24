using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons
{
    [System.Serializable]
    public class SimpleMagazine : MonoBehaviour, IMagazine
    {
        IBulletBehavior roundType;

        [SerializeField]
        int maxRounds;

        int currentRoundCount;

        private void Start()
        {
            roundType = GetComponent<IBulletBehavior>();
            currentRoundCount = maxRounds;
        }

        public IBulletBehavior FeedRound()
        {
            IBulletBehavior tmp = null;

            if (currentRoundCount > 0)
            {
                tmp = roundType;
                currentRoundCount--;
            }

            return tmp;
        }

        public bool PushBullet(IBulletBehavior newRound)
        {
            return false;
        }

        public bool PopBullet()
        {
            return false;
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
        }

        public void MagOut(Weapon weap)
        {
            weap.Magazine = null;
        }        
    }
}