using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VRWeapons;

namespace VRWeapons
{
    public class RaycastBullet : MonoBehaviour, IBulletBehavior
    {
        public IMagazine thisMag;
       
        public void DoBulletBehavior(Transform muzzleDir, float damage, float range, float bulletSpreadRange, Weapon thisWeapon, LayerMask shotMask)
        {
            RaycastHit hit;
            Vector3 shotLocation = (muzzleDir.forward * range) + (Random.insideUnitSphere * bulletSpreadRange);
            Debug.DrawRay(muzzleDir.position, shotLocation, Color.red, 15f);
            if (Physics.Raycast(muzzleDir.position, shotLocation, out hit, range, shotMask))
            {
                ExecuteEvents.Execute<IAttackReceiver>(hit.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(thisWeapon.NewAttack(damage, transform.position, hit))));
            }
        }
    }
}