using UnityEngine.EventSystems;

namespace Valve.VR.InteractionSystem
{

    public interface IAttackReceiver : IEventSystemHandler
    {
        void ReceiveAttack(VRTK.Weapon.Attack attack);
    }
}
