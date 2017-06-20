using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

public class Magazine : MonoBehaviour, IMagazine {
    
    public List<IBulletBehavior> RoundsInMag;
    int index = 0;

    private void Start()
    {
        RoundsInMag = new List<IBulletBehavior>();
        RoundsInMag.Add(new RaycastBullet());
    }

    public IBulletBehavior FeedRound()
    {
        return RoundsInMag[index];
    }

    public void MagIn(Weapon weap)
    {
        weap.Magazine = this;
    }

    public void MagOut(Weapon weap)
    {
        weap.Magazine = null;
    }

}
