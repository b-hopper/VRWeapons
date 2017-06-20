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

        AudioSource audioSource;
        AudioClip soundToPlay;
        
        bool isFiring, justFired, stopFiring;

        float nextFire;
        [HideInInspector]

        //// Shown in inspector ////
        [Tooltip("Weapon will never run out of ammo.")]
        [SerializeField]
        bool infiniteAmmo;

        [Tooltip("Bolt will rack forward after racking backward, unless magazine is inserted and empty.")]
        [SerializeField]
        public bool autoRackForward;

        [Tooltip("Bolt moves back on firing. Disable for bolt/pump action weapons.")]
        [SerializeField]
        bool boltMovesOnFiring;

        public ImpactProfile impactProfile;

        [SerializeField]
        FireMode fireMode;

        [Tooltip("Fire rate in seconds")]
        [SerializeField]        
        float fireRate;

        [Tooltip("Sound effect played when magazine is inserted.")]
        [SerializeField]
        AudioClip MagIn;
        [Tooltip("Sound effect played when magazine is removed.")]
        [SerializeField]
        AudioClip MagOut;
        [Tooltip("Sound effect played when bolt is moved back.")]
        [SerializeField]
        AudioClip SlideBack;
        [Tooltip("Sound effect played when bolt is moved forward.")]
        [SerializeField]
        AudioClip SlideForward;
        [Tooltip("Sound effect played when attempting to fire an empty weapon.")]
        [SerializeField]
        AudioClip DryFire;

        //// End shown in inspector ////

        [System.Serializable]
        public enum FireMode
        {
            SemiAuto = 1,
            Automatic = 2,
            Burst = 3
        }

        public enum AudioClips
        {
            MagIn = 0,
            MagOut = 1,
            SlideForward = 2,
            SlideBack = 3,
            DryFire = 4
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
            Ejector = GetComponentInChildren<IEjectorActions>();
            Kick = GetComponent<IKickActions>();
            Bolt.SetEjector(Ejector);
            audioSource = GetComponent<AudioSource>();
        }

        public void StartFiring(GameObject usingObject)
        {
            isFiring = true;
        }

        public void StopFiring(GameObject previousUsingObject)
        {
            stopFiring = true;
            isFiring = false;
        }

        public void ChamberNewRound(IBulletBehavior round)
        {
            chamberedRound = round;                             // TEMPORARY, to be handled by bolt actions
        }


        public void PlaySound(Weapon.AudioClips clip)
        {
            switch (clip)
            {
                case AudioClips.MagIn:
                    soundToPlay = MagIn;
                    break;
                case AudioClips.MagOut:
                    soundToPlay = MagOut;
                    break;
                case AudioClips.SlideForward:
                    soundToPlay = SlideForward;
                    break;
                case AudioClips.SlideBack:
                    soundToPlay = SlideBack;
                    break;
                case AudioClips.DryFire:
                    soundToPlay = DryFire;
                    break;
            }
            if (soundToPlay != null)
            {
                audioSource.pitch = Time.timeScale;
                audioSource.clip = soundToPlay;
                audioSource.Play();
            }

        }

        public Attack NewAttack(float newDamage, Vector3 newOrigin, RaycastHit newHit)
        {
            return new Attack
            {
                damage = newDamage,
                origin = newOrigin,
                hitInfo = newHit
            };
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
                        DoOnFireActions();
                        chamberedRound = null;
                        nextFire = Time.time;
                        justFired = true;
                    }
                    else if (Time.time - nextFire >= fireRate)
                    {
                        PlaySound(AudioClips.DryFire);
                        nextFire = Time.time;
                        justFired = true;
                    }
                }
            }
            else if (stopFiring)
            {
                stopFiring = false;
                justFired = false;
                Muzzle.StopFiring();
            }
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

        #endregion

        void DoOnFireActions()
        {
            if (Bolt != null && boltMovesOnFiring)
            {
                Bolt.BoltBack();
            }
            if (Kick != null)
            {
                Kick.Kick();
            }
        }
    }
}