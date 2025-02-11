using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE.Compatibility;

[HarmonyPatch]
internal class Harmony_Compat_RunAndGun
{
	private static readonly string logPrefix = Assembly.GetExecutingAssembly().GetName().Name + " :: ";

	private static readonly Assembly ass = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault((Assembly assembly) => assembly.GetName().Name == "RunAndGun");

	private static MethodBase targetMethod = null;

	private static Type Stance_RunAndGun = null;

	internal static bool Prepare()
	{
		if (ass != null)
		{
			Type[] types = ass.GetTypes();
			foreach (Type type in types)
			{
				if (type.Name == "Verb_TryStartCastOn")
				{
					targetMethod = AccessTools.Method(type, "Prefix", (Type[])null, (Type[])null);
				}
				if (type.Name == "Stance_RunAndGun")
				{
					Stance_RunAndGun = type;
					CompAmmoUser.rgStance = type;
				}
			}
			if (targetMethod == null || Stance_RunAndGun == null)
			{
				Log.Error(logPrefix + "Failed to find target method while attempting to patch RunAndGun.");
				return false;
			}
			return true;
		}
		return false;
	}

	internal static MethodBase TargetMethod()
	{
		Log.Message(logPrefix + "Applying compatibility patch for " + ass.FullName);
		return targetMethod;
	}

	internal static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
	{
		bool patched = false;
		bool ready = false;
		List<CodeInstruction> patch = new List<CodeInstruction>
		{
			new CodeInstruction(OpCodes.Ldarg_0, (object)null),
			new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_Compat_RunAndGun), "CanBeFiredNow", (Type[])null, (Type[])null)),
			new CodeInstruction(OpCodes.Brfalse, (object)null)
		};
		foreach (CodeInstruction code in instructions)
		{
			yield return code;
			if (patched)
			{
				continue;
			}
			if (code.opcode == OpCodes.Isinst && code.operand == Stance_RunAndGun)
			{
				ready = true;
			}
			if (!ready || !(code.opcode == OpCodes.Brtrue))
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

	internal static bool CanBeFiredNow(Verb instance)
	{
		return instance.EquipmentSource.TryGetComp<CompAmmoUser>()?.CanBeFiredNow ?? true;
	}
}
