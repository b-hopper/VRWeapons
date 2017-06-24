using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

namespace VRWeapons
{
    public interface IEjectorActions
    {
        void Eject(Transform t, Rigidbody rb);
    }
}