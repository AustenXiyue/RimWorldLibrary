using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE.Compatibility;

[HarmonyPatch]
internal class Harmony_Compat_BBBodySupport
{
	private static readonly string logPrefix = Assembly.GetExecutingAssembly().GetName().Name + " :: ";

	private static Assembly ass = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault((Assembly assembly) => assembly.GetName().Name == "BBBodySupport");

	private static bool IsHeadwear(ApparelLayerDef layer)
	{
		return layer.GetModExtension<ApparelLayerExtension>()?.IsHeadwear ?? false;
	}

	private static bool Prepare()
	{
		Assembly assembly = ass;
		if ((object)assembly != null && assembly.FullName.Contains("BBBodySupport"))
		{
			return true;
		}
		return false;
	}

	private static IEnumerable<MethodBase> TargetMethods()
	{
		bool found = false;
		Type[] types = ass.GetTypes();
		foreach (Type t in types)
		{
			foreach (MethodInfo m in AccessTools.GetDeclaredMethods(t))
			{
				if (m.Name.Contains("BBBody_ApparelPatch") || m.Name.Contains("BBBody_ApparelZombiefiedPatch"))
				{
					found = true;
					yield return m;
				}
			}
		}
		if (found)
		{
			Log.Message(logPrefix + "Applying compatibility patch for " + ass.FullName);
		}
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		bool patched = false;
		bool ready = false;
		List<CodeInstruction> patch = new List<CodeInstruction>
		{
			new CodeInstruction(OpCodes.Ldarg_0, (object)null),
			new CodeInstruction(OpCodes.Ldind_Ref, (object)null),
			new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(Thing), "def")),
			new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(ThingDef), "apparel")),
			new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Property(typeof(ApparelProperties), "LastLayer").GetGetMethod()),
			new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_Compat_BBBodySupport), "IsHeadwear", (Type[])null, (Type[])null)),
			new CodeInstruction(OpCodes.Brtrue, (object)null)
		};
		foreach (CodeInstruction code in instructions)
		{
			yield return code;
			if (patched)
			{
				continue;
			}
			if (code.opcode == OpCodes.Ldsfld)
			{
				ready = true;
			}
			if (!ready || !(code.opcode == OpCodes.Beq_S))
			{
				continue;
			}
			patch.Last().operand = code.operand;
			foreach (CodeInstruction item in patch)
			{
				yield return item;
			}
			patched = true;
		}
	}
}
