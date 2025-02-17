using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

internal class CodeTranspiler
{
	private readonly IEnumerable<CodeInstruction> codeInstructions;

	private readonly List<MethodInfo> transpilers = new List<MethodInfo>();

	private static readonly Dictionary<OpCode, OpCode> allJumpCodes = new Dictionary<OpCode, OpCode>
	{
		{
			OpCodes.Beq_S,
			OpCodes.Beq
		},
		{
			OpCodes.Bge_S,
			OpCodes.Bge
		},
		{
			OpCodes.Bge_Un_S,
			OpCodes.Bge_Un
		},
		{
			OpCodes.Bgt_S,
			OpCodes.Bgt
		},
		{
			OpCodes.Bgt_Un_S,
			OpCodes.Bgt_Un
		},
		{
			OpCodes.Ble_S,
			OpCodes.Ble
		},
		{
			OpCodes.Ble_Un_S,
			OpCodes.Ble_Un
		},
		{
			OpCodes.Blt_S,
			OpCodes.Blt
		},
		{
			OpCodes.Blt_Un_S,
			OpCodes.Blt_Un
		},
		{
			OpCodes.Bne_Un_S,
			OpCodes.Bne_Un
		},
		{
			OpCodes.Brfalse_S,
			OpCodes.Brfalse
		},
		{
			OpCodes.Brtrue_S,
			OpCodes.Brtrue
		},
		{
			OpCodes.Br_S,
			OpCodes.Br
		},
		{
			OpCodes.Leave_S,
			OpCodes.Leave
		}
	};

	internal CodeTranspiler(List<ILInstruction> ilInstructions)
	{
		codeInstructions = ilInstructions.Select((ILInstruction ilInstruction) => ilInstruction.GetCodeInstruction()).ToList().AsEnumerable();
	}

	internal void Add(MethodInfo transpiler)
	{
		transpilers.Add(transpiler);
	}

	internal static object ConvertInstruction(Type type, object instruction, out Dictionary<string, object> unassigned)
	{
		Dictionary<string, object> nonExisting = new Dictionary<string, object>();
		object result = AccessTools.MakeDeepCopy(instruction, type, delegate(string namePath, Traverse trvSrc, Traverse trvDest)
		{
			object value = trvSrc.GetValue();
			if (!trvDest.FieldExists())
			{
				nonExisting[namePath] = value;
				return (object)null;
			}
			return (namePath == "opcode") ? ((object)ReplaceShortJumps((OpCode)value)) : value;
		});
		unassigned = nonExisting;
		return result;
	}

	internal static bool ShouldAddExceptionInfo(object op, int opIndex, List<object> originalInstructions, List<object> newInstructions, Dictionary<object, Dictionary<string, object>> unassignedValues)
	{
		int num = originalInstructions.IndexOf(op);
		if (num == -1)
		{
			return false;
		}
		if (!unassignedValues.TryGetValue(op, out var unassigned))
		{
			return false;
		}
		if (!unassigned.TryGetValue("blocks", out var blocksObject))
		{
			return false;
		}
		List<ExceptionBlock> blocks = blocksObject as List<ExceptionBlock>;
		int num2 = newInstructions.Count((object instr) => instr == op);
		if (num2 <= 1)
		{
			return true;
		}
		ExceptionBlock exceptionBlock = blocks.FirstOrDefault((ExceptionBlock block) => block.blockType != ExceptionBlockType.EndExceptionBlock);
		ExceptionBlock exceptionBlock2 = blocks.FirstOrDefault((ExceptionBlock block) => block.blockType == ExceptionBlockType.EndExceptionBlock);
		if (exceptionBlock != null && exceptionBlock2 == null)
		{
			object obj = originalInstructions.Skip(num + 1).FirstOrDefault(delegate(object instr)
			{
				if (!unassignedValues.TryGetValue(instr, out unassigned))
				{
					return false;
				}
				if (!unassigned.TryGetValue("blocks", out blocksObject))
				{
					return false;
				}
				blocks = blocksObject as List<ExceptionBlock>;
				return blocks.Count > 0;
			});
			if (obj != null)
			{
				int num3 = num + 1;
				int num4 = num3 + originalInstructions.Skip(num3).ToList().IndexOf(obj) - 1;
				IEnumerable<object> first = originalInstructions.GetRange(num3, num4 - num3).Intersect(newInstructions);
				obj = newInstructions.Skip(opIndex + 1).FirstOrDefault(delegate(object instr)
				{
					if (!unassignedValues.TryGetValue(instr, out unassigned))
					{
						return false;
					}
					if (!unassigned.TryGetValue("blocks", out blocksObject))
					{
						return false;
					}
					blocks = blocksObject as List<ExceptionBlock>;
					return blocks.Count > 0;
				});
				if (obj != null)
				{
					num3 = opIndex + 1;
					num4 = num3 + newInstructions.Skip(opIndex + 1).ToList().IndexOf(obj) - 1;
					List<object> range = newInstructions.GetRange(num3, num4 - num3);
					List<object> list = first.Except(range).ToList();
					return list.Count == 0;
				}
			}
		}
		if (exceptionBlock == null && exceptionBlock2 != null)
		{
			object obj2 = originalInstructions.GetRange(0, num).LastOrDefault(delegate(object instr)
			{
				if (!unassignedValues.TryGetValue(instr, out unassigned))
				{
					return false;
				}
				if (!unassigned.TryGetValue("blocks", out blocksObject))
				{
					return false;
				}
				blocks = blocksObject as List<ExceptionBlock>;
				return blocks.Count > 0;
			});
			if (obj2 != null)
			{
				int num5 = originalInstructions.GetRange(0, num).LastIndexOf(obj2);
				int num6 = num;
				IEnumerable<object> first2 = originalInstructions.GetRange(num5, num6 - num5).Intersect(newInstructions);
				obj2 = newInstructions.GetRange(0, opIndex).LastOrDefault(delegate(object instr)
				{
					if (!unassignedValues.TryGetValue(instr, out unassigned))
					{
						return false;
					}
					if (!unassigned.TryGetValue("blocks", out blocksObject))
					{
						return false;
					}
					blocks = blocksObject as List<ExceptionBlock>;
					return blocks.Count > 0;
				});
				if (obj2 != null)
				{
					num5 = newInstructions.GetRange(0, opIndex).LastIndexOf(obj2);
					num6 = opIndex;
					List<object> range2 = newInstructions.GetRange(num5, num6 - num5);
					IEnumerable<object> source = first2.Except(range2);
					return !source.Any();
				}
			}
		}
		return true;
	}

