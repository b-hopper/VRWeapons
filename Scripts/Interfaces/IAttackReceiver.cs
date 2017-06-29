using UnityEngine.EventSystems;
using VRWeapons;

namespace VRWeapons
{

    public interface IAttackReceiver : IEventSystemHandler
    {
        void ReceiveAttack(VRWeapons.Weapon.Attack attack);
    }
}
