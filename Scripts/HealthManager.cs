using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

public class HealthManager : MonoBehaviour, IAttackReceiver {

	[SerializeField]
	float maxHealth = 100f;
	[SerializeField]
	float currentHealth;
	[SerializeField]
	bool destroyOnDepleted = false;

	public UnityEvent OnDamaged;
	public UnityEvent OnHealthDepleted;

	public float MaxHealth {
		get {
			return maxHealth;
		}
	}

	public float CurrentHealth {
		get {
			return currentHealth;
		}
	}

	void OnEnable () {
		Reset();
	}

	public void ReceiveAttack (VRW_Weapon.Attack attack)
	{
        currentHealth -= attack.damage;
		OnDamaged.Invoke();

		if (currentHealth <= 0) {
			OnHealthDepleted.Invoke();
			if (destroyOnDepleted) {
				Destroy(gameObject);
			}
		}
	}

	void Reset () {
		currentHealth = maxHealth;
	}
}
