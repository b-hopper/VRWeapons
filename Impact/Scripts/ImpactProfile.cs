using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class ImpactProfile : ScriptableObject
{
	Dictionary<Material, ImpactInfo> materialLookup;
	Dictionary<Texture, ImpactInfo> textureLookup;

	[SerializeField]
	public ImpactInfo defaultImpact;

	[SerializeField]
	public ImpactInfo[] impacts = new ImpactInfo[0];

	void OnEnable ()
	{
		materialLookup = new Dictionary<Material, ImpactInfo> ();
		textureLookup = new Dictionary<Texture, ImpactInfo> ();

		for (int i = 0; i < impacts.Length; i++) {
			var impact = impacts [i];
			if (impact.ImpactType == ImpactInfo.Type.Material) {
				materialLookup [impact.Material] = impact;
			} else if (impact.ImpactType == ImpactInfo.Type.Terrain) {
				textureLookup [impact.Texture] = impact;
			}
		}
	}

	public ImpactInfo GetImpactInfo (Material material)
	{
		ImpactInfo impact;
		if (materialLookup.TryGetValue (material, out impact)) {
			return impact;
		}

		return defaultImpact;
	}

	public ImpactInfo GetImpactInfo (RaycastHit hitInfo)
	{
		var collider = hitInfo.transform.GetComponent<Collider> ();
        var ImpProfileOverride = hitInfo.transform.GetComponent<ImpactProfileOverride>();
		if (collider is TerrainCollider) {
			var terrain = collider.GetComponent<Terrain> ();
			if (terrain != null) {
				var tex = GetTerrainTextureAt (terrain, hitInfo.point);
				ImpactInfo impact;
				if (textureLookup.TryGetValue (tex, out impact)) {
					return textureLookup [tex];
				}
			}
			return defaultImpact;
		}

        if (ImpProfileOverride != null)
        {
            return GetImpactInfo(ImpProfileOverride.materialOverride);
        }

		var renderer = hitInfo.transform.GetComponent<Renderer> ();
		if (renderer == null || renderer.sharedMaterial == null) {
			return defaultImpact;
		}

		return GetImpactInfo (renderer.sharedMaterial);
	}

	Texture GetTerrainTextureAt (Terrain terrain, Vector3 position)
	{
		TerrainData terrainData = terrain.terrainData;
		Texture tex = null;
		Vector3 terrainSize; // terrain size
		Vector2 alphamapSize; // control texture size

		terrainSize = terrainData.size;
		alphamapSize.x = terrainData.alphamapWidth;
		alphamapSize.y = terrainData.alphamapHeight;

		int aX = (int)((position.x / terrainSize.x) * alphamapSize.x + 0.5f);
		int aY = (int)((position.z / terrainSize.z) * alphamapSize.y + 0.5f);

		float[,,] alphamap = terrainData.GetAlphamaps (aX, aY, 1, 1);

		var splats = terrainData.splatPrototypes;
        float alphamax = Mathf.NegativeInfinity;
        int idx = -1;
		for (int i = 0; i < splats.Length; i++) {
            float val = alphamap[0, 0, i];
			if (val > alphamax) {
                alphamax = val;
                idx = i;
			}
		}

        if (idx != -1)
        {
            tex = splats[idx].texture;
        } else
        {
            Debug.LogError("Couldn't find a texture from terrain data for position " + position.ToString() + "!");
        }

		return tex;
	}
}
