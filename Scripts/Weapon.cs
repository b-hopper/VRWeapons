using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace VRWeapons
{

    [RequireComponent(typeof(AudioSource))]

    [System.Serializable]
    public class Weapon : MonoBehaviour
    {
        IMuzzleActions Muzzle;
        IEjectorActions Ejector;
        IKickActions Kick;
        IBoltActions Bolt;
        IObjectPool shellPool;
        public IBulletBehavior chamberedRound;
        public IMagazine Magazine;

        AudioSource audioSource;
        AudioClip soundToPlay;

        bool isFiring, justFired, stopFiring;
        int burstCount;

        [HideInInspector]
        public bool secondHandGripped;

        float nextFire;

        Vector3 triggerAngleStart;

        //// Shown in inspector ////
        [Tooltip("Weapon will never run out of ammo."), SerializeField]
        bool infiniteAmmo;

        [Tooltip("Bolt will rack forward after racking backward, unless magazine is inserted and empty."), SerializeField]
        public bool autoRackForward;


        [Tooltip("Bolt moves back on firing. Disable for bolt/pump action weapons."), SerializeField]
        bool boltMovesOnFiring;

        public ImpactProfile impactProfile;

        [SerializeField]
        FireMode fireMode;

        [Tooltip("Fire rate in seconds"), SerializeField]        
        float fireRate;

        [Tooltip("Only applies to burst fire mode."), SerializeField]
        int burstAmount;

        [Tooltip("Trigger GameObject. Used to accurately rotate weapon's trigger on controller trigger pull."), SerializeField]
        Transform trigger;

        [Tooltip("End rotation of trigger, when fully pressed down."), SerializeField]
        Vector3 triggerEndRotation;

        [Tooltip("Sound effect played when magazine is inserted."), SerializeField]
        AudioClip MagIn;

        [Tooltip("Sound effect played when magazine is removed."), SerializeField]
        AudioClip MagOut;

        [Tooltip("Sound effect played when bolt is moved back."), SerializeField]
        AudioClip SlideBack;

        [Tooltip("Sound effect played when bolt is moved forward."), SerializeField]
        AudioClip SlideForward;

        [Tooltip("Sound effect played when attempting to fire an empty weapon."), SerializeField]
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
            Muzzle = GetComponentInChildren<IMuzzleActions>();
            Bolt = GetComponentInChildren<IBoltActions>();
            Ejector = GetComponentInChildren<IEjectorActions>();
            Kick = GetComponent<IKickActions>();
            shellPool = GetComponent<IObjectPool>();
            Magazine = GetComponentInChildren<IMagazine>();
            Bolt.SetEjector(Ejector);
            audioSource = GetComponent<AudioSource>();
            if (trigger != null) {
                triggerAngleStart = trigger.localEulerAngles;
            }

            if (Magazine != null)
            {
                Magazine.MagIn(this);       // Required for internal magazines
            }
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
            chamberedRound = round;
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

        public void DropMagazine()
        {
            if (Magazine != null)
            {
                Magazine.MagOut(this);
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
                if (!justFired || fireMode == FireMode.Automatic || (fireMode == FireMode.Burst && burstCount < burstAmount))
                {
                    if ((Time.time - nextFire >= fireRate) && IsChambered())
                    {
                        Muzzle.StartFiring(chamberedRound);
                        DoOnFireActions();
                        chamberedRound = null;
                        nextFire = Time.time;
                        justFired = true;
                        burstCount++;
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
                burstCount = 0;
                Muzzle.StopFiring();
            }
        }

        #region Attachment-Friendly functions
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
            if (Bolt != null)
            {
                if (shellPool != null)
                {
                    Bolt.ReplaceRoundWithEmptyShell(shellPool.GetNewObj());
                }
                if (boltMovesOnFiring)
                {
                    Bolt.BoltBack();
                }
            }
            if (Kick != null)
            {
                Kick.Kick();
            }            
        }

        public void SetTriggerAngle(float angle)
        {
            if (trigger != null)
            {
                trigger.localRotation = Quaternion.Lerp(Quaternion.Euler(triggerAngleStart), Quaternion.Euler(triggerEndRotation), angle);
            }
        }

        public bool IsWeaponFiring()
        {
            return isFiring;
        }
    }
}