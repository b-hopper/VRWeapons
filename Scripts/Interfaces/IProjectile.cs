using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons.BulletTypes.Projectile
{
    public interface IProjectile
    {
        void SetParams(Weapon.Attack attack, LayerMask newMask);        
    }
}