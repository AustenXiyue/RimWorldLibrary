using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(CompressibilityDecider), "DetermineReferences")]
public class CompressibilityDecider_DetermineReferences
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		List<CodeInstruction> instructionsList = instructions.ToList();
		Label notProjectileLabel = generator.DefineLabel();
		for (int i = 0; i < instructionsList.Count(); i++)
		{
			if (CodeInstructionExtensions.Is(instructionsList[i], OpCodes.Castclass, (MemberInfo)typeof(Projectile)) && CodeInstructionExtensions.IsLdloc(instructionsList[i - 2], (LocalBuilder)null))
			{
				for (int j = i + 1; j < instructionsList.Count(); j++)
				{
					if (CodeInstructionExtensions.IsLdloc(instructionsList[j], (LocalBuilder)null) && CodeInstructionExtensions.OperandIs(instructionsList[j], instructionsList[i - 2].operand) && instructionsList[j + 1].opcode == OpCodes.Ldc_I4_1 && instructionsList[j + 2].opcode == OpCodes.Add)
					{
						instructionsList[j].labels.Add(notProjectileLabel);
						break;
					}
				}
				yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Projectile));
				yield return new CodeInstruction(OpCodes.Brfalse_S, (object)notProjectileLabel);
				yield return instructionsList[i - 3].Clone();
				yield return instructionsList[i - 2].Clone();
				yield return instructionsList[i - 1].Clone();
			}
			yield return instructionsList[i];
		}
	}
}
