using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

namespace VRWeapons
{
    public interface IBoltActions
    {
        IBulletBehavior ChamberNewRound();

        void BoltBack();
        void SetEjector(IEjectorActions newEjector);

        void IsCurrentlyBeingManipulated(bool val);

        Vector3 GetMinValue();
        Vector3 GetMaxValue();

        float GetLerpValue();
        void SetLerpValue(float val);
    }
}