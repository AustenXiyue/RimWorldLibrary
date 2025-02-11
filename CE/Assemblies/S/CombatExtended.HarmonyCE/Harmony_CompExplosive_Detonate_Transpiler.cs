using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(CompExplosive), "Detonate")]
public class Harmony_CompExplosive_Detonate_Transpiler
{
	internal static void ThrowFragments(ThingWithComps parent, Map map, Thing instigator)
	{
		parent.TryGetComp<CompFragments>()?.Throw(parent.PositionHeld.ToVector3(), map, instigator);
	}

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		bool codeFound = false;
		foreach (CodeInstruction code in instructions)
		{
			if (!codeFound && CodeInstructionExtensions.Calls(code, AccessTools.Method(typeof(GenExplosion), "DoExplosion", (Type[])null, (Type[])null)))
			{
				codeFound = true;
				yield return new CodeInstruction(OpCodes.Ldc_R4, (object)0f);
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(ThingComp), "parent"));
				yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(Thing), "stackCount"));
				yield return new CodeInstruction(OpCodes.Conv_R4, (object)null);
				yield return new CodeInstruction(OpCodes.Ldc_R4, (object)0.333f);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Mathf), "Pow", (Type[])null, (Type[])null));
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(CompExplosive), "destroyedThroughDetonation"));
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(ThingComp), "parent"));
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(GenExplosionCE), "DoExplosion", (Type[])null, (Type[])null));
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(ThingComp), "parent"));
				yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
				yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_CompExplosive_Detonate_Transpiler), "ThrowFragments", (Type[])null, (Type[])null));
			}
			else
			{
				yield return code;
			}
		}
		if (!codeFound)
		{
			Log.Warning("CombatExtended :: Could not find doExplosionInfo");
		}
	}
}