	internal static IEnumerable ConvertInstructionsAndUnassignedValues(Type type, IEnumerable enumerable, out Dictionary<object, Dictionary<string, object>> unassignedValues)
	{
		Assembly assembly = type.GetGenericTypeDefinition().Assembly;
		Type type2 = assembly.GetType(typeof(List<>).FullName);
		Type type3 = type.GetGenericArguments()[0];
		Type type4 = type2.MakeGenericType(type3);
		Type type5 = assembly.GetType(type4.FullName);
		object obj = Activator.CreateInstance(type5);
		MethodInfo method = obj.GetType().GetMethod("Add");
		unassignedValues = new Dictionary<object, Dictionary<string, object>>();
		foreach (object item in enumerable)
		{
			Dictionary<string, object> unassigned;
			object obj2 = ConvertInstruction(type3, item, out unassigned);
			unassignedValues.Add(obj2, unassigned);
			method.Invoke(obj, new object[1] { obj2 });
		}
		return obj as IEnumerable;
	}

	internal static IEnumerable ConvertToOurInstructions(IEnumerable instructions, Type codeInstructionType, List<object> originalInstructions, Dictionary<object, Dictionary<string, object>> unassignedValues)
	{
		List<object> newInstructions = instructions.Cast<object>().ToList();
		int index = -1;
		foreach (object item in newInstructions)
		{
			index++;
			object obj = AccessTools.MakeDeepCopy(item, codeInstructionType);
			if (unassignedValues.TryGetValue(item, out var value))
			{
				bool flag = ShouldAddExceptionInfo(item, index, originalInstructions, newInstructions, unassignedValues);
				Traverse traverse = Traverse.Create(obj);
				foreach (KeyValuePair<string, object> item2 in value)
				{
					if (flag || item2.Key != "blocks")
					{
						traverse.Field(item2.Key).SetValue(item2.Value);
					}
				}
			}
			yield return obj;
		}
	}

	private static bool IsCodeInstructionsParameter(Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition().Name.StartsWith("IEnumerable", StringComparison.Ordinal);
		}
		return false;
	}

	internal static IEnumerable ConvertToGeneralInstructions(MethodInfo transpiler, IEnumerable enumerable, out Dictionary<object, Dictionary<string, object>> unassignedValues)
	{
		Type type = (from p in transpiler.GetParameters()
			select p.ParameterType).FirstOrDefault(IsCodeInstructionsParameter);
		if (type == typeof(IEnumerable<CodeInstruction>))
		{
			unassignedValues = null;
			return (enumerable as IList<CodeInstruction>) ?? ((enumerable as IEnumerable<CodeInstruction>) ?? enumerable.Cast<CodeInstruction>()).ToList();
		}
		return ConvertInstructionsAndUnassignedValues(type, enumerable, out unassignedValues);
	}

	internal static List<object> GetTranspilerCallParameters(ILGenerator generator, MethodInfo transpiler, MethodBase method, IEnumerable instructions)
	{
		List<object> parameter = new List<object>();
		(from param in transpiler.GetParameters()
			select param.ParameterType).Do(delegate(Type type)
		{
			if (type.IsAssignableFrom(typeof(ILGenerator)))
			{
				parameter.Add(generator);
			}
			else if (type.IsAssignableFrom(typeof(MethodBase)))
			{
				parameter.Add(method);
			}
			else if (IsCodeInstructionsParameter(type))
			{
				parameter.Add(instructions);
			}
		});
		return parameter;
	}

	internal List<CodeInstruction> GetResult(ILGenerator generator, MethodBase method)
	{
		IEnumerable instructions = codeInstructions;
		transpilers.ForEach(delegate(MethodInfo transpiler)
		{
			instructions = ConvertToGeneralInstructions(transpiler, instructions, out var unassignedValues);
			List<object> originalInstructions = null;
			if (unassignedValues != null)
			{
				originalInstructions = instructions.Cast<object>().ToList();
			}
			List<object> transpilerCallParameters = GetTranspilerCallParameters(generator, transpiler, method, instructions);
			if (transpiler.Invoke(null, transpilerCallParameters.ToArray()) is IEnumerable enumerable)
			{
				instructions = enumerable;
			}
			if (unassignedValues != null)
			{
				instructions = ConvertToOurInstructions(instructions, typeof(CodeInstruction), originalInstructions, unassignedValues);
			}
		});
		return (instructions as List<CodeInstruction>) ?? instructions.Cast<CodeInstruction>().ToList();
	}

	private static OpCode ReplaceShortJumps(OpCode opcode)
	{
		foreach (KeyValuePair<OpCode, OpCode> allJumpCode in allJumpCodes)
		{
			if (opcode == allJumpCode.Key)
			{
				return allJumpCode.Value;
			}
		}
		return opcode;
	}
}
