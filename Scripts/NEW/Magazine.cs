using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

public class Magazine : MonoBehaviour, IMagazine {
    
    public List<IBulletBehavior> RoundsInMag;
    int index;
    [SerializeField]
    int maxRounds, countIndex;

    private void Start()
    {
        RoundsInMag = new List<IBulletBehavior>(maxRounds);
        index = 0;
    }

    public bool PushBullet(IBulletBehavior newRound)
    {
        bool val = false;
        if (RoundsInMag.Count < maxRounds)
        {
            Debug.Log(index);
            RoundsInMag.Insert(index, newRound);
            val = true;
            index++;
        }
        ReportRoundsInMag();
        return val;
    }

    public bool PopBullet()
    {
        bool val = false;
        if (RoundsInMag.Remove(RoundsInMag[index - 1]))
        {
            val = true;
            index--;
        }
        return val;
    }

    public IBulletBehavior FeedRound()
    {
        IBulletBehavior tmp = null;
        if (index > 0)
        {
            tmp = RoundsInMag[index - 1];
            index--;
        }
        return tmp;
    }

    public void MagIn(Weapon weap)
    {
        weap.Magazine = this;
    }

    public void MagOut(Weapon weap)
    {
        weap.Magazine = null;
    }

    void ReportRoundsInMag()
    {
        countIndex++;
        foreach(IBulletBehavior tmp in RoundsInMag)
        {
            Debug.Log("Count " + countIndex + ": " + tmp);
        }
    }

}
