using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch]
public class WoundWithShader : FleshTypeDef.Wound
{
	public ShaderTypeDef shader;

	[HarmonyPatch(typeof(FleshTypeDef.Wound), "Resolve")]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> Resolve_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		foreach (CodeInstruction instruction in instructions)
		{
			yield return instruction;
			if (instruction.opcode == OpCodes.Stloc_0)
			{
				Label label = generator.DefineLabel();
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(WoundWithShader));
				yield return new CodeInstruction(OpCodes.Dup, (object)null);
				yield return new CodeInstruction(OpCodes.Brfalse, (object)label);
				yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(WoundWithShader), "shader"));
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.PropertyGetter(typeof(ShaderTypeDef), "Shader"));
				yield return new CodeInstruction(OpCodes.Stloc_0, (object)null);
				yield return CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Pop, (object)null), new Label[1] { label });
			}
		}
	}
}
