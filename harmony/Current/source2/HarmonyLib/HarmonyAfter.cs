using System;

namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HarmonyAfter : HarmonyAttribute
{
	public HarmonyAfter(params string[] after)
	{
		info.after = after;
	}
}
