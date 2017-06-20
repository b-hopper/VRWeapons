using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.InteractionSystem;
using VRWeapons;


namespace VRWeapons
{
    [RequireComponent(typeof(AudioSource))]
    public class Muzzle_Bullet : MonoBehaviour, IMuzzleActions {
        IKickActions Kick;
        IEjectorActions Ejector;
        IBoltActions Bolt;

        AudioSource audioSource;
        Weapon thisWeapon;
        VRWControl control;

        public float fireRate, range, bulletSpreadRange, damage;
        float nextFire;

        void Start()
        {
            thisWeapon = GetComponentInParent<Weapon>();
            audioSource = GetComponent<AudioSource>();
            control = FindObjectOfType<VRWControl>();

        }


        public void StartFiring(IBulletBehavior round)
        {
            Debug.Log("SCHUT");
            Fire(round);
        }

        public void StopFiring()
        {
            Debug.Log("STOPSCHUT");
        }

        void Fire(IBulletBehavior round)
        {
            if (Time.time - nextFire >= fireRate)   // NEEDS CHANGING, Weapon.cs should decide. Muzzle actions should only fire
            {
                FireBullet(round);
                nextFire = Time.time;
            }






            /*if (Time.time - nextFire >= fireRate)
            {
                PlaySound(0);
                if (thisWeapon.chamberedRound != null && bulletShell != null)
                {
                    DestroyImmediate(chamberedRound.gameObject);
                    if (!isBoltSeparate)
                    {
                        chamberedRound = thisWeapon.shellPool.GetNewObj();
                            Instantiate(bulletShell, chamberedRoundLocation.position, chamberedRoundLocation.rotation);
                        chamberedRound.transform.parent = slideObj.transform;
                    }
                    else
                    {
                        chamberedRound = Instantiate(bulletShell, chamberedRoundLocation.position, chamberedRoundLocation.rotation);
                        chamberedRound.transform.parent = separateBolt.transform;
                    }
                    chamberedRound.layer = gameObject.layer;
                    if (chamberedRound.GetComponent<Rigidbody>() != null)
                    {
                        chamberedRound.GetComponent<Rigidbody>().isKinematic = true;
                    }
                }
                RaycastHit hit;
                Vector3 shotLocation = (muzzleDirection.position - muzzle.position).normalized * range + (Random.insideUnitSphere * bulletSpreadRange);
                Debug.DrawRay(muzzle.position, shotLocation, Color.red, Mathf.Infinity);
                if (Physics.Raycast(muzzle.transform.position, shotLocation, out hit, range, shotMask))
                {
                    if (impactProfile != null)
                    {
                        ImpactInfo impact = impactProfile.GetImpactInfo(hit);
                        GameObject cloneImpact = Instantiate(impact.GetRandomPrefab(), hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                        cloneImpact.transform.parent = hit.transform;
                    }
                    if (hit.transform.GetComponent<Rigidbody>() != null)
                    {
                        hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(force * (muzzleDirection.position - muzzle.position).normalized, hit.point);
                    }

                    var attack = new Attack
                    {
                        damage = damage,
                        headshotMultiplier = headshotMultiplier,
                        origin = muzzle.position,
                        hitInfo = hit
                    };
                    if (usesLineRenderer)
                    {
                        shotLineRenderer.SetPosition(0, muzzle.position);
                        shotLineRenderer.SetPosition(1, hit.point);
                        StartCoroutine(DrawLine());
                    }
                    //ExecuteEvents.Execute<IAttackReceiver>(hit.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(attack)));
                }
                else
                {
                    if (usesLineRenderer)
                    {
                        shotLineRenderer.SetPosition(0, muzzle.position);
                        shotLineRenderer.SetPosition(1, shotLocation);
                        StartCoroutine(DrawLine());
                    }
                }

                if (device != null)
                    StartCoroutine(forceFeedback(device));
                if (!infiniteAmmo) { chambered = false; }
                StartCoroutine(MuzzleFlash());
                StartCoroutine(Kick());
                nextFire = Time.time;
                justFired = true;
            }*/
        }

        void FireBullet(IBulletBehavior round)
        {
            PlaySound(0);

            round.DoBulletBehavior(transform, damage, range, bulletSpreadRange, thisWeapon, control.shotMask);
            
            if (Kick != null)
            {
                Kick.Kick(thisWeapon.transform, null);
            }
            if (Ejector != null)
            {
                Ejector.Eject();
            }
            if (Bolt != null/* && thisWeap.boltMovesOnFiring*/)
            {
                Bolt.BoltBack();
            }
        }

        void PlaySound(int clip)
        {
            Debug.Log("Play Sound");
            audioSource.Play();
        }

        public void SetEjector(IEjectorActions newEjector)
        {
            Ejector = newEjector;
        }

        public void SetKick(IKickActions newKick)
        {
            Kick = newKick;
        }

        public void SetBolt(IBoltActions newBolt)
        {
            Bolt = newBolt;
        }
    }
}