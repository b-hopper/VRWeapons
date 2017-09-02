using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VRWeapons.BulletTypes.Projectile;

public class RealisticBulletProjectile : MonoBehaviour, IProjectile {
    Vector3 lastPosition;
    RaycastHit hit;

    VRWeapons.Weapon.Attack attack;
    
    LayerMask shotMask;

    private void Update()
    {
        if (Physics.Linecast(lastPosition, transform.position, out hit, shotMask))
        {
            if (hit.transform != transform)
            {
                ExecuteEvents.Execute<VRWeapons.IAttackReceiver>(hit.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(attack.originWeapon.NewAttack(attack.damage, transform.position, hit))));

                if (attack.originWeapon.impactProfile != null)
                {
                    ImpactInfo impact = attack.originWeapon.impactProfile.GetImpactInfo(hit);
                    GameObject cloneImpact = Instantiate(impact.GetRandomPrefab(), hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                    cloneImpact.transform.parent = hit.transform;
                }
                Debug.Log(hit.transform);
                Destroy(this.gameObject);
            }
        }

        lastPosition = transform.position;
    }

    public void SetParams(VRWeapons.Weapon.Attack newAttack, LayerMask newMask)
    {
        lastPosition = transform.position;
        attack = newAttack;
        shotMask = newMask;
    }
}
