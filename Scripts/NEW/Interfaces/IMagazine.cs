using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

namespace VRWeapons
{
    public interface IMagazine
    {
        IBulletBehavior FeedRound();
        void MagIn(Weapon weap);
        void MagOut(Weapon weap);
        bool PushBullet(IBulletBehavior newRound);
        bool PopBullet();
        Rigidbody GetRoundRigidBody();
        Transform GetRoundTransform();
    }
}