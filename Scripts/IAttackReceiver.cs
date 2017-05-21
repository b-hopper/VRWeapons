using UnityEngine.EventSystems;

namespace Valve.VR.InteractionSystem
{

    public interface IAttackReceiver : IEventSystemHandler
    {
        void ReceiveAttack(Weapon.Attack attack);
    }
}
