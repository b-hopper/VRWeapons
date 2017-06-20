using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace VRWeapons
{

    [RequireComponent(typeof(AudioSource))]


    public class Weapon : MonoBehaviour
    {
        IMuzzleActions Muzzle;
        IEjectorActions Ejector;
        IKickActions Kick;
        IBoltActions Bolt;
        IObjectPool shellPool;
        IObjectPool flashPool;
        public IBulletBehavior chamberedRound;
        public IMagazine Magazine;
        public bool infiniteAmmo, autoRackForward;
        bool isFiring, justFired;

        [SerializeField]
        FireMode fireMode;

        [SerializeField]
        float fireRate;
        float nextFire;

        [System.Serializable]
        public enum FireMode
        {
            SemiAuto = 1,
            Automatic = 2
        }
                
        [System.Serializable]
        public struct Attack
        {
            public float damage;
            public Vector3 origin;
            public RaycastHit hitInfo;
        }

        private void Start()
        {
            Muzzle = GetComponentInChildren<IMuzzleActions>();  // TEMPORARY, to be assigned in-editor later
            Bolt = GetComponentInChildren<IBoltActions>();
            Muzzle.SetBolt(Bolt);

        }

        public void StartFiring(GameObject usingObject)
        {
            isFiring = true;
        }

        public void StopFiring(GameObject previousUsingObject)
        {
            isFiring = false;
        }

        public void ChamberNewRound(IBulletBehavior round)
        {
            chamberedRound = round;                             // TEMPORARY, to be handled by bolt actions
        }


        void PlaySound(int clip) { }

        public Attack NewAttack(float newDamage, Vector3 newOrigin, RaycastHit newHit)
        {
            return new Attack
            {
                damage = newDamage,
                origin = newOrigin,
                hitInfo = newHit
            };
        }

        #region Editor-Friendly functions
        public void SetMuzzle(IMuzzleActions newMuzzle)
        {
            Muzzle = newMuzzle;
        }

        public void SetEjector(IEjectorActions newEjector)
        {
            Ejector = newEjector;
        }

        bool IsChambered()
        {
            bool val = false;
            if (chamberedRound != null)
            {
                val = true;
            }
            return val;
        }

        private void FixedUpdate()
        {
            if (isFiring)
            {
                if (!justFired || fireMode == FireMode.Automatic)
                {
                    if ((Time.time - nextFire >= fireRate) && IsChambered())
                    {
                        Muzzle.StartFiring(chamberedRound);
                        chamberedRound = null;
                        justFired = true;
                    }
                }
            }
            else
            {
                justFired = false;
                Muzzle.StopFiring();
            }
        }
        #endregion
    }
}