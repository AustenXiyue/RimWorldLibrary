using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.SourceGen.Attributes;
using MonoMod.Utils;

namespace MonoMod.Cil;

[EmitILOverloads("ILOpcodes.txt", "ILMatcher")]
internal static class ILPatternMatchingExt
{
	private sealed class ParameterRefEqualityComparer : IEqualityComparer<ParameterReference>
	{
		public static readonly ParameterRefEqualityComparer Instance = new ParameterRefEqualityComparer();

		public bool Equals(ParameterReference? x, ParameterReference? y)
		{
			if (x == null)
			{
				return y == null;
			}
			if (y == null)
			{
				return false;
			}
			return IsEquivalent(x.ParameterType, y.ParameterType);
		}

		public int GetHashCode([DisallowNull] ParameterReference obj)
		{
			return obj.ParameterType.GetHashCode();
		}
	}

	private static bool IsEquivalent(int l, int r)
	{
		return l == r;
	}

	private static bool IsEquivalent(int l, uint r)
	{
		return l == (int)r;
	}

	private static bool IsEquivalent(long l, long r)
	{
		return l == r;
	}

	private static bool IsEquivalent(long l, ulong r)
	{
		return l == (long)r;
	}

	private static bool IsEquivalent(float l, float r)
	{
		return l == r;
	}

	private static bool IsEquivalent(double l, double r)
	{
		return l == r;
	}

	private static bool IsEquivalent(string l, string r)
	{
		return l == r;
	}

	private static bool IsEquivalent(ILLabel l, ILLabel r)
	{
		return l == r;
	}

	private static bool IsEquivalent(ILLabel l, Instruction r)
	{
		return IsEquivalent(l.Target, r);
	}

	private static bool IsEquivalent(Instruction? l, Instruction? r)
	{
		return l == r;
	}

	private static bool IsEquivalent(TypeReference l, TypeReference r)
	{
		return l == r;
	}

	private static bool IsEquivalent(TypeReference l, Type r)
	{
		return l.Is(r);
	}

	private static bool IsEquivalent(MethodReference l, MethodReference r)
	{
		return l == r;
	}

	private static bool IsEquivalent(MethodReference l, MethodBase r)
	{
		return l.Is(r);
	}

	private static bool IsEquivalent(MethodReference l, Type type, string name)
	{
		if (l.DeclaringType.Is(type))
		{
			return l.Name == name;
		}
		return false;
	}

	private static bool IsEquivalent(FieldReference l, FieldReference r)
	{
		return l == r;
	}

	private static bool IsEquivalent(FieldReference l, FieldInfo r)
	{
		return l.Is(r);
	}

	private static bool IsEquivalent(FieldReference l, Type type, string name)
	{
		if (l.DeclaringType.Is(type))
		{
			return l.Name == name;
		}
		return false;
	}

	private static bool IsEquivalent(ILLabel[] l, ILLabel[] r)
	{
		if (l != r)
		{
			return l.SequenceEqual<ILLabel>(r);
		}
		return true;
	}

	private static bool IsEquivalent(ILLabel[] l, Instruction[] r)
	{
		if (l.Length != r.Length)
		{
			return false;
		}
		for (int i = 0; i < l.Length; i++)
		{
			if (!IsEquivalent(l[i].Target, r[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsEquivalent(IMethodSignature l, IMethodSignature r)
	{
		if (l != r)
		{
			if (l.CallingConvention == r.CallingConvention && l.HasThis == r.HasThis && l.ExplicitThis == r.ExplicitThis && IsEquivalent(l.ReturnType, r.ReturnType))
			{
				return CastParamsToRef(l).SequenceEqual<ParameterReference>(CastParamsToRef(r), ParameterRefEqualityComparer.Instance);
			}
			return false;
		}
		return true;
	}

	private static IEnumerable<ParameterReference> CastParamsToRef(IMethodSignature sig)
	{
		return sig.Parameters;
	}

	private static bool IsEquivalent(IMetadataTokenProvider l, IMetadataTokenProvider r)
	{
		if (l != r)
		{
			return l.MetadataToken == r.MetadataToken;
		}
		return true;
	}

	private static bool IsEquivalent(IMetadataTokenProvider l, Type r)
	{
		if (!(l is TypeReference l2))
		{
			return false;
		}
		return IsEquivalent(l2, r);
	}

	private static bool IsEquivalent(IMetadataTokenProvider l, FieldInfo r)
	{
		if (!(l is FieldReference l2))
		{
			return false;
		}
		return IsEquivalent(l2, r);
	}

	private static bool IsEquivalent(IMetadataTokenProvider l, MethodBase r)
	{
		if (!(l is MethodReference l2))
		{
			return false;
		}
		return IsEquivalent(l2, r);
	}

	public static bool Match(this Instruction instr, OpCode opcode)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == opcode;
	}

	public static bool Match<T>(this Instruction instr, OpCode opcode, T value)
	{
		if (instr.Match<T>(opcode, out var value2))
		{
			return value2?.Equals(value) ?? (value == null);
		}
		return false;
	}

	public static bool Match<T>(this Instruction instr, OpCode opcode, [MaybeNullWhen(false)] out T value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == opcode)
		{
			value = (T)instr.Operand;
			return true;
		}
		value = default(T);
		return false;
	}

	[Obsolete("Leftover from legacy MonoMod, use MatchLeave instead")]
	public static bool MatchLeaveS(this Instruction instr, ILLabel value)
	{
		if (instr.MatchLeaveS(out ILLabel value2))
		{
			return value2 == value;
		}
		return false;
	}

	[Obsolete("Leftover from legacy MonoMod, use MatchLeave instead")]
	public static bool MatchLeaveS(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == OpCodes.Leave_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdarg(this Instruction instr, out int value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == OpCodes.Ldarg || instr.OpCode == OpCodes.Ldarg_S)
		{
			value = ((ParameterReference)instr.Operand).Index;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldarg_0)
		{
			value = 0;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldarg_1)
		{
			value = 1;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldarg_2)
		{
			value = 2;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldarg_3)
		{
			value = 3;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchStarg(this Instruction instr, out int value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == OpCodes.Starg || instr.OpCode == OpCodes.Starg_S)
		{
			value = ((ParameterReference)instr.Operand).Index;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchLdarga(this Instruction instr, out int value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == OpCodes.Ldarga || instr.OpCode == OpCodes.Ldarga_S)
		{
			value = ((ParameterReference)instr.Operand).Index;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchLdloc(this Instruction instr, out int value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == OpCodes.Ldloc || instr.OpCode == OpCodes.Ldloc_S)
		{
			value = ((VariableReference)instr.Operand).Index;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldloc_0)
		{
			value = 0;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldloc_1)
		{
			value = 1;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldloc_2)
		{
			value = 2;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldloc_3)
		{
			value = 3;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchStloc(this Instruction instr, out int value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == OpCodes.Stloc || instr.OpCode == OpCodes.Stloc_S)
		{
			value = ((VariableReference)instr.Operand).Index;
			return true;
		}
		if (instr.OpCode == OpCodes.Stloc_0)
		{
			value = 0;
			return true;
		}
		if (instr.OpCode == OpCodes.Stloc_1)
		{
			value = 1;
			return true;
		}
		if (instr.OpCode == OpCodes.Stloc_2)
		{
			value = 2;
			return true;
		}
		if (instr.OpCode == OpCodes.Stloc_3)
		{
			value = 3;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchLdloca(this Instruction instr, out int value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == OpCodes.Ldloca || instr.OpCode == OpCodes.Ldloca_S)
		{
			value = ((VariableReference)instr.Operand).Index;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchLdcI4(this Instruction instr, out int value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == OpCodes.Ldc_I4)
		{
			value = (int)instr.Operand;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_S)
		{
			value = (sbyte)instr.Operand;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_0)
		{
			value = 0;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_1)
		{
			value = 1;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_2)
		{
			value = 2;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_3)
		{
			value = 3;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_4)
		{
			value = 4;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_5)
		{
			value = 5;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_6)
		{
			value = 6;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_7)
		{
			value = 7;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_8)
		{
			value = 8;
			return true;
		}
		if (instr.OpCode == OpCodes.Ldc_I4_M1)
		{
			value = -1;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, [MaybeNullWhen(false)] out MethodReference value)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		if (instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt)
		{
			value = (MethodReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, Type type)
	{
		if (instr.MatchNewobj(out MethodReference value))
		{
			return value.DeclaringType.Is(type);
		}
		return false;
	}

	public static bool MatchNewobj<T>(this Instruction instr)
	{
		return instr.MatchNewobj(typeof(T));
	}

	public static bool MatchNewobj(this Instruction instr, string typeFullName)
	{
		if (instr.MatchNewobj(out MethodReference value))
		{
			return value.DeclaringType.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchAdd(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Add;
	}

	public static bool MatchAddOvf(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Add_Ovf;
	}

	public static bool MatchAddOvfUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Add_Ovf_Un;
	}

	public static bool MatchAnd(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.And;
	}

	public static bool MatchArglist(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Arglist;
	}

	public static bool MatchBeq(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Beq || instr.OpCode == OpCodes.Beq_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBeq(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBeq(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBeq(this Instruction instr, Instruction value)
	{
		if (instr.MatchBeq(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBge(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Bge || instr.OpCode == OpCodes.Bge_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBge(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBge(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBge(this Instruction instr, Instruction value)
	{
		if (instr.MatchBge(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBgeUn(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Bge_Un || instr.OpCode == OpCodes.Bge_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBgeUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBgeUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBgeUn(this Instruction instr, Instruction value)
	{
		if (instr.MatchBgeUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBgt(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Bgt || instr.OpCode == OpCodes.Bgt_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBgt(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBgt(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBgt(this Instruction instr, Instruction value)
	{
		if (instr.MatchBgt(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBgtUn(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Bgt_Un || instr.OpCode == OpCodes.Bgt_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBgtUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBgtUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBgtUn(this Instruction instr, Instruction value)
	{
		if (instr.MatchBgtUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBle(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ble || instr.OpCode == OpCodes.Ble_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBle(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBle(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBle(this Instruction instr, Instruction value)
	{
		if (instr.MatchBle(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBleUn(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ble_Un || instr.OpCode == OpCodes.Ble_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBleUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBleUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBleUn(this Instruction instr, Instruction value)
	{
		if (instr.MatchBleUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBlt(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Blt || instr.OpCode == OpCodes.Blt_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBlt(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBlt(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBlt(this Instruction instr, Instruction value)
	{
		if (instr.MatchBlt(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBltUn(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Blt_Un || instr.OpCode == OpCodes.Blt_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBltUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBltUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBltUn(this Instruction instr, Instruction value)
	{
		if (instr.MatchBltUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBneUn(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Bne_Un || instr.OpCode == OpCodes.Bne_Un_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBneUn(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBneUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBneUn(this Instruction instr, Instruction value)
	{
		if (instr.MatchBneUn(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBox(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Box)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBox(this Instruction instr, TypeReference value)
	{
		if (instr.MatchBox(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBox(this Instruction instr, Type value)
	{
		if (instr.MatchBox(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBox<T>(this Instruction instr)
	{
		return instr.MatchBox(typeof(T));
	}

	public static bool MatchBox(this Instruction instr, string typeFullName)
	{
		if (instr.MatchBox(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchBr(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Br || instr.OpCode == OpCodes.Br_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBr(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBr(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBr(this Instruction instr, Instruction value)
	{
		if (instr.MatchBr(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBreak(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Break;
	}

	public static bool MatchBrfalse(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Brfalse || instr.OpCode == OpCodes.Brfalse_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBrfalse(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBrfalse(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBrfalse(this Instruction instr, Instruction value)
	{
		if (instr.MatchBrfalse(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBrtrue(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Brtrue || instr.OpCode == OpCodes.Brtrue_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchBrtrue(this Instruction instr, ILLabel value)
	{
		if (instr.MatchBrtrue(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchBrtrue(this Instruction instr, Instruction value)
	{
		if (instr.MatchBrtrue(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCall(this Instruction instr, [MaybeNullWhen(false)] out MethodReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Call)
		{
			value = (MethodReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCall(this Instruction instr, MethodReference value)
	{
		if (instr.MatchCall(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCall(this Instruction instr, MethodBase value)
	{
		if (instr.MatchCall(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCall(this Instruction instr, Type type, string name)
	{
		if (instr.MatchCall(out MethodReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchCall<T>(this Instruction instr, string name)
	{
		return instr.MatchCall(typeof(T), name);
	}

	public static bool MatchCall(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchCall(out MethodReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchCalli(this Instruction instr, [MaybeNullWhen(false)] out IMethodSignature value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Calli)
		{
			value = (IMethodSignature)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCalli(this Instruction instr, IMethodSignature value)
	{
		if (instr.MatchCalli(out IMethodSignature value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCallvirt(this Instruction instr, [MaybeNullWhen(false)] out MethodReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Callvirt)
		{
			value = (MethodReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCallvirt(this Instruction instr, MethodReference value)
	{
		if (instr.MatchCallvirt(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCallvirt(this Instruction instr, MethodBase value)
	{
		if (instr.MatchCallvirt(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCallvirt(this Instruction instr, Type type, string name)
	{
		if (instr.MatchCallvirt(out MethodReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchCallvirt<T>(this Instruction instr, string name)
	{
		return instr.MatchCallvirt(typeof(T), name);
	}

	public static bool MatchCallvirt(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchCallvirt(out MethodReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, MethodReference value)
	{
		if (instr.MatchCallOrCallvirt(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, MethodBase value)
	{
		if (instr.MatchCallOrCallvirt(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, Type type, string name)
	{
		if (instr.MatchCallOrCallvirt(out MethodReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchCallOrCallvirt<T>(this Instruction instr, string name)
	{
		return instr.MatchCallOrCallvirt(typeof(T), name);
	}

	public static bool MatchCallOrCallvirt(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchCallOrCallvirt(out MethodReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchCastclass(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Castclass)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCastclass(this Instruction instr, TypeReference value)
	{
		if (instr.MatchCastclass(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCastclass(this Instruction instr, Type value)
	{
		if (instr.MatchCastclass(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCastclass<T>(this Instruction instr)
	{
		return instr.MatchCastclass(typeof(T));
	}

	public static bool MatchCastclass(this Instruction instr, string typeFullName)
	{
		if (instr.MatchCastclass(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchCeq(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ceq;
	}

	public static bool MatchCgt(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Cgt;
	}

	public static bool MatchCgtUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Cgt_Un;
	}

	public static bool MatchCkfinite(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ckfinite;
	}

	public static bool MatchClt(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Clt;
	}

	public static bool MatchCltUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Clt_Un;
	}

	public static bool MatchConstrained(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Constrained)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchConstrained(this Instruction instr, TypeReference value)
	{
		if (instr.MatchConstrained(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchConstrained(this Instruction instr, Type value)
	{
		if (instr.MatchConstrained(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchConstrained<T>(this Instruction instr)
	{
		return instr.MatchConstrained(typeof(T));
	}

	public static bool MatchConstrained(this Instruction instr, string typeFullName)
	{
		if (instr.MatchConstrained(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchConvI(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_I;
	}

	public static bool MatchConvI1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_I1;
	}

	public static bool MatchConvI2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_I2;
	}

	public static bool MatchConvI4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_I4;
	}

	public static bool MatchConvI8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_I8;
	}

	public static bool MatchConvOvfI(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I;
	}

	public static bool MatchConvOvfIUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I_Un;
	}

	public static bool MatchConvOvfI1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I1;
	}

	public static bool MatchConvOvfI1Un(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I1_Un;
	}

	public static bool MatchConvOvfI2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I2;
	}

	public static bool MatchConvOvfI2Un(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I2_Un;
	}

	public static bool MatchConvOvfI4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I4;
	}

	public static bool MatchConvOvfI4Un(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I4_Un;
	}

	public static bool MatchConvOvfI8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I8;
	}

	public static bool MatchConvOvfI8Un(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_I8_Un;
	}

	public static bool MatchConvOvfU(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U;
	}

	public static bool MatchConvOvfUUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U_Un;
	}

	public static bool MatchConvOvfU1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U1;
	}

	public static bool MatchConvOvfU1Un(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U1_Un;
	}

	public static bool MatchConvOvfU2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U2;
	}

	public static bool MatchConvOvfU2Un(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U2_Un;
	}

	public static bool MatchConvOvfU4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U4;
	}

	public static bool MatchConvOvfU4Un(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U4_Un;
	}

	public static bool MatchConvOvfU8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U8;
	}

	public static bool MatchConvOvfU8Un(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_Ovf_U8_Un;
	}

	public static bool MatchConvRUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_R_Un;
	}

	public static bool MatchConvR4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_R4;
	}

	public static bool MatchConvR8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_R8;
	}

	public static bool MatchConvU(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_U;
	}

	public static bool MatchConvU1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_U1;
	}

	public static bool MatchConvU2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_U2;
	}

	public static bool MatchConvU4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_U4;
	}

	public static bool MatchConvU8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Conv_U8;
	}

	public static bool MatchCpblk(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Cpblk;
	}

	public static bool MatchCpobj(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Cpobj)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchCpobj(this Instruction instr, TypeReference value)
	{
		if (instr.MatchCpobj(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCpobj(this Instruction instr, Type value)
	{
		if (instr.MatchCpobj(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchCpobj<T>(this Instruction instr)
	{
		return instr.MatchCpobj(typeof(T));
	}

	public static bool MatchCpobj(this Instruction instr, string typeFullName)
	{
		if (instr.MatchCpobj(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchDiv(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Div;
	}

	public static bool MatchDivUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Div_Un;
	}

	public static bool MatchDup(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Dup;
	}

	public static bool MatchEndfilter(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Endfilter;
	}

	public static bool MatchEndfinally(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Endfinally;
	}

	public static bool MatchInitblk(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Initblk;
	}

	public static bool MatchInitobj(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Initobj)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchInitobj(this Instruction instr, TypeReference value)
	{
		if (instr.MatchInitobj(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchInitobj(this Instruction instr, Type value)
	{
		if (instr.MatchInitobj(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchInitobj<T>(this Instruction instr)
	{
		return instr.MatchInitobj(typeof(T));
	}

	public static bool MatchInitobj(this Instruction instr, string typeFullName)
	{
		if (instr.MatchInitobj(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchIsinst(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Isinst)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchIsinst(this Instruction instr, TypeReference value)
	{
		if (instr.MatchIsinst(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchIsinst(this Instruction instr, Type value)
	{
		if (instr.MatchIsinst(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchIsinst<T>(this Instruction instr)
	{
		return instr.MatchIsinst(typeof(T));
	}

	public static bool MatchIsinst(this Instruction instr, string typeFullName)
	{
		if (instr.MatchIsinst(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchJmp(this Instruction instr, [MaybeNullWhen(false)] out MethodReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Jmp)
		{
			value = (MethodReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchJmp(this Instruction instr, MethodReference value)
	{
		if (instr.MatchJmp(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchJmp(this Instruction instr, MethodBase value)
	{
		if (instr.MatchJmp(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchJmp(this Instruction instr, Type type, string name)
	{
		if (instr.MatchJmp(out MethodReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchJmp<T>(this Instruction instr, string name)
	{
		return instr.MatchJmp(typeof(T), name);
	}

	public static bool MatchJmp(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchJmp(out MethodReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdarg0(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldarg_0;
	}

	public static bool MatchLdarg1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldarg_1;
	}

	public static bool MatchLdarg2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldarg_2;
	}

	public static bool MatchLdarg3(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldarg_3;
	}

	public static bool MatchLdarg(this Instruction instr, int value)
	{
		if (instr.MatchLdarg(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdarg(this Instruction instr, uint value)
	{
		if (instr.MatchLdarg(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdarga(this Instruction instr, int value)
	{
		if (instr.MatchLdarga(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdarga(this Instruction instr, uint value)
	{
		if (instr.MatchLdarga(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdcI4(this Instruction instr, int value)
	{
		if (instr.MatchLdcI4(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdcI4(this Instruction instr, uint value)
	{
		if (instr.MatchLdcI4(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdcI8(this Instruction instr, [MaybeNullWhen(false)] out long value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldc_I8)
		{
			value = (long)instr.Operand;
			return true;
		}
		value = 0L;
		return false;
	}

	public static bool MatchLdcI8(this Instruction instr, long value)
	{
		if (instr.MatchLdcI8(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdcI8(this Instruction instr, ulong value)
	{
		if (instr.MatchLdcI8(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdcR4(this Instruction instr, [MaybeNullWhen(false)] out float value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldc_R4)
		{
			value = (float)instr.Operand;
			return true;
		}
		value = 0f;
		return false;
	}

	public static bool MatchLdcR4(this Instruction instr, float value)
	{
		if (instr.MatchLdcR4(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdcR8(this Instruction instr, [MaybeNullWhen(false)] out double value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldc_R8)
		{
			value = (double)instr.Operand;
			return true;
		}
		value = 0.0;
		return false;
	}

	public static bool MatchLdcR8(this Instruction instr, double value)
	{
		if (instr.MatchLdcR8(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdelemAny(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_Any)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdelemAny(this Instruction instr, TypeReference value)
	{
		if (instr.MatchLdelemAny(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdelemAny(this Instruction instr, Type value)
	{
		if (instr.MatchLdelemAny(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdelemAny<T>(this Instruction instr)
	{
		return instr.MatchLdelemAny(typeof(T));
	}

	public static bool MatchLdelemAny(this Instruction instr, string typeFullName)
	{
		if (instr.MatchLdelemAny(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchLdelemI(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_I;
	}

	public static bool MatchLdelemI1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_I1;
	}

	public static bool MatchLdelemI2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_I2;
	}

	public static bool MatchLdelemI4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_I4;
	}

	public static bool MatchLdelemI8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_I8;
	}

	public static bool MatchLdelemR4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_R4;
	}

	public static bool MatchLdelemR8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_R8;
	}

	public static bool MatchLdelemRef(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_Ref;
	}

	public static bool MatchLdelemU1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_U1;
	}

	public static bool MatchLdelemU2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_U2;
	}

	public static bool MatchLdelemU4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelem_U4;
	}

	public static bool MatchLdelema(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldelema)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdelema(this Instruction instr, TypeReference value)
	{
		if (instr.MatchLdelema(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdelema(this Instruction instr, Type value)
	{
		if (instr.MatchLdelema(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdelema<T>(this Instruction instr)
	{
		return instr.MatchLdelema(typeof(T));
	}

	public static bool MatchLdelema(this Instruction instr, string typeFullName)
	{
		if (instr.MatchLdelema(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchLdfld(this Instruction instr, [MaybeNullWhen(false)] out FieldReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldfld)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdfld(this Instruction instr, FieldReference value)
	{
		if (instr.MatchLdfld(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdfld(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchLdfld(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdfld(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdfld(out FieldReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchLdfld<T>(this Instruction instr, string name)
	{
		return instr.MatchLdfld(typeof(T), name);
	}

	public static bool MatchLdfld(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdfld(out FieldReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdflda(this Instruction instr, [MaybeNullWhen(false)] out FieldReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldflda)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdflda(this Instruction instr, FieldReference value)
	{
		if (instr.MatchLdflda(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdflda(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchLdflda(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdflda(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdflda(out FieldReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchLdflda<T>(this Instruction instr, string name)
	{
		return instr.MatchLdflda(typeof(T), name);
	}

	public static bool MatchLdflda(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdflda(out FieldReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdftn(this Instruction instr, [MaybeNullWhen(false)] out MethodReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldftn)
		{
			value = (MethodReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdftn(this Instruction instr, MethodReference value)
	{
		if (instr.MatchLdftn(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdftn(this Instruction instr, MethodBase value)
	{
		if (instr.MatchLdftn(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdftn(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdftn(out MethodReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchLdftn<T>(this Instruction instr, string name)
	{
		return instr.MatchLdftn(typeof(T), name);
	}

	public static bool MatchLdftn(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdftn(out MethodReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdindI(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_I;
	}

	public static bool MatchLdindI1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_I1;
	}

	public static bool MatchLdindI2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_I2;
	}

	public static bool MatchLdindI4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_I4;
	}

	public static bool MatchLdindI8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_I8;
	}

	public static bool MatchLdindR4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_R4;
	}

	public static bool MatchLdindR8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_R8;
	}

	public static bool MatchLdindRef(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_Ref;
	}

	public static bool MatchLdindU1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_U1;
	}

	public static bool MatchLdindU2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_U2;
	}

	public static bool MatchLdindU4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldind_U4;
	}

	public static bool MatchLdlen(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldlen;
	}

	public static bool MatchLdloc0(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldloc_0;
	}

	public static bool MatchLdloc1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldloc_1;
	}

	public static bool MatchLdloc2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldloc_2;
	}

	public static bool MatchLdloc3(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldloc_3;
	}

	public static bool MatchLdloc(this Instruction instr, int value)
	{
		if (instr.MatchLdloc(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdloc(this Instruction instr, uint value)
	{
		if (instr.MatchLdloc(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdloca(this Instruction instr, int value)
	{
		if (instr.MatchLdloca(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdloca(this Instruction instr, uint value)
	{
		if (instr.MatchLdloca(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdnull(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldnull;
	}

	public static bool MatchLdobj(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldobj)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdobj(this Instruction instr, TypeReference value)
	{
		if (instr.MatchLdobj(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdobj(this Instruction instr, Type value)
	{
		if (instr.MatchLdobj(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdobj<T>(this Instruction instr)
	{
		return instr.MatchLdobj(typeof(T));
	}

	public static bool MatchLdobj(this Instruction instr, string typeFullName)
	{
		if (instr.MatchLdobj(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchLdsfld(this Instruction instr, [MaybeNullWhen(false)] out FieldReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldsfld)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdsfld(this Instruction instr, FieldReference value)
	{
		if (instr.MatchLdsfld(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdsfld(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchLdsfld(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdsfld(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdsfld(out FieldReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchLdsfld<T>(this Instruction instr, string name)
	{
		return instr.MatchLdsfld(typeof(T), name);
	}

	public static bool MatchLdsfld(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdsfld(out FieldReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdsflda(this Instruction instr, [MaybeNullWhen(false)] out FieldReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldsflda)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdsflda(this Instruction instr, FieldReference value)
	{
		if (instr.MatchLdsflda(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdsflda(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchLdsflda(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdsflda(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdsflda(out FieldReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchLdsflda<T>(this Instruction instr, string name)
	{
		return instr.MatchLdsflda(typeof(T), name);
	}

	public static bool MatchLdsflda(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdsflda(out FieldReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLdstr(this Instruction instr, [MaybeNullWhen(false)] out string value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldstr)
		{
			value = (string)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdstr(this Instruction instr, string value)
	{
		if (instr.MatchLdstr(out string value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdtoken(this Instruction instr, [MaybeNullWhen(false)] out IMetadataTokenProvider value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldtoken)
		{
			value = (IMetadataTokenProvider)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdtoken(this Instruction instr, IMetadataTokenProvider value)
	{
		if (instr.MatchLdtoken(out IMetadataTokenProvider value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdtoken(this Instruction instr, Type value)
	{
		if (instr.MatchLdtoken(out IMetadataTokenProvider value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdtoken<T>(this Instruction instr)
	{
		return instr.MatchLdtoken(typeof(T));
	}

	public static bool MatchLdtoken(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchLdtoken(out IMetadataTokenProvider value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdtoken(this Instruction instr, MethodBase value)
	{
		if (instr.MatchLdtoken(out IMetadataTokenProvider value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdvirtftn(this Instruction instr, [MaybeNullWhen(false)] out MethodReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ldvirtftn)
		{
			value = (MethodReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLdvirtftn(this Instruction instr, MethodReference value)
	{
		if (instr.MatchLdvirtftn(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdvirtftn(this Instruction instr, MethodBase value)
	{
		if (instr.MatchLdvirtftn(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLdvirtftn(this Instruction instr, Type type, string name)
	{
		if (instr.MatchLdvirtftn(out MethodReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchLdvirtftn<T>(this Instruction instr, string name)
	{
		return instr.MatchLdvirtftn(typeof(T), name);
	}

	public static bool MatchLdvirtftn(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchLdvirtftn(out MethodReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchLeave(this Instruction instr, [MaybeNullWhen(false)] out ILLabel value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Leave || instr.OpCode == OpCodes.Leave_S)
		{
			value = (ILLabel)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchLeave(this Instruction instr, ILLabel value)
	{
		if (instr.MatchLeave(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLeave(this Instruction instr, Instruction value)
	{
		if (instr.MatchLeave(out ILLabel value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchLocalloc(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Localloc;
	}

	public static bool MatchMkrefany(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Mkrefany)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchMkrefany(this Instruction instr, TypeReference value)
	{
		if (instr.MatchMkrefany(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchMkrefany(this Instruction instr, Type value)
	{
		if (instr.MatchMkrefany(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchMkrefany<T>(this Instruction instr)
	{
		return instr.MatchMkrefany(typeof(T));
	}

	public static bool MatchMkrefany(this Instruction instr, string typeFullName)
	{
		if (instr.MatchMkrefany(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchMul(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Mul;
	}

	public static bool MatchMulOvf(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Mul_Ovf;
	}

	public static bool MatchMulOvfUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Mul_Ovf_Un;
	}

	public static bool MatchNeg(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Neg;
	}

	public static bool MatchNewarr(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Newarr)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchNewarr(this Instruction instr, TypeReference value)
	{
		if (instr.MatchNewarr(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchNewarr(this Instruction instr, Type value)
	{
		if (instr.MatchNewarr(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchNewarr<T>(this Instruction instr)
	{
		return instr.MatchNewarr(typeof(T));
	}

	public static bool MatchNewarr(this Instruction instr, string typeFullName)
	{
		if (instr.MatchNewarr(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, [MaybeNullWhen(false)] out MethodReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Newobj)
		{
			value = (MethodReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, MethodReference value)
	{
		if (instr.MatchNewobj(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, MethodBase value)
	{
		if (instr.MatchNewobj(out MethodReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchNewobj(this Instruction instr, Type type, string name)
	{
		if (instr.MatchNewobj(out MethodReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchNewobj<T>(this Instruction instr, string name)
	{
		return instr.MatchNewobj(typeof(T), name);
	}

	public static bool MatchNewobj(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchNewobj(out MethodReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchNop(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Nop;
	}

	public static bool MatchNot(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Not;
	}

	public static bool MatchOr(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Or;
	}

	public static bool MatchPop(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Pop;
	}

	public static bool MatchReadonly(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Readonly;
	}

	public static bool MatchRefanytype(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Refanytype;
	}

	public static bool MatchRefanyval(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Refanyval)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchRefanyval(this Instruction instr, TypeReference value)
	{
		if (instr.MatchRefanyval(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchRefanyval(this Instruction instr, Type value)
	{
		if (instr.MatchRefanyval(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchRefanyval<T>(this Instruction instr)
	{
		return instr.MatchRefanyval(typeof(T));
	}

	public static bool MatchRefanyval(this Instruction instr, string typeFullName)
	{
		if (instr.MatchRefanyval(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchRem(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Rem;
	}

	public static bool MatchRemUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Rem_Un;
	}

	public static bool MatchRet(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Ret;
	}

	public static bool MatchRethrow(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Rethrow;
	}

	public static bool MatchShl(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Shl;
	}

	public static bool MatchShr(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Shr;
	}

	public static bool MatchShrUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Shr_Un;
	}

	public static bool MatchSizeof(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Sizeof)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchSizeof(this Instruction instr, TypeReference value)
	{
		if (instr.MatchSizeof(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchSizeof(this Instruction instr, Type value)
	{
		if (instr.MatchSizeof(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchSizeof<T>(this Instruction instr)
	{
		return instr.MatchSizeof(typeof(T));
	}

	public static bool MatchSizeof(this Instruction instr, string typeFullName)
	{
		if (instr.MatchSizeof(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchStarg(this Instruction instr, int value)
	{
		if (instr.MatchStarg(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStarg(this Instruction instr, uint value)
	{
		if (instr.MatchStarg(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStelemAny(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stelem_Any)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchStelemAny(this Instruction instr, TypeReference value)
	{
		if (instr.MatchStelemAny(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStelemAny(this Instruction instr, Type value)
	{
		if (instr.MatchStelemAny(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStelemAny<T>(this Instruction instr)
	{
		return instr.MatchStelemAny(typeof(T));
	}

	public static bool MatchStelemAny(this Instruction instr, string typeFullName)
	{
		if (instr.MatchStelemAny(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchStelemI(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stelem_I;
	}

	public static bool MatchStelemI1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stelem_I1;
	}

	public static bool MatchStelemI2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stelem_I2;
	}

	public static bool MatchStelemI4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stelem_I4;
	}

	public static bool MatchStelemI8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stelem_I8;
	}

	public static bool MatchStelemR4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stelem_R4;
	}

	public static bool MatchStelemR8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stelem_R8;
	}

	public static bool MatchStelemRef(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stelem_Ref;
	}

	public static bool MatchStfld(this Instruction instr, [MaybeNullWhen(false)] out FieldReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stfld)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchStfld(this Instruction instr, FieldReference value)
	{
		if (instr.MatchStfld(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStfld(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchStfld(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStfld(this Instruction instr, Type type, string name)
	{
		if (instr.MatchStfld(out FieldReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchStfld<T>(this Instruction instr, string name)
	{
		return instr.MatchStfld(typeof(T), name);
	}

	public static bool MatchStfld(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchStfld(out FieldReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchStindI(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stind_I;
	}

	public static bool MatchStindI1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stind_I1;
	}

	public static bool MatchStindI2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stind_I2;
	}

	public static bool MatchStindI4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stind_I4;
	}

	public static bool MatchStindI8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stind_I8;
	}

	public static bool MatchStindR4(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stind_R4;
	}

	public static bool MatchStindR8(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stind_R8;
	}

	public static bool MatchStindRef(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stind_Ref;
	}

	public static bool MatchStloc0(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stloc_0;
	}

	public static bool MatchStloc1(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stloc_1;
	}

	public static bool MatchStloc2(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stloc_2;
	}

	public static bool MatchStloc3(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stloc_3;
	}

	public static bool MatchStloc(this Instruction instr, int value)
	{
		if (instr.MatchStloc(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStloc(this Instruction instr, uint value)
	{
		if (instr.MatchStloc(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStobj(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stobj)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchStobj(this Instruction instr, TypeReference value)
	{
		if (instr.MatchStobj(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStobj(this Instruction instr, Type value)
	{
		if (instr.MatchStobj(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStobj<T>(this Instruction instr)
	{
		return instr.MatchStobj(typeof(T));
	}

	public static bool MatchStobj(this Instruction instr, string typeFullName)
	{
		if (instr.MatchStobj(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchStsfld(this Instruction instr, [MaybeNullWhen(false)] out FieldReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Stsfld)
		{
			value = (FieldReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchStsfld(this Instruction instr, FieldReference value)
	{
		if (instr.MatchStsfld(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStsfld(this Instruction instr, FieldInfo value)
	{
		if (instr.MatchStsfld(out FieldReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchStsfld(this Instruction instr, Type type, string name)
	{
		if (instr.MatchStsfld(out FieldReference value))
		{
			return IsEquivalent(value, type, name);
		}
		return false;
	}

	public static bool MatchStsfld<T>(this Instruction instr, string name)
	{
		return instr.MatchStsfld(typeof(T), name);
	}

	public static bool MatchStsfld(this Instruction instr, string typeFullName, string name)
	{
		if (instr.MatchStsfld(out FieldReference value))
		{
			return value.Is(typeFullName, name);
		}
		return false;
	}

	public static bool MatchSub(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Sub;
	}

	public static bool MatchSubOvf(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Sub_Ovf;
	}

	public static bool MatchSubOvfUn(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Sub_Ovf_Un;
	}

	public static bool MatchSwitch(this Instruction instr, [MaybeNullWhen(false)] out ILLabel[] value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Switch)
		{
			value = (ILLabel[])instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchSwitch(this Instruction instr, ILLabel[] value)
	{
		if (instr.MatchSwitch(out ILLabel[] value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchSwitch(this Instruction instr, Instruction[] value)
	{
		if (instr.MatchSwitch(out ILLabel[] value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchTail(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Tail;
	}

	public static bool MatchThrow(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Throw;
	}

	public static bool MatchUnaligned(this Instruction instr, [MaybeNullWhen(false)] out byte value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Unaligned)
		{
			value = (byte)instr.Operand;
			return true;
		}
		value = 0;
		return false;
	}

	public static bool MatchUnaligned(this Instruction instr, byte value)
	{
		if (instr.MatchUnaligned(out var value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchUnbox(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Unbox)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchUnbox(this Instruction instr, TypeReference value)
	{
		if (instr.MatchUnbox(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchUnbox(this Instruction instr, Type value)
	{
		if (instr.MatchUnbox(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchUnbox<T>(this Instruction instr)
	{
		return instr.MatchUnbox(typeof(T));
	}

	public static bool MatchUnbox(this Instruction instr, string typeFullName)
	{
		if (instr.MatchUnbox(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchUnboxAny(this Instruction instr, [MaybeNullWhen(false)] out TypeReference value)
	{
		if (Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Unbox_Any)
		{
			value = (TypeReference)instr.Operand;
			return true;
		}
		value = null;
		return false;
	}

	public static bool MatchUnboxAny(this Instruction instr, TypeReference value)
	{
		if (instr.MatchUnboxAny(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchUnboxAny(this Instruction instr, Type value)
	{
		if (instr.MatchUnboxAny(out TypeReference value2))
		{
			return IsEquivalent(value2, value);
		}
		return false;
	}

	public static bool MatchUnboxAny<T>(this Instruction instr)
	{
		return instr.MatchUnboxAny(typeof(T));
	}

	public static bool MatchUnboxAny(this Instruction instr, string typeFullName)
	{
		if (instr.MatchUnboxAny(out TypeReference value))
		{
			return value.Is(typeFullName);
		}
		return false;
	}

	public static bool MatchVolatile(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Volatile;
	}

	public static bool MatchXor(this Instruction instr)
	{
		return Helpers.ThrowIfNull(instr, "instr").OpCode == OpCodes.Xor;
	}
}
