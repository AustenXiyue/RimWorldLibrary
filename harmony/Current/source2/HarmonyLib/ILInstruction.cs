using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace HarmonyLib;

internal class ILInstruction
{
	internal int offset;

	internal OpCode opcode;

	internal object operand;

	internal object argument;

	internal List<Label> labels = new List<Label>();

	internal List<ExceptionBlock> blocks = new List<ExceptionBlock>();

	internal ILInstruction(OpCode opcode, object operand = null)
	{
		this.opcode = opcode;
		this.operand = operand;
		argument = operand;
	}

	internal CodeInstruction GetCodeInstruction()
	{
		CodeInstruction codeInstruction = new CodeInstruction(opcode, argument);
		if (opcode.OperandType == OperandType.InlineNone)
		{
			codeInstruction.operand = null;
		}
		codeInstruction.labels = labels;
		codeInstruction.blocks = blocks;
		return codeInstruction;
	}

	internal int GetSize()
	{
		int num = opcode.Size;
		switch (opcode.OperandType)
		{
		case OperandType.InlineSwitch:
			num += (1 + ((Array)operand).Length) * 4;
			break;
		case OperandType.InlineI8:
		case OperandType.InlineR:
			num += 8;
			break;
		case OperandType.InlineBrTarget:
		case OperandType.InlineField:
		case OperandType.InlineI:
		case OperandType.InlineMethod:
		case OperandType.InlineSig:
		case OperandType.InlineString:
		case OperandType.InlineTok:
		case OperandType.InlineType:
		case OperandType.ShortInlineR:
			num += 4;
			break;
		case OperandType.InlineVar:
			num += 2;
			break;
		case OperandType.ShortInlineBrTarget:
		case OperandType.ShortInlineI:
		case OperandType.ShortInlineVar:
			num++;
			break;
		}
		return num;
	}

	public override string ToString()
	{
		string str = "";
		AppendLabel(ref str, this);
		str = str + ": " + opcode.Name;
		if (operand == null)
		{
			return str;
		}
		str += " ";
		switch (opcode.OperandType)
		{
		case OperandType.InlineBrTarget:
		case OperandType.ShortInlineBrTarget:
			AppendLabel(ref str, operand);
			break;
		case OperandType.InlineSwitch:
		{
			ILInstruction[] array = (ILInstruction[])operand;
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					str += ",";
				}
				AppendLabel(ref str, array[i]);
			}
			break;
		}
		case OperandType.InlineString:
			str += $"\"{operand}\"";
			break;
		default:
			str += operand;
			break;
		}
		return str;
	}

	private static void AppendLabel(ref string str, object argument)
	{
		ILInstruction iLInstruction = argument as ILInstruction;
		str += $"IL_{iLInstruction?.offset.ToString("X4") ?? argument}";
	}
}
