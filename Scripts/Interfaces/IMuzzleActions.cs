using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

namespace VRWeapons
{
    public interface IMuzzleActions
    {
        void StartFiring(IBulletBehavior round);
        void StopFiring();
        void SetNewWeapon(Weapon newWeap);
    }
}