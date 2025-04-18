using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

public static class CodeInstructionExtensions
{
	internal static readonly HashSet<OpCode> opcodesCalling = new HashSet<OpCode>
	{
		OpCodes.Call,
		OpCodes.Callvirt
	};

	internal static readonly HashSet<OpCode> opcodesLoadingLocalByAddress = new HashSet<OpCode>
	{
		OpCodes.Ldloca_S,
		OpCodes.Ldloca
	};

	internal static readonly HashSet<OpCode> opcodesLoadingLocalNormal = new HashSet<OpCode>
	{
		OpCodes.Ldloc_0,
		OpCodes.Ldloc_1,
		OpCodes.Ldloc_2,
		OpCodes.Ldloc_3,
		OpCodes.Ldloc_S,
		OpCodes.Ldloc
	};

	internal static readonly HashSet<OpCode> opcodesStoringLocal = new HashSet<OpCode>
	{
		OpCodes.Stloc_0,
		OpCodes.Stloc_1,
		OpCodes.Stloc_2,
		OpCodes.Stloc_3,
		OpCodes.Stloc_S,
		OpCodes.Stloc
	};

	internal static readonly HashSet<OpCode> opcodesLoadingArgumentByAddress = new HashSet<OpCode>
	{
		OpCodes.Ldarga_S,
		OpCodes.Ldarga
	};

	internal static readonly HashSet<OpCode> opcodesLoadingArgumentNormal = new HashSet<OpCode>
	{
		OpCodes.Ldarg_0,
		OpCodes.Ldarg_1,
		OpCodes.Ldarg_2,
		OpCodes.Ldarg_3,
		OpCodes.Ldarg_S,
		OpCodes.Ldarg
	};

	internal static readonly HashSet<OpCode> opcodesStoringArgument = new HashSet<OpCode>
	{
		OpCodes.Starg_S,
		OpCodes.Starg
	};

	internal static readonly HashSet<OpCode> opcodesBranching = new HashSet<OpCode>
	{
		OpCodes.Br_S,
		OpCodes.Brfalse_S,
		OpCodes.Brtrue_S,
		OpCodes.Beq_S,
		OpCodes.Bge_S,
		OpCodes.Bgt_S,
		OpCodes.Ble_S,
		OpCodes.Blt_S,
		OpCodes.Bne_Un_S,
		OpCodes.Bge_Un_S,
		OpCodes.Bgt_Un_S,
		OpCodes.Ble_Un_S,
		OpCodes.Blt_Un_S,
		OpCodes.Br,
		OpCodes.Brfalse,
		OpCodes.Brtrue,
		OpCodes.Beq,
		OpCodes.Bge,
		OpCodes.Bgt,
		OpCodes.Ble,
		OpCodes.Blt,
		OpCodes.Bne_Un,
		OpCodes.Bge_Un,
		OpCodes.Bgt_Un,
		OpCodes.Ble_Un,
		OpCodes.Blt_Un
	};

	private static readonly HashSet<OpCode> constantLoadingCodes = new HashSet<OpCode>
	{
		OpCodes.Ldc_I4_M1,
		OpCodes.Ldc_I4_0,
		OpCodes.Ldc_I4_1,
		OpCodes.Ldc_I4_2,
		OpCodes.Ldc_I4_3,
		OpCodes.Ldc_I4_4,
		OpCodes.Ldc_I4_5,
		OpCodes.Ldc_I4_6,
		OpCodes.Ldc_I4_7,
		OpCodes.Ldc_I4_8,
		OpCodes.Ldc_I4,
		OpCodes.Ldc_I4_S,
		OpCodes.Ldc_I8,
		OpCodes.Ldc_R4,
		OpCodes.Ldc_R8
	};

	public static bool IsValid(this OpCode code)
	{
		return code.Size > 0;
	}

	public static bool OperandIs(this CodeInstruction code, object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (code.operand == null)
		{
			return false;
		}
		Type type = value.GetType();
		Type type2 = code.operand.GetType();
		if (AccessTools.IsInteger(type) && AccessTools.IsNumber(type2))
		{
			return Convert.ToInt64(code.operand) == Convert.ToInt64(value);
		}
		if (AccessTools.IsFloatingPoint(type) && AccessTools.IsNumber(type2))
		{
			return Convert.ToDouble(code.operand) == Convert.ToDouble(value);
		}
		return object.Equals(code.operand, value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool OperandIs(this CodeInstruction code, MemberInfo value)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		return object.Equals(code.operand, value);
	}

	public static bool Is(this CodeInstruction code, OpCode opcode, object operand)
	{
		if (code.opcode == opcode)
		{
			return code.OperandIs(operand);
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool Is(this CodeInstruction code, OpCode opcode, MemberInfo operand)
	{
		if (code.opcode == opcode)
		{
			return code.OperandIs(operand);
		}
		return false;
	}

	public static bool IsLdarg(this CodeInstruction code, int? n = null)
	{
		if ((!n.HasValue || n.Value == 0) && code.opcode == OpCodes.Ldarg_0)
		{
			return true;
		}
		if ((!n.HasValue || n.Value == 1) && code.opcode == OpCodes.Ldarg_1)
		{
			return true;
		}
		if ((!n.HasValue || n.Value == 2) && code.opcode == OpCodes.Ldarg_2)
		{
			return true;
		}
		if ((!n.HasValue || n.Value == 3) && code.opcode == OpCodes.Ldarg_3)
		{
			return true;
		}
		if (code.opcode == OpCodes.Ldarg && (!n.HasValue || n.Value == Convert.ToInt32(code.operand)))
		{
			return true;
		}
		if (code.opcode == OpCodes.Ldarg_S && (!n.HasValue || n.Value == Convert.ToInt32(code.operand)))
		{
			return true;
		}
		return false;
	}

	public static bool IsLdarga(this CodeInstruction code, int? n = null)
	{
		if (code.opcode != OpCodes.Ldarga && code.opcode != OpCodes.Ldarga_S)
		{
			return false;
		}
		if (n.HasValue)
		{
			return n.Value == Convert.ToInt32(code.operand);
		}
		return true;
	}

	public static bool IsStarg(this CodeInstruction code, int? n = null)
	{
		if (code.opcode != OpCodes.Starg && code.opcode != OpCodes.Starg_S)
		{
			return false;
		}
		if (n.HasValue)
		{
			return n.Value == Convert.ToInt32(code.operand);
		}
		return true;
	}

	public static bool IsLdloc(this CodeInstruction code, LocalBuilder variable = null)
	{
		if (!opcodesLoadingLocalNormal.Contains(code.opcode) && !opcodesLoadingLocalByAddress.Contains(code.opcode))
		{
			return false;
		}
		if (variable != null)
		{
			return object.Equals(variable, code.operand);
		}
		return true;
	}

	public static bool IsStloc(this CodeInstruction code, LocalBuilder variable = null)
	{
		if (!opcodesStoringLocal.Contains(code.opcode))
		{
			return false;
		}
		if (variable != null)
		{
			return object.Equals(variable, code.operand);
		}
		return true;
	}

	public static bool Branches(this CodeInstruction code, out Label? label)
	{
		if (opcodesBranching.Contains(code.opcode))
		{
			label = (Label)code.operand;
			return true;
		}
		label = null;
		return false;
	}

	public static bool Calls(this CodeInstruction code, MethodInfo method)
	{
		if ((object)method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (code.opcode != OpCodes.Call && code.opcode != OpCodes.Callvirt)
		{
			return false;
		}
		return object.Equals(code.operand, method);
	}

	public static bool LoadsConstant(this CodeInstruction code)
	{
		return constantLoadingCodes.Contains(code.opcode);
	}

	public static bool LoadsConstant(this CodeInstruction code, long number)
	{
		OpCode opcode = code.opcode;
		if (number == -1 && opcode == OpCodes.Ldc_I4_M1)
		{
			return true;
		}
		if (number == 0L && opcode == OpCodes.Ldc_I4_0)
		{
			return true;
		}
		if (number == 1 && opcode == OpCodes.Ldc_I4_1)
		{
			return true;
		}
		if (number == 2 && opcode == OpCodes.Ldc_I4_2)
		{
			return true;
		}
		if (number == 3 && opcode == OpCodes.Ldc_I4_3)
		{
			return true;
		}
		if (number == 4 && opcode == OpCodes.Ldc_I4_4)
		{
			return true;
		}
		if (number == 5 && opcode == OpCodes.Ldc_I4_5)
		{
			return true;
		}
		if (number == 6 && opcode == OpCodes.Ldc_I4_6)
		{
			return true;
		}
		if (number == 7 && opcode == OpCodes.Ldc_I4_7)
		{
			return true;
		}
		if (number == 8 && opcode == OpCodes.Ldc_I4_8)
		{
			return true;
		}
		if (opcode != OpCodes.Ldc_I4 && opcode != OpCodes.Ldc_I4_S && opcode != OpCodes.Ldc_I8)
		{
			return false;
		}
		return Convert.ToInt64(code.operand) == number;
	}

	public static bool LoadsConstant(this CodeInstruction code, double number)
	{
		if (code.opcode != OpCodes.Ldc_R4 && code.opcode != OpCodes.Ldc_R8)
		{
			return false;
		}
		double num = Convert.ToDouble(code.operand);
		return num == number;
	}

	public static bool LoadsConstant(this CodeInstruction code, Enum e)
	{
		return code.LoadsConstant(Convert.ToInt64(e));
	}

	public static bool LoadsConstant(this CodeInstruction code, string str)
	{
		if (code.opcode != OpCodes.Ldstr)
		{
			return false;
		}
		string text = Convert.ToString(code.operand);
		return text == str;
	}

	public static bool LoadsField(this CodeInstruction code, FieldInfo field, bool byAddress = false)
	{
		if ((object)field == null)
		{
			throw new ArgumentNullException("field");
		}
		OpCode opCode = (field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld);
		if (!byAddress && code.opcode == opCode && object.Equals(code.operand, field))
		{
			return true;
		}
		OpCode opCode2 = (field.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda);
		if (byAddress && code.opcode == opCode2 && object.Equals(code.operand, field))
		{
			return true;
		}
		return false;
	}

	public static bool StoresField(this CodeInstruction code, FieldInfo field)
	{
		if ((object)field == null)
		{
			throw new ArgumentNullException("field");
		}
		OpCode opCode = (field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld);
		if (code.opcode == opCode)
		{
			return object.Equals(code.operand, field);
		}
		return false;
	}

	public static int LocalIndex(this CodeInstruction code)
	{
		if (code.opcode == OpCodes.Ldloc_0 || code.opcode == OpCodes.Stloc_0)
		{
			return 0;
		}
		if (code.opcode == OpCodes.Ldloc_1 || code.opcode == OpCodes.Stloc_1)
		{
			return 1;
		}
		if (code.opcode == OpCodes.Ldloc_2 || code.opcode == OpCodes.Stloc_2)
		{
			return 2;
		}
		if (code.opcode == OpCodes.Ldloc_3 || code.opcode == OpCodes.Stloc_3)
		{
			return 3;
		}
		if (code.opcode == OpCodes.Ldloc_S || code.opcode == OpCodes.Ldloc)
		{
			return Convert.ToInt32(code.operand);
		}
		if (code.opcode == OpCodes.Stloc_S || code.opcode == OpCodes.Stloc)
		{
			return Convert.ToInt32(code.operand);
		}
		if (code.opcode == OpCodes.Ldloca_S || code.opcode == OpCodes.Ldloca)
		{
			return Convert.ToInt32(code.operand);
		}
		throw new ArgumentException("Instruction is not a load or store", "code");
	}

	public static int ArgumentIndex(this CodeInstruction code)
	{
		if (code.opcode == OpCodes.Ldarg_0)
		{
			return 0;
		}
		if (code.opcode == OpCodes.Ldarg_1)
		{
			return 1;
		}
		if (code.opcode == OpCodes.Ldarg_2)
		{
			return 2;
		}
		if (code.opcode == OpCodes.Ldarg_3)
		{
			return 3;
		}
		if (code.opcode == OpCodes.Ldarg_S || code.opcode == OpCodes.Ldarg)
		{
			return Convert.ToInt32(code.operand);
		}
		if (code.opcode == OpCodes.Starg_S || code.opcode == OpCodes.Starg)
		{
			return Convert.ToInt32(code.operand);
		}
		if (code.opcode == OpCodes.Ldarga_S || code.opcode == OpCodes.Ldarga)
		{
			return Convert.ToInt32(code.operand);
		}
		throw new ArgumentException("Instruction is not a load or store", "code");
	}

	public static CodeInstruction WithLabels(this CodeInstruction code, params Label[] labels)
	{
		code.labels.AddRange(labels);
		return code;
	}

	public static CodeInstruction WithLabels(this CodeInstruction code, IEnumerable<Label> labels)
	{
		code.labels.AddRange(labels);
		return code;
	}

	public static List<Label> ExtractLabels(this CodeInstruction code)
	{
		List<Label> result = new List<Label>(code.labels);
		code.labels.Clear();
		return result;
	}

	public static CodeInstruction MoveLabelsTo(this CodeInstruction code, CodeInstruction other)
	{
		other.WithLabels(code.ExtractLabels());
		return code;
	}

	public static CodeInstruction MoveLabelsFrom(this CodeInstruction code, CodeInstruction other)
	{
		return code.WithLabels(other.ExtractLabels());
	}

	public static CodeInstruction WithBlocks(this CodeInstruction code, params ExceptionBlock[] blocks)
	{
		code.blocks.AddRange(blocks);
		return code;
	}

	public static CodeInstruction WithBlocks(this CodeInstruction code, IEnumerable<ExceptionBlock> blocks)
	{
		code.blocks.AddRange(blocks);
		return code;
	}

	public static List<ExceptionBlock> ExtractBlocks(this CodeInstruction code)
	{
		List<ExceptionBlock> result = new List<ExceptionBlock>(code.blocks);
		code.blocks.Clear();
		return result;
	}

	public static CodeInstruction MoveBlocksTo(this CodeInstruction code, CodeInstruction other)
	{
		other.WithBlocks(code.ExtractBlocks());
		return code;
	}

	public static CodeInstruction MoveBlocksFrom(this CodeInstruction code, CodeInstruction other)
	{
		return code.WithBlocks(other.ExtractBlocks());
	}
}
