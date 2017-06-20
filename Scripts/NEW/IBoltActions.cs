using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

namespace VRWeapons
{
    public interface IBoltActions
    {
        void BoltBack();
        IBulletBehavior ChamberNewRound();
    }
}