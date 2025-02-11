using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CombatExtended.Utilities;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
[HarmonyPriority(800)]
public class Harmony_Thing_Position
{
	private static FieldInfo fPosition = AccessTools.Field(typeof(Thing), "positionInt");

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		List<CodeInstruction> codes = instructions.ToList();
		bool finished = false;
		Label l1 = generator.DefineLabel();
		Label l2 = generator.DefineLabel();
		for (int i = 0; i < codes.Count; i++)
		{
			if (!finished && codes[i].opcode == OpCodes.Stfld && CodeInstructionExtensions.OperandIs(codes[i], (MemberInfo)fPosition))
			{
				finished = true;
				yield return codes[i];
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.PropertyGetter(typeof(Thing), "Spawned"));
				yield return new CodeInstruction(OpCodes.Brfalse_S, (object)l1);
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.PropertyGetter(typeof(Thing), "Map"));
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(ThingsTracker), "GetTracker", (Type[])null, (Type[])null));
				yield return new CodeInstruction(OpCodes.Dup, (object)null);
				yield return new CodeInstruction(OpCodes.Brfalse_S, (object)l2);
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(ThingsTracker), "Notify_PositionChanged", (Type[])null, (Type[])null));
				yield return new CodeInstruction(OpCodes.Br_S, (object)l1);
				yield return new CodeInstruction(OpCodes.Pop, (object)null)
				{
					labels = new List<Label> { l2 }
				};
				codes[i + 1].labels.Add(l1);
			}
			else
			{
				yield return codes[i];
			}
		}
	}
}
