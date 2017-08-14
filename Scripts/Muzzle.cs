using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.InteractionSystem;
using VRWeapons;

namespace VRWeapons
{
    [RequireComponent(typeof(AudioSource))]
    public class Muzzle : MonoBehaviour, IMuzzleActions {

        AudioSource audioSource;
        Weapon thisWeapon;
        VRWControl control;
        GameObject currentFlash;

        //IObjectPool flashPool;

        Dictionary<GameObject, List<ParticleSystem>> flashPool;

        [SerializeField]
        public float range = 200, bulletSpreadRange, damage = 50;

        [Tooltip("Muzzle flash objects. If the object contains a Particle System, use empty parent GameObject and set particle system GameObjects as children. Flash objects should " +
            "be children of muzzle, and set inactive."), SerializeField]
        GameObject[] muzzleFlashes;

        void Start()
        {
            /* All the expensive GetComponent and FindObject operations are in Start(), to reduce runtime lag as much as possible. */
            thisWeapon = GetComponentInParent<Weapon>();
            audioSource = GetComponent<AudioSource>();
            control = FindObjectOfType<VRWControl>();
            flashPool = new Dictionary<GameObject, List<ParticleSystem>>();

            AddObjectsToFlashPool();
        }

        public void StartFiring(IBulletBehavior round)
        {
            Fire(round);
        }

        public void StopFiring()
        {
        }

        public void SetNewWeapon(Weapon newWeap)
        {
            thisWeapon = newWeap;
        }

        void Fire(IBulletBehavior round)
        {
            FireBullet(round);            
        }

        void FireBullet(IBulletBehavior round)
        {
            PlaySound(0);

            round.DoBulletBehavior(transform, damage, range, bulletSpreadRange, thisWeapon, control.shotMask);
            DoMuzzleFlash();
        }

        void PlaySound(int clip)
        {
            audioSource.Play();
        }

        void DoMuzzleFlash()
        {
            if (muzzleFlashes.Length > 0)
            {
                currentFlash = muzzleFlashes[Random.Range(0, muzzleFlashes.Length)];

                currentFlash.SetActive(true);

                currentFlash.transform.localPosition = Vector3.zero;

                List<ParticleSystem> tmp;
                if (flashPool.TryGetValue(currentFlash, out tmp))
                {
                    tmp = flashPool[currentFlash];
                    foreach (ParticleSystem a in tmp)
                    {
                        a.Clear();
                        a.Play();
                    }
                }
            }
        }

        void AddObjectsToFlashPool()
        {
            for (int i = 0; i < muzzleFlashes.Length; i++)
            {
                if (!muzzleFlashes[i].transform.parent)             // Checks to see if object is prefab or in-scene... Not foolproof, but I don't know of a better way.
                {
                    muzzleFlashes[i] = Instantiate(muzzleFlashes[i]);
                }
                List<ParticleSystem> tmp = new List<ParticleSystem>(muzzleFlashes[i].GetComponentsInChildren<ParticleSystem>());

                if (tmp.Count > 0)
                {
                    flashPool.Add(muzzleFlashes[i], tmp);
                }               

                muzzleFlashes[i].transform.parent = transform;

                muzzleFlashes[i].SetActive(false);

                tmp = null;
            }
        }
        
    }
}