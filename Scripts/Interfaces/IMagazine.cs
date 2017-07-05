using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

namespace VRWeapons
{
    public interface IMagazine
    {
        event EventHandler MagDropped;

        IBulletBehavior FeedRound();
        void MagIn(Weapon weap);
        void MagOut(Weapon weap);
        bool PushBullet(GameObject newRound);
        bool PopBullet();
        Rigidbody GetRoundRigidBody();
        Transform GetRoundTransform();
        bool CanMagBeDetached { get; set; }
    }
}