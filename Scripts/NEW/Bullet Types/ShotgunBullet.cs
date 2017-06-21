using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VRWeapons {

    public class ShotgunBullet : MonoBehaviour, IBulletBehavior{

        [Tooltip("Shotgun pellets per shell.")]
        [SerializeField]
        int shotgunPellets = 10;

	    public void DoBulletBehavior(Transform muzzleDir, float damage, float range, float bulletSpreadRange, Weapon thisWeapon, LayerMask shotMask)
        {
            for (int i = 0; i < shotgunPellets; i++)
            {
                RaycastHit hit;
                Vector3 shotLocation = (muzzleDir.forward * range) + (Random.insideUnitSphere * bulletSpreadRange);
                Debug.DrawRay(muzzleDir.position, shotLocation, Color.red, 15f);
                if (Physics.Raycast(muzzleDir.position, shotLocation, out hit, range, shotMask))
                {
                    ExecuteEvents.Execute<IAttackReceiver>(hit.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(thisWeapon.NewAttack(damage, transform.position, hit))));
                }

                /*RaycastHit hit;
                Debug.DrawRay(muzzle.position, (muzzleDirection.position - muzzle.position).normalized, Color.red, Mathf.Infinity);
                if (Physics.Raycast(muzzle.transform.position, (muzzleDirection.position - muzzle.position).normalized + (Random.insideUnitSphere * bulletSpreadRange), out hit, range, shotMask))
                {
                    if (impactProfile != null)
                    {
                        ImpactInfo impact = impactProfile.GetImpactInfo(hit);
                        GameObject cloneImpact = Instantiate(impact.GetRandomPrefab(), hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                        cloneImpact.transform.parent = hit.transform;
                    }
                    if (hit.rigidbody != null)
                    {
                        hit.rigidbody.AddForceAtPosition(force * (muzzleDirection.position - muzzle.position).normalized, hit.point);
                    }

                    var attack = new Attack
                    {
                        damage = damage,
                        headshotMultiplier = headshotMultiplier,
                        origin = muzzle.position,
                        hitInfo = hit
                    };
                    //ExecuteEvents.Execute<IAttackReceiver>(hit.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(attack)));
                }*/
            }
        }
    }
}