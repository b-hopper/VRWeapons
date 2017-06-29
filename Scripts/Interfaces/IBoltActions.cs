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
        void OnTriggerPullActions(float angle);
        void SetEjector(IEjectorActions newEjector);

        void IsCurrentlyBeingManipulated(bool val);

        void ReplaceRoundWithEmptyShell(GameObject go);

        Vector3 GetMinValue();
        Vector3 GetMaxValue();

        float boltLerpVal { set; get; }
    }
}