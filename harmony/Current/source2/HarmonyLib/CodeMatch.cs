using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

public class CodeMatch : CodeInstruction
{
	public string name;

	public HashSet<OpCode> opcodeSet = new HashSet<OpCode>();

	public List<object> operands = new List<object>();

	public List<int> jumpsFrom = new List<int>();

	public List<int> jumpsTo = new List<int>();

	public Func<CodeInstruction, bool> predicate;

	[Obsolete("Use opcodeSet instead")]
	public List<OpCode> opcodes
	{
		get
		{
			HashSet<OpCode> hashSet = opcodeSet;
			List<OpCode> list = new List<OpCode>(hashSet.Count);
			list.AddRange(hashSet);
			return list;
		}
		set
		{
			opcodeSet = new HashSet<OpCode>(value);
		}
	}

	internal CodeMatch Set(object operand, string name)
	{
		if (base.operand == null)
		{
			base.operand = operand;
		}
		if (operand != null)
		{
			operands.Add(operand);
		}
		if (this.name == null)
		{
			this.name = name;
		}
		return this;
	}

	internal CodeMatch Set(OpCode opcode, object operand, string name)
	{
		base.opcode = opcode;
		opcodeSet.Add(opcode);
		if (base.operand == null)
		{
			base.operand = operand;
		}
		if (operand != null)
		{
			operands.Add(operand);
		}
		if (this.name == null)
		{
			this.name = name;
		}
		return this;
	}

	public CodeMatch(OpCode? opcode = null, object operand = null, string name = null)
	{
		if (opcode.HasValue)
		{
			OpCode item = (base.opcode = opcode.GetValueOrDefault());
			opcodeSet.Add(item);
		}
		if (operand != null)
		{
			operands.Add(operand);
		}
		base.operand = operand;
		this.name = name;
	}

	public static CodeMatch WithOpcodes(HashSet<OpCode> opcodes, object operand = null, string name = null)
	{
		return new CodeMatch(null, operand, name)
		{
			opcodeSet = opcodes
		};
	}

	public CodeMatch(Expression<Action> expression, string name = null)
	{
		opcodeSet.UnionWith(CodeInstructionExtensions.opcodesCalling);
		operand = SymbolExtensions.GetMethodInfo(expression);
		if (operand != null)
		{
			operands.Add(operand);
		}
		this.name = name;
	}

	public CodeMatch(LambdaExpression expression, string name = null)
	{
		opcodeSet.UnionWith(CodeInstructionExtensions.opcodesCalling);
		operand = SymbolExtensions.GetMethodInfo(expression);
		if (operand != null)
		{
			operands.Add(operand);
		}
		this.name = name;
	}

	public CodeMatch(CodeInstruction instruction, string name = null)
		: this(instruction.opcode, instruction.operand, name)
	{
	}

	public CodeMatch(Func<CodeInstruction, bool> predicate, string name = null)
	{
		this.predicate = predicate;
		this.name = name;
	}

	internal bool Matches(List<CodeInstruction> codes, CodeInstruction instruction)
	{
		if (predicate != null)
		{
			return predicate(instruction);
		}
		if (opcodeSet.Count > 0 && !opcodeSet.Contains(instruction.opcode))
		{
			return false;
		}
		if (operands.Count > 0 && !operands.Contains(instruction.operand))
		{
			return false;
		}
		if (labels.Count > 0 && !labels.Intersect(instruction.labels).Any())
		{
			return false;
		}
		if (blocks.Count > 0 && !blocks.Intersect(instruction.blocks).Any())
		{
			return false;
		}
		if (jumpsFrom.Count > 0 && !jumpsFrom.Select((int index) => codes[index].operand).OfType<Label>().Intersect(instruction.labels)
			.Any())
		{
			return false;
		}
		if (jumpsTo.Count > 0)
		{
			object obj = instruction.operand;
			if (obj == null || obj.GetType() != typeof(Label))
			{
				return false;
			}
			Label label = (Label)obj;
			IEnumerable<int> second = from idx in Enumerable.Range(0, codes.Count)
				where codes[idx].labels.Contains(label)
				select idx;
			if (!jumpsTo.Intersect(second).Any())
			{
				return false;
			}
		}
		return true;
	}

	public static CodeMatch IsLdarg(int? n = null)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.IsLdarg(n));
	}

	public static CodeMatch IsLdarga(int? n = null)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.IsLdarga(n));
	}

	public static CodeMatch IsStarg(int? n = null)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.IsStarg(n));
	}

	public static CodeMatch IsLdloc(LocalBuilder variable = null)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.IsLdloc(variable));
	}

	public static CodeMatch IsStloc(LocalBuilder variable = null)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.IsStloc(variable));
	}

	public static CodeMatch Calls(MethodInfo method)
	{
		return WithOpcodes(CodeInstructionExtensions.opcodesCalling, method);
	}

	public static CodeMatch LoadsConstant()
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.LoadsConstant());
	}

	public static CodeMatch LoadsConstant(long number)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.LoadsConstant(number));
	}

	public static CodeMatch LoadsConstant(double number)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.LoadsConstant(number));
	}

	public static CodeMatch LoadsConstant(Enum e)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.LoadsConstant(e));
	}

	public static CodeMatch LoadsConstant(string str)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.LoadsConstant(str));
	}

	public static CodeMatch LoadsField(FieldInfo field, bool byAddress = false)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.LoadsField(field, byAddress));
	}

	public static CodeMatch StoresField(FieldInfo field)
	{
		return new CodeMatch((CodeInstruction instruction) => instruction.StoresField(field));
	}

	public static CodeMatch Calls(Expression<Action> expression)
	{
		return new CodeMatch(expression);
	}

	public static CodeMatch Calls(LambdaExpression expression)
	{
		return new CodeMatch(expression);
	}

	public static CodeMatch LoadsLocal(bool useAddress = false, string name = null)
	{
		return WithOpcodes(useAddress ? CodeInstructionExtensions.opcodesLoadingLocalByAddress : CodeInstructionExtensions.opcodesLoadingLocalNormal, null, name);
	}

	public static CodeMatch StoresLocal(string name = null)
	{
		return WithOpcodes(CodeInstructionExtensions.opcodesStoringLocal, null, name);
	}

	public static CodeMatch LoadsArgument(bool useAddress = false, string name = null)
	{
		return WithOpcodes(useAddress ? CodeInstructionExtensions.opcodesLoadingArgumentByAddress : CodeInstructionExtensions.opcodesLoadingArgumentNormal, null, name);
	}

	public static CodeMatch StoresArgument(string name = null)
	{
		return WithOpcodes(CodeInstructionExtensions.opcodesStoringArgument, null, name);
	}

	public static CodeMatch Branches(string name = null)
	{
		return WithOpcodes(CodeInstructionExtensions.opcodesBranching, null, name);
	}

	public override string ToString()
	{
		string text = "[";
		if (name != null)
		{
			text = text + name + ": ";
		}
		if (opcodeSet.Count > 0)
		{
			text = text + "opcodes=" + opcodeSet.Join() + " ";
		}
		if (operands.Count > 0)
		{
			text = text + "operands=" + operands.Join() + " ";
		}
		if (labels.Count > 0)
		{
			text = text + "labels=" + labels.Join() + " ";
		}
		if (blocks.Count > 0)
		{
			text = text + "blocks=" + blocks.Join() + " ";
		}
		if (jumpsFrom.Count > 0)
		{
			text = text + "jumpsFrom=" + jumpsFrom.Join() + " ";
		}
		if (jumpsTo.Count > 0)
		{
			text = text + "jumpsTo=" + jumpsTo.Join() + " ";
		}
		if (predicate != null)
		{
			text += "predicate=yes ";
		}
		return text.TrimEnd() + "]";
	}
}
