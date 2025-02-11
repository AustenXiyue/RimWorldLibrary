using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CombatExtended;

public static class Def_Extensions
{
	private static Dictionary<string, Texture2D> _cachedDefIcons = new Dictionary<string, Texture2D>();

	private const float alphaThreshold = 0.25f;

	public static Texture2D IconTexture(this Def def)
	{
		if (_cachedDefIcons.ContainsKey(def.defName))
		{
			return _cachedDefIcons[def.defName];
		}
		BuildableDef buildableDef = def as BuildableDef;
		ThingDef thingDef = def as ThingDef;
		PawnKindDef pawnKindDef = def as PawnKindDef;
		if (def is RecipeDef recipeDef && !recipeDef.products.NullOrEmpty())
		{
			_cachedDefIcons[def.defName] = recipeDef.products.First().thingDef.IconTexture();
			return _cachedDefIcons[def.defName];
		}
		if (pawnKindDef != null)
		{
			try
			{
				_cachedDefIcons[def.defName] = (pawnKindDef.lifeStages.Last().bodyGraphicData.Graphic.MatSouth.mainTexture as Texture2D).Crop();
				return _cachedDefIcons[def.defName];
			}
			catch (Exception ex)
			{
				Log.Error("Combat Extended :: IconTexture(" + def.ToString() + ") - pawnKindDef check - resulted in the following error [defaulting to non-cropped texture]: " + ex.ToString());
				_cachedDefIcons[def.defName] = pawnKindDef.lifeStages.Last().bodyGraphicData.Graphic.MatSouth.mainTexture as Texture2D;
				return _cachedDefIcons[def.defName];
			}
		}
		if (buildableDef == null)
		{
			return null;
		}
		if (thingDef != null && thingDef.entityDefToBuild != null)
		{
			try
			{
				_cachedDefIcons[def.defName] = thingDef.entityDefToBuild.IconTexture().Crop();
				return _cachedDefIcons[def.defName];
			}
			catch (Exception ex2)
			{
				Log.Error("Combat Extended :: IconTexture(" + def.ToString() + ") - entityDefToBuild check - resulted in the following error [defaulting to non-cropped texture]: " + ex2.ToString());
				_cachedDefIcons[def.defName] = thingDef.entityDefToBuild.IconTexture();
				return _cachedDefIcons[def.defName];
			}
		}
		_cachedDefIcons[def.defName] = buildableDef.uiIcon;
		return _cachedDefIcons[def.defName];
	}

	public static IntRange CropVertical(Color[] array, int width, int height)
	{
		IntRange result = new IntRange(0, height - 1);
		int num = 0;
		while (array[num].a < 0.25f)
		{
			num++;
			if (num % width == 0)
			{
				result.max--;
				if (num > width * height - 1)
				{
					throw new ArgumentException("Color[] has no pixels with alpha < " + 0.25f);
				}
			}
		}
		num = array.Length - 1;
		while (array[num].a < 0.25f)
		{
			if (num % width == 0)
			{
				result.min++;
			}
			num--;
		}
		return result;
	}

	public static IntRange CropHorizontal(Color[] array, int width, int height)
	{
		IntRange result = new IntRange(0, width - 1);
		int num = 0;
		while (array[num].a < 0.25f)
		{
			num += width;
			if (num > width * height - 1)
			{
				result.min++;
				num = result.min;
				if (num > width - 1)
				{
					throw new ArgumentException("Color[] has no pixels with alpha >= " + 0.25f);
				}
			}
		}
		num = array.Length - 1;
		while (array[num].a < 0.25f)
		{
			num -= width;
			if (num <= 0)
			{
				result.max--;
				num = array.Length - (width - result.max);
			}
		}
		return result;
	}

	public static Rect CropHorizontalVertical(Color[] array, int width, int height)
	{
		IntRange intRange = CropVertical(array, width, height);
		if (intRange == IntRange.zero)
		{
			return Rect.zero;
		}
		IntRange intRange2 = CropHorizontal(array, width, height);
		return Rect.MinMaxRect(intRange2.min, intRange.min, intRange2.max, intRange.max);
	}

	public static Texture2D Crop(this Texture2D tex)
	{
		int width;
		int height;
		Color[] colorSafe = tex.GetColorSafe(out width, out height);
		Rect rect = CropHorizontalVertical(colorSafe, width, height);
		if (rect == Rect.zero)
		{
			throw new ArgumentException("Texture2D has no pixels with alpha >= " + 0.25f, "tex");
		}
		return tex.BlitCrop(rect);
	}
}
