using System;
using System.Collections.Generic;
using UnityEngine;

namespace TMPro;

public struct MaterialReference
{
	public int index;

	public TMP_FontAsset fontAsset;

	public TMP_SpriteAsset spriteAsset;

	public Material material;

	public bool isDefaultMaterial;

	public bool isFallbackMaterial;

	public Material fallbackMaterial;

	public float padding;

	public int referenceCount;

	public MaterialReference(int index, TMP_FontAsset fontAsset, TMP_SpriteAsset spriteAsset, Material material, float padding)
	{
		this.index = index;
		this.fontAsset = fontAsset;
		this.spriteAsset = spriteAsset;
		this.material = material;
		isDefaultMaterial = ((material.GetInstanceID() == fontAsset.material.GetInstanceID()) ? true : false);
		isFallbackMaterial = false;
		fallbackMaterial = null;
		this.padding = padding;
		referenceCount = 0;
	}

	public static bool Contains(MaterialReference[] materialReferences, TMP_FontAsset fontAsset)
	{
		int instanceID = fontAsset.GetInstanceID();
		for (int i = 0; i < materialReferences.Length && materialReferences[i].fontAsset != null; i++)
		{
			if (materialReferences[i].fontAsset.GetInstanceID() == instanceID)
			{
				return true;
			}
		}
		return false;
	}

	public static int AddMaterialReference(Material material, TMP_FontAsset fontAsset, ref MaterialReference[] materialReferences, Dictionary<int, int> materialReferenceIndexLookup)
	{
		int instanceID = material.GetInstanceID();
		if (materialReferenceIndexLookup.TryGetValue(instanceID, out var value))
		{
			return value;
		}
		value = (materialReferenceIndexLookup[instanceID] = materialReferenceIndexLookup.Count);
		if (value >= materialReferences.Length)
		{
			Array.Resize(ref materialReferences, Mathf.NextPowerOfTwo(value + 1));
		}
		materialReferences[value].index = value;
		materialReferences[value].fontAsset = fontAsset;
		materialReferences[value].spriteAsset = null;
		materialReferences[value].material = material;
		materialReferences[value].isDefaultMaterial = ((instanceID == fontAsset.material.GetInstanceID()) ? true : false);
		materialReferences[value].referenceCount = 0;
		return value;
	}

	public static int AddMaterialReference(Material material, TMP_SpriteAsset spriteAsset, ref MaterialReference[] materialReferences, Dictionary<int, int> materialReferenceIndexLookup)
	{
		int instanceID = material.GetInstanceID();
		if (materialReferenceIndexLookup.TryGetValue(instanceID, out var value))
		{
			return value;
		}
		value = (materialReferenceIndexLookup[instanceID] = materialReferenceIndexLookup.Count);
		if (value >= materialReferences.Length)
		{
			Array.Resize(ref materialReferences, Mathf.NextPowerOfTwo(value + 1));
		}
		materialReferences[value].index = value;
		materialReferences[value].fontAsset = materialReferences[0].fontAsset;
		materialReferences[value].spriteAsset = spriteAsset;
		materialReferences[value].material = material;
		materialReferences[value].isDefaultMaterial = true;
		materialReferences[value].referenceCount = 0;
		return value;
	}
}
