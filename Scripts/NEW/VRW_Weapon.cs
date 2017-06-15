
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


#region Interfaces
public interface IVRW_MuzzleActions
{
    void StartFiring();
    void StopFiring();
    void SetEjector(IVRW_EjectorActions newEjector);
    void SetBolt(IVRW_BoltActions newBolt);
    void SetKick(IVRW_KickActions newKick);
}

public interface IVRW_EjectorActions
{
    void Eject();
}

public interface IVRW_KickActions
{
    void Kick(Transform weap, Rigidbody weapRB);
}

public interface IVRW_BoltActions
{
    void BoltBack();
}

public interface IVRW_ObjectPool
{
    GameObject GetNewObj();
}
#endregion

[RequireComponent(typeof(AudioSource))]

public class VRW_Weapon : VRTK_InteractableObject
{
    IVRW_MuzzleActions Muzzle;
    IVRW_EjectorActions Ejector;
    IVRW_KickActions Kick;
    IVRW_BoltActions Bolt;
    IVRW_ObjectPool shellPool;
    IVRW_ObjectPool flashPool;
    public bool infiniteAmmo, autoRackForward;
    bool IsChambered { set; get; }

    [System.Serializable]
    public struct Attack
    {
        public float damage;
        public Vector3 origin;
        public RaycastHit hitInfo;
    }

    private void Start()
    {
        Muzzle = GetComponentInChildren<IVRW_MuzzleActions>();  // TEMPORARY, to be assigned in-editor later
        Bolt = GetComponentInChildren<IVRW_BoltActions>();
        IsChambered = true;
        Muzzle.SetBolt(Bolt);

    }

    public override void StartUsing(GameObject usingObject)
    {
        if (IsChambered)
        {
            Muzzle.StartFiring();
        }
    }

    public override void StopUsing(GameObject previousUsingObject)
    {
        Muzzle.StopFiring();
    }

    void PlaySound(int clip) { }


    public void SetChambered(bool val) { IsChambered = val; }

    public Attack NewAttack(float newDamage, Vector3 newOrigin, RaycastHit newHit)
    {
        return new Attack
        {
            damage = newDamage,
            origin = newOrigin,
            hitInfo = newHit
        };
    }

    #region Editor-Friendly functions
    public void SetMuzzle(IVRW_MuzzleActions newMuzzle)
    {
        Muzzle = newMuzzle;
    }

    public void SetEjector(IVRW_EjectorActions newEjector)
    {
        Ejector = newEjector;
    }

    #endregion
}
