using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class VRW_Muzzle_Bullet : MonoBehaviour, VRTK.IMuzzleActions {
    AudioSource audioSource;
    VRTK.VRW_Weapon weapon;

    void Awake()
    {
        weapon = GetComponentInParent<VRTK.VRW_Weapon>();
        audioSource = GetComponent<AudioSource>();
    }


    public void StartFiring()
    {
        Debug.Log("SCHUT");
    }

    public void StopFiring()
    {
        Debug.Log("STOPSCHUT");
    }

    void Fire()
    {

        if (Time.time - nextFire >= fireRate)
        {
            PlaySound(shotSoundSource, 0);
            if (chamberedRound != null && bulletShell != null)
            {
                DestroyImmediate(chamberedRound.gameObject);
                if (!isBoltSeparate)
                {
                    chamberedRound = Instantiate(bulletShell, chamberedRoundLocation.position, chamberedRoundLocation.rotation);
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
        }
    }

}
