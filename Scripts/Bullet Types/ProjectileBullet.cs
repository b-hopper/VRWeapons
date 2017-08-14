using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VRWeapons.BulletTypes
{
    [System.Serializable]
    public class ProjectileBullet : MonoBehaviour, IBulletBehavior
    {
        [SerializeField]
        public GameObject projectile;

        [SerializeField]
        float shotForce;

        Rigidbody projRB;

        Projectile.IProjectile projActual;

        private void Start()
        {
            projRB = projectile.GetComponent<Rigidbody>();

            projActual = projectile.GetComponent<Projectile.IProjectile>();

            if (projRB == null)
            {
                Debug.LogError("Projectiles require a rigidbody to function correctly. Please add a rigidbody.", projectile);
            }
            projectile.SetActive(false);
        }

        public void DoBulletBehavior(Transform muzzleDir, float damage, float range, float bulletSpreadRange, Weapon thisWeapon, LayerMask shotMask)
        {
            Weapon.Attack attack = new Weapon.Attack();
            attack.damage = damage;
            attack.origin = transform.position;
            attack.originWeapon = thisWeapon;

            projActual.SetParams(attack, shotMask);
                
            projectile.SetActive(true);
            projectile.transform.position = muzzleDir.position;
            projectile.transform.parent = null;
            projRB.AddForce(muzzleDir.forward * shotForce, ForceMode.Impulse);
            gameObject.SetActive(false);
        }
    }
}