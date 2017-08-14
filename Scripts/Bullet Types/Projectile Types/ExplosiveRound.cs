using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VRWeapons.BulletTypes.Projectile;

public class ExplosiveRound : MonoBehaviour, IProjectile
{
    RaycastHit hit;

    VRWeapons.Weapon.Attack attack;

    LayerMask shotMask;

    [SerializeField]
    float armingDistance, explosionRadius;

    [SerializeField]
    GameObject explosion;

    private void Start()
    {
        if (explosion.transform.parent == null)
        {
            explosion = Instantiate(explosion);
        }
        explosion.transform.parent = transform;
        explosion.transform.localEulerAngles = Vector3.zero;
        explosion.SetActive(false);
        Debug.Break();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(Vector3.Distance(attack.origin, transform.position));
        if (Vector3.Distance(attack.origin, transform.position) > armingDistance)
        {
            Debug.Log("Explosion triggered");
            if (explosion != null)
            {
                explosion.SetActive(true);
                explosion.transform.parent = null;
            }
            if (Physics.SphereCast(transform.position, explosionRadius, transform.forward, out hit, explosionRadius, shotMask))
            {
                Debug.Log("Explosion hit: " + hit.collider.gameObject);
                ExecuteEvents.Execute<VRWeapons.IAttackReceiver>(hit.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(attack.originWeapon.NewAttack(attack.damage, transform.position, hit))));
            }
            ExecuteEvents.Execute<VRWeapons.IAttackReceiver>(collision.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(attack.originWeapon.NewAttack(attack.damage, transform.position, hit))));

            gameObject.SetActive(false);
        }
    }
    
    public void SetParams(VRWeapons.Weapon.Attack newAttack, LayerMask newMask)
    {
        attack.originWeapon = newAttack.originWeapon;
        attack.origin = newAttack.origin;
        attack.damage = newAttack.damage;
        shotMask = newMask;
    }
}
