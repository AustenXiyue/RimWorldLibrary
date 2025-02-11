using System;
using UnityEngine;

namespace CombatExtended.RocketGUI;

public static class RectUtility
{
	public static Rect[] Columns(this Rect rect, int pieces, float gap = 5f)
	{
		if (pieces <= 1)
		{
			throw new InvalidOperationException("Can't divide into 1 or less pieces");
		}
		float num = rect.width / (float)pieces - gap * (float)(pieces - 1);
		Rect[] array = new Rect[pieces];
		Rect source = new Rect(rect.position, new Vector2(num, rect.height));
		for (int i = 0; i < pieces; i++)
		{
			array[i] = new Rect(source);
			source.x += num;
		}
		return array;
	}

	public static Rect SliceXPixels(this ref Rect inRect, float pixels)
	{
		Rect result = new Rect(inRect.x, inRect.y, Mathf.Min(inRect.width, pixels), inRect.height);
		inRect.xMin += result.width;
		return result;
	}

	public static Rect SliceYPixels(this ref Rect inRect, float pixels)
	{
		Rect result = new Rect(inRect.x, inRect.y, inRect.width, Mathf.Min(inRect.height, pixels));
		inRect.yMin += result.height;
		return result;
	}

	public static Rect SliceXPart(this ref Rect inRect, float part)
	{
		Rect result = new Rect(inRect.x, inRect.y, inRect.width * part, inRect.height);
		inRect.xMin += result.width;
		return result;
	}

	public static Rect SliceYPart(this ref Rect inRect, float part)
	{
		Rect result = new Rect(inRect.x, inRect.y, inRect.width, inRect.height * part);
		inRect.yMin += result.height;
		return result;
	}

	public static Rect[] Rows(this Rect rect, int pieces, float gap = 5f)
	{
		if (pieces <= 1)
		{
			throw new InvalidOperationException("Can't divide into 1 or less pieces");
		}
		float num = rect.height / (float)pieces - gap * (float)(pieces - 1);
		Rect[] array = new Rect[pieces];
		Rect source = new Rect(rect.position, new Vector2(rect.width, num));
		for (int i = 0; i < pieces; i++)
		{
			array[i] = new Rect(source);
			source.y += num;
		}
		return array;
	}
}
