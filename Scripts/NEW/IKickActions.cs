using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

namespace VRWeapons
{
    public interface IKickActions
    {
        void Kick(Transform weap, Rigidbody weapRB);
    }
}