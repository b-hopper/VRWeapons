using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

public class AttackReceiver : MonoBehaviour, IAttackReceiver {
	public UnityEvent OnAttackReceived;

	public void ReceiveAttack (Weapon.Attack attack)
	{
		OnAttackReceived.Invoke();
	}
}
