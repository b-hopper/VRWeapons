using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

public class AttackReceiver : MonoBehaviour, IAttackReceiver {
	public UnityEvent OnAttackReceived;

	public void ReceiveAttack (VRTK.Weapon.Attack attack)
	{
		OnAttackReceived.Invoke();
	}
}
