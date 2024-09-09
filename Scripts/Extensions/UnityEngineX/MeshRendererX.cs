using System;
using UnityEngine;

public static class MeshRendererX {

	public static bool SharedMaterialsContains (this MeshRenderer meshRenderer, Material material) {
		int materialIndex = meshRenderer.sharedMaterials.IndexOf(material);
		return materialIndex != -1;
	}

	/// <summary>
	/// Adds to the shared materials of a meshRenderer.
	/// </summary>
	/// <param name="meshRenderer">Mesh renderer.</param>
	/// <param name="material">Material.</param>
	public static void AddToSharedMaterials (this MeshRenderer meshRenderer, Material material) {
		Material[] materials = new Material[meshRenderer.sharedMaterials.Length + 1];
		Array.Copy(meshRenderer.sharedMaterials, materials, meshRenderer.sharedMaterials.Length);
		materials[materials.Length-1] = material;
		meshRenderer.sharedMaterials = materials;
	}

	/// <summary>
	/// Removes from the shared materials of a meshRenderer.
	/// </summary>
	/// <returns><c>true</c>, if from shared materials was removed, <c>false</c> otherwise.</returns>
	/// <param name="meshRenderer">Mesh renderer.</param>
	/// <param name="material">Material.</param>
	public static bool RemoveFromSharedMaterials (this MeshRenderer meshRenderer, Material material) {
		int materialIndex = meshRenderer.sharedMaterials.IndexOf(material);
		if(materialIndex == -1) 
			return false;
		Material[] materials = new Material[meshRenderer.sharedMaterials.Length - 1];
		int currentIndex = 0;
		for(int i = 0; i < meshRenderer.sharedMaterials.Length; i++) {
			if(i == materialIndex) 
				continue;
			materials[currentIndex] = meshRenderer.sharedMaterials[i];
			currentIndex++;
		}
		meshRenderer.sharedMaterials = materials;
		return true;
	}
}
