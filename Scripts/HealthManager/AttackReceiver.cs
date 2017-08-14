using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;
using VRWeapons;

public class AttackReceiver : MonoBehaviour, IAttackReceiver {
	public UnityEvent OnAttackReceived;

	public void ReceiveAttack (VRWeapons.Weapon.Attack attack)
	{
		OnAttackReceived.Invoke();
	}
}
