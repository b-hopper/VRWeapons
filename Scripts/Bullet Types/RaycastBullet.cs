using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VRWeapons.BulletTypes
{
    [System.Serializable]
    public class RaycastBullet : MonoBehaviour, IBulletBehavior
    {
        [SerializeField]
        float shotForce;

        public void DoBulletBehavior(Transform muzzleDir, float damage, float range, float bulletSpreadRange, Weapon thisWeapon, LayerMask shotMask)
        {
            RaycastHit hit;
            Vector3 shotLocation = (muzzleDir.forward * range) + (Random.insideUnitSphere * bulletSpreadRange);
            Debug.DrawRay(muzzleDir.position, shotLocation, Color.red, 15f);
            if (Physics.Raycast(muzzleDir.position, shotLocation, out hit, range, shotMask))
            {
                ExecuteEvents.Execute<IAttackReceiver>(hit.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(thisWeapon.NewAttack(damage, transform.position, hit))));

                if (thisWeapon.impactProfile != null)
                {
                    ImpactInfo impact = thisWeapon.impactProfile.GetImpactInfo(hit);
                    GameObject cloneImpact = Instantiate(impact.GetRandomPrefab(), hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;   // Need to decide where Object Pool goes, here
                    cloneImpact.transform.parent = hit.transform;
                }
                Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.AddForceAtPosition(shotForce * (muzzleDir.forward).normalized, hit.point);
                }
            }
        }
    }
}