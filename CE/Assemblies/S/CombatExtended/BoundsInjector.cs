using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public static class BoundsInjector
{
	public enum GraphicType
	{
		Pawn,
		Plant
	}

	private static Dictionary<string, Vector2> boundMap = new Dictionary<string, Vector2>();

	public static Vector2 BoundMap(Graphic graphic, GraphicType type, Graphic headGraphic, Vector2 headOffset)
	{
		string key = graphic.path + ((headGraphic == null) ? "" : ("+" + headGraphic.path));
		if (!boundMap.ContainsKey(key))
		{
			try
			{
				boundMap[key] = ExtractBounds(graphic, type, headGraphic, headOffset);
			}
			catch (Exception innerException)
			{
				throw new Exception("BoundMap(,,,)", innerException);
			}
		}
		return boundMap[key];
	}

	public static Vector2 BoundMap(Graphic graphic, GraphicType type)
	{
		if (boundMap.TryGetValue(graphic.path, out var value))
		{
			return value;
		}
		try
		{
			Vector2 vector = ExtractBounds(graphic, type);
			boundMap[graphic.path] = vector;
			return vector;
		}
		catch (Exception innerException)
		{
			throw new Exception("BoundMap(,)", innerException);
		}
	}

	private static Vector2 ExtractBounds(Graphic graphic, GraphicType type, Graphic headGraphic, Vector2 headOffset)
	{
		IntRange intRange;
		int height;
		try
		{
			intRange = Def_Extensions.CropVertical((graphic.MatEast.mainTexture as Texture2D).GetColorSafe(out var width, out height), width, height);
		}
		catch (Exception innerException)
		{
			throw new Exception("Combat Extended :: CropVertical error while cropping Textures/" + graphic.path + "_side", innerException);
		}
		IntRange intRange2;
		int width2;
		try
		{
			intRange2 = Def_Extensions.CropHorizontal((graphic.MatSouth.mainTexture as Texture2D).GetColorSafe(out width2, out var height2), width2, height2);
		}
		catch (Exception innerException2)
		{
			throw new Exception("Combat Extended :: CropHorizontal error while cropping Textures/" + graphic.path + "_front", innerException2);
		}
		IntRange intRange3;
		int height3;
		try
		{
			intRange3 = Def_Extensions.CropVertical((headGraphic.MatEast.mainTexture as Texture2D).GetColorSafe(out var width3, out height3), width3, height3);
		}
		catch (Exception innerException3)
		{
			throw new Exception("Combat Extended :: CropVertical error while cropping Textures/" + headGraphic.path + "_side", innerException3);
		}
		intRange3.min -= (int)(headOffset.y * (float)height3);
		intRange3.max -= (int)(headOffset.y * (float)height3);
		intRange.min = Math.Min(intRange.min, (int)((float)intRange3.min * (float)height / (float)height3));
		intRange.max = Math.Max(intRange.max, (int)((float)intRange3.max * (float)height / (float)height3));
		IntRange intRange4;
		int width4;
		try
		{
			intRange4 = Def_Extensions.CropHorizontal((headGraphic.MatSouth.mainTexture as Texture2D).GetColorSafe(out width4, out var height4), width4, height4);
		}
		catch (Exception innerException4)
		{
			throw new Exception("Combat Extended :: CropHorizontal error while cropping Textures/" + headGraphic.path + "_front", innerException4);
		}
		intRange4.min += (int)(headOffset.x * (float)width4);
		intRange4.max += (int)(headOffset.x * (float)width4);
		intRange2.max = Math.Max(intRange2.max, (int)((float)intRange4.max * (float)width2 / (float)width4));
		intRange2.min = Math.Min(intRange2.min, (int)((float)intRange4.min * (float)width2 / (float)width4));
		return new Vector2((float)(intRange2.max - intRange2.min) / (float)width2, (float)(intRange.max - intRange.min) / (float)height);
	}

	private static Vector2 ExctractBoundCollection(Graphic_Collection graphic, GraphicType type)
	{
		IEnumerable<Vector2> source = graphic.subGraphics.Select((Graphic x) => ExtractBounds(x, type));
		return new Vector2(source.Average((Vector2 v) => v.x), source.Average((Vector2 v) => v.y));
	}

	private static Vector2 ExtractBounds(Graphic graphic, GraphicType type)
	{
		if (graphic is Graphic_Collection graphic2)
		{
			return ExctractBoundCollection(graphic2, type);
		}
		IntRange intRange;
		int height;
		try
		{
			intRange = Def_Extensions.CropVertical((graphic.MatEast.mainTexture as Texture2D).GetColorSafe(out var width, out height), width, height);
		}
		catch (Exception innerException)
		{
			throw new Exception("Combat Extended :: CropVertical error while cropping Textures/" + graphic.path + "_side", innerException);
		}
		if (type == GraphicType.Plant)
		{
			return new Vector2(1f, (float)(intRange.max - intRange.min) / (float)height);
		}
		IntRange intRange2;
		int width2;
		try
		{
			intRange2 = Def_Extensions.CropHorizontal((graphic.MatSouth.mainTexture as Texture2D).GetColorSafe(out width2, out var height2), width2, height2);
		}
		catch (Exception innerException2)
		{
			throw new Exception("Combat Extended :: CropHorizontal error while cropping Textures/" + graphic.path + "_front", innerException2);
		}
		return new Vector2((float)(intRange2.max - intRange2.min) / (float)width2, (float)(intRange.max - intRange.min) / (float)height);
	}

	public static void Inject()
	{
		foreach (PawnKindDef item in DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef x) => !x.RaceProps.Humanlike))
		{
			for (int i = 0; i < item.lifeStages.Count; i++)
			{
				PawnKindLifeStage pawnKindLifeStage = item.lifeStages[i];
				try
				{
					if (pawnKindLifeStage.bodyGraphicData != null && pawnKindLifeStage.bodyGraphicData.Graphic != null)
					{
						BoundMap(pawnKindLifeStage.bodyGraphicData.Graphic, GraphicType.Pawn);
					}
				}
				catch (Exception innerException)
				{
					throw new Exception(item?.ToString() + ".lifeStages[" + i + "].bodyGraphicData", innerException);
				}
				try
				{
					if (pawnKindLifeStage.femaleGraphicData != null && pawnKindLifeStage.femaleGraphicData.Graphic != null)
					{
						BoundMap(pawnKindLifeStage.femaleGraphicData.Graphic, GraphicType.Pawn);
					}
				}
				catch (Exception innerException2)
				{
					throw new Exception(item?.ToString() + ".lifeStages[" + i + "].femaleGraphicData", innerException2);
				}
				try
				{
					if (pawnKindLifeStage.dessicatedBodyGraphicData != null && pawnKindLifeStage.dessicatedBodyGraphicData.Graphic != null)
					{
						BoundMap(pawnKindLifeStage.dessicatedBodyGraphicData.Graphic, GraphicType.Pawn);
					}
				}
				catch (Exception innerException3)
				{
					throw new Exception(item?.ToString() + ".lifeStages[" + i + "].dessicatedBodyGraphicData", innerException3);
				}
				try
				{
					if (pawnKindLifeStage.femaleDessicatedBodyGraphicData != null && pawnKindLifeStage.femaleDessicatedBodyGraphicData.Graphic != null)
					{
						BoundMap(pawnKindLifeStage.femaleDessicatedBodyGraphicData.Graphic, GraphicType.Pawn);
					}
				}
				catch (Exception innerException4)
				{
					throw new Exception(item?.ToString() + ".lifeStages[" + i + "].femaleDessicatedBodyGraphicData", innerException4);
				}
			}
		}
		foreach (ThingDef item2 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.plant != null))
		{
			try
			{
				if (item2.graphicData != null && item2.graphicData.Graphic != null)
				{
					BoundMap(item2.graphicData.Graphic, GraphicType.Plant);
				}
			}
			catch (Exception innerException5)
			{
				throw new Exception(item2?.ToString() + ".graphicData", innerException5);
			}
			try
			{
				if (item2.plant.leaflessGraphic != null)
				{
					BoundMap(item2.plant.leaflessGraphic, GraphicType.Plant);
				}
			}
			catch (Exception innerException6)
			{
				throw new Exception(item2?.ToString() + ".plant.leaflessGraphic", innerException6);
			}
			try
			{
				if (item2.plant.immatureGraphic != null)
				{
					BoundMap(item2.plant.immatureGraphic, GraphicType.Plant);
				}
			}
			catch (Exception innerException7)
			{
				throw new Exception(item2?.ToString() + ".plant.immatureGraphic", innerException7);
			}
		}
		Graphic graphicSowing = Plant.GraphicSowing;
		try
		{
			if (graphicSowing != null)
			{
				BoundMap(graphicSowing, GraphicType.Plant);
			}
		}
		catch (Exception innerException8)
		{
			throw new Exception("GraphicSowing", innerException8);
		}
		Log.Message("Combat Extended :: Bounds pre-generated");
	}

	public static Vector2 ForPawn(Pawn pawn)
	{
		if (pawn.RaceProps.Humanlike)
		{
			return new Vector2(0.5f, 1f);
		}
		PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
		GraphicData graphicData = ((!pawn.IsDessicated() || curKindLifeStage.dessicatedBodyGraphicData == null) ? ((pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null) ? curKindLifeStage.bodyGraphicData : curKindLifeStage.femaleGraphicData) : ((pawn.gender != Gender.Female || curKindLifeStage.femaleDessicatedBodyGraphicData == null) ? curKindLifeStage.dessicatedBodyGraphicData : curKindLifeStage.femaleDessicatedBodyGraphicData));
		string text = ((!pawn.IsDessicated() || curKindLifeStage.dessicatedBodyGraphicData == null) ? ((pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null) ? "bodyGraphicData" : "femaleGraphicData") : ((pawn.gender != Gender.Female || curKindLifeStage.femaleDessicatedBodyGraphicData == null) ? "dessicatedBodyGraphicData" : "femaleDessicatedBodyGraphicData"));
		Graphic graphic = graphicData.Graphic;
		Vector2 drawSize = graphicData.drawSize;
		if (!pawn.kindDef.alternateGraphics.NullOrEmpty())
		{
		}
		if (graphic == null)
		{
			Log.Error(pawn?.ToString() + ".lifeStage[" + pawn.ageTracker.CurLifeStageIndex + "]." + text + " could not be found");
			return Vector2.zero;
		}
		try
		{
			return Vector2.Scale(BoundMap(graphic, GraphicType.Pawn), drawSize);
		}
		catch (ArgumentException innerException)
		{
			throw new ArgumentException(pawn?.ToString() + ".lifeStage[" + pawn.ageTracker.CurLifeStageIndex + "]." + text, innerException);
		}
	}

	public static Vector2 ForPlant(Plant plant)
	{
		return plant.def.plant.visualSizeRange.LerpThroughRange(plant.Growth) * BoundMap(plant.Graphic, GraphicType.Plant);
	}
}
