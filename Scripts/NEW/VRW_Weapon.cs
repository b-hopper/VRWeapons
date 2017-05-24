
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTK
{

    public interface IMuzzleActions
    {
        void StartFiring();
        void StopFiring();
    }

    public interface IEjectorActions
    {
        void Eject();
    }

    public class VRW_Weapon : VRTK_InteractableObject
    {
        IMuzzleActions muzzle;
        IEjectorActions ejector;

        new void Awake()
        {
            muzzle = GetComponentInChildren<IMuzzleActions>();
            Debug.Log(muzzle);
        }

        public override void StartUsing(GameObject usingObject)
        {
            muzzle.StartFiring();
        }

        public override void StopUsing(GameObject previousUsingObject)
        {
            muzzle.StopFiring();
        }

        #region Editor-Friendly functions
        public void SetMuzzle(IMuzzleActions newMuzzle)
        {
            muzzle = newMuzzle;
        }

        public void SetEjector(IEjectorActions newEjector)
        {
            ejector = newEjector;
        }

        #endregion
    }
}