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
        void SetEjector(IEjectorActions newEjector);
        void SetBolt(IBoltActions newBolt);
        void SetKick(IKickActions newKick);
    }
}