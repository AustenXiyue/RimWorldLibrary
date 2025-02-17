using System;

namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HarmonyBefore : HarmonyAttribute
{
	public HarmonyBefore(params string[] before)
	{
		info.before = before;
	}
}
