using UnityEngine;

public class AutoDestroyParticleSystem : MonoBehaviour
{
	public ParticleSystem particleSystem;

	void Update ()
	{
		if (!particleSystem.IsAlive ()) {
			Destroy (gameObject);
		}
	}
}
