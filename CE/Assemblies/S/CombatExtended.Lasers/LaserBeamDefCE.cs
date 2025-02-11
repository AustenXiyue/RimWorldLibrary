using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CombatExtended.Lasers;

public class LaserBeamDefCE : AmmoDef
{
	public float capSize = 1f;

	public float capOverlap = 0.0171875f;

	public int lifetime = 30;

	public int flickerFrameTime = 5;

	public float impulse = 4f;

	public float beamWidth = 1f;

	public float shieldDamageMultiplier = 0.5f;

	public float seam = -1f;

	public float causefireChance = -1f;

	public bool canExplode = false;

	public bool LightningBeam = false;

	public float LightningVariance = 3f;

	public bool StaticLightning = true;

	public int ArcCount = 1;

	public List<LaserBeamDecoration> decorations;

	public EffecterDef explosionEffect;

	public EffecterDef hitLivingEffect;

	public ThingDef beamGraphic;

	public List<string> textures;

	public List<Material> materials = new List<Material>();

	public bool IsWeakToShields => shieldDamageMultiplier < 1f;

	private void CreateGraphics()
	{
		if (graphicData.graphicClass == typeof(Graphic_Random) || graphicData.graphicClass == typeof(Graphic_Flicker))
		{
			for (int i = 0; i < textures.Count; i++)
			{
				List<Texture2D> list = (from x in ContentFinder<Texture2D>.GetAllInFolder(textures[i])
					where !x.name.EndsWith(Graphic_Single.MaskSuffix)
					orderby x.name
					select x).ToList();
				if (list.NullOrEmpty())
				{
					Log.Error("Collection cannot init: No textures found at path " + textures[i]);
				}
				for (int j = 0; j < list.Count; j++)
				{
					Material material = MaterialPool.MatFrom(textures[i] + "/" + list[j].name, ShaderDatabase.TransparentPostLight);
					material.color = graphicData.color;
				}
			}
		}
		else
		{
			for (int k = 0; k < textures.Count; k++)
			{
				Material material2 = MaterialPool.MatFrom(textures[k], ShaderDatabase.TransparentPostLight);
				material2.color = graphicData.color;
				materials.Add(material2);
			}
		}
	}

	public Material GetBeamMaterial(int index)
	{
		if (materials.Count == 0 && textures.Count != 0)
		{
			CreateGraphics();
		}
		if (materials.Count == 0)
		{
			return null;
		}
		if (index >= materials.Count || index < 0)
		{
			index = 0;
		}
		materials[index].color = graphicData.color;
		return materials[index];
	}
}
