using UnityEngine;

[System.Serializable]
public struct ImpactInfo
{
	public enum Type
	{
		Material,
		Terrain
	}

	[SerializeField]
	Type impactType;

	[SerializeField]
	Material material;
	[SerializeField]
	Texture texture;
	[SerializeField]
	GameObject[] impactPrefabs;

	public Type ImpactType {
		get {
			return impactType;
		}
	}

	public Material Material {
		get {
			return material;
		}
	}

	public Texture Texture {
		get {
			return texture;
		}
	}
    
	public GameObject GetRandomPrefab ()
	{
		int length = impactPrefabs.Length;

		if (length == 0) {
			Debug.LogErrorFormat ("Please assign at least one impact prefab for material '{0}'", material.name);
			return null;
		} else if (length == 1) {
			return impactPrefabs [0];
		}

		return impactPrefabs [Random.Range (0, length)];
	}
}
