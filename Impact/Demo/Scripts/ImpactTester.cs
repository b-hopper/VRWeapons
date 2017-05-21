using UnityEngine;

public class ImpactTester : MonoBehaviour
{
	public ImpactProfile impactProfile;

	void Update ()
	{
		if (Input.GetButtonDown ("Fire1")) {
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)) {
				var impactInfo = impactProfile.GetImpactInfo (hit);
				var prefab = impactInfo.GetRandomPrefab ();

				Instantiate (prefab, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
			}
		}
	}
}
