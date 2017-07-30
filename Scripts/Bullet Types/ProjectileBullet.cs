using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VRWeapons.BulletTypes
{
    [System.Serializable]
    public class ProjectileBullet : MonoBehaviour, IBulletBehavior
    {
        public void DoBulletBehavior(Transform muzzleDir, float damage, float range, float bulletSpreadRange, Weapon thisWeapon, LayerMask shotMask)
        {
            Debug.Log("PROJECTILE SHOT TODO");
        }
    }
}