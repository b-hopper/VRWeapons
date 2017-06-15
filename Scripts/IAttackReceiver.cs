using UnityEngine.EventSystems;

namespace Valve.VR.InteractionSystem
{

    public interface IAttackReceiver : IEventSystemHandler
    {
        void ReceiveAttack(VRW_Weapon.Attack attack);
    }
}
