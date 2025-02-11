using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Graphic_StackCountRanged : Graphic_Collection
{
	public override Material MatSingle => subGraphics[subGraphics.Length - 1].MatSingle;

	public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		return GraphicDatabase.Get<Graphic_StackCount>(path, newShader, drawSize, newColor, newColorTwo, data);
	}

	public override Material MatAt(Rot4 rot, Thing thing = null)
	{
		if (thing == null)
		{
			return MatSingle;
		}
		return MatSingleFor(thing);
	}

	public override Material MatSingleFor(Thing thing)
	{
		if (thing == null)
		{
			return MatSingle;
		}
		return SubGraphicFor(thing).MatSingle;
	}

	public Graphic SubGraphicFor(Thing thing)
	{
		return SubGraphicForStackCount(thing.stackCount, thing.def);
	}

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Graphic graphic = ((thing == null) ? subGraphics[0] : SubGraphicFor(thing));
		graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
	}

	public Graphic SubGraphicForStackCount(int stackCount, ThingDef def)
	{
		switch (subGraphics.Length)
		{
		case 1:
			return subGraphics[0];
		case 2:
			if (stackCount == 1)
			{
				return subGraphics[0];
			}
			return subGraphics[1];
		default:
		{
			List<KeyValuePair<int, Graphic>> list = new List<KeyValuePair<int, Graphic>>();
			Graphic[] array = subGraphics;
			foreach (Graphic graphic in array)
			{
				int key = int.Parse(Regex.Match(graphic.path, "\\d+(?!\\D*\\d)").Value);
				list.Add(new KeyValuePair<int, Graphic>(key, graphic));
			}
			list.Reverse();
			Graphic value = list[0].Value;
			foreach (KeyValuePair<int, Graphic> item in list)
			{
				if (stackCount <= item.Key)
				{
					value = item.Value;
					continue;
				}
				break;
			}
			return value;
		}
		}
	}

	public override string ToString()
	{
		return "StackCount(path=" + path + ", count=" + subGraphics.Length + ")";
	}
}
