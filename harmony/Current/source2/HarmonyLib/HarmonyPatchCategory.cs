using System;

namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HarmonyPatchCategory : HarmonyAttribute
{
	public HarmonyPatchCategory(string category)
	{
		info.category = category;
	}
}
