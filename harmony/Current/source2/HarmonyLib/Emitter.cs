using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;
using MonoMod.Utils.Cil;

namespace HarmonyLib;

internal class Emitter
{
	private readonly CecilILGenerator il;

	private readonly Dictionary<int, CodeInstruction> instructions = new Dictionary<int, CodeInstruction>();

	private readonly bool debug;

	internal Emitter(ILGenerator il, bool debug)
	{
		this.il = il.GetProxiedShim<CecilILGenerator>();
		this.debug = debug;
	}

	internal Dictionary<int, CodeInstruction> GetInstructions()
	{
		return instructions;
	}

	internal void AddInstruction(System.Reflection.Emit.OpCode opcode, object operand)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, operand));
	}

	internal int CurrentPos()
	{
		return il.ILOffset;
	}

	internal static string CodePos(int offset)
	{
		return $"IL_{offset:X4}: ";
	}

	internal string CodePos()
	{
		return CodePos(CurrentPos());
	}

	internal void LogComment(string comment)
	{
		if (debug)
		{
			string str = $"{CodePos()}// {comment}";
			FileLog.LogBuffered(str);
		}
	}

	internal void LogIL(System.Reflection.Emit.OpCode opcode)
	{
		if (debug)
		{
			FileLog.LogBuffered($"{CodePos()}{opcode}");
		}
	}

	internal void LogIL(System.Reflection.Emit.OpCode opcode, object arg, string extra = null)
	{
		if (debug)
		{
			string text = FormatArgument(arg, extra);
			string text2 = ((text.Length > 0) ? " " : "");
			string text3 = opcode.ToString();
			if (opcode.FlowControl == System.Reflection.Emit.FlowControl.Branch || opcode.FlowControl == System.Reflection.Emit.FlowControl.Cond_Branch)
			{
				text3 += " =>";
			}
			text3 = text3.PadRight(10);
			FileLog.LogBuffered($"{CodePos()}{text3}{text2}{text}");
		}
	}

	internal void LogAllLocalVariables()
	{
		if (debug)
		{
			il.IL.Body.Variables.Do(delegate(VariableDefinition v)
			{
				string str = string.Format("{0}Local var {1}: {2}{3}", CodePos(0), v.Index, v.VariableType.FullName, v.IsPinned ? "(pinned)" : "");
				FileLog.LogBuffered(str);
			});
		}
	}

	internal static string FormatArgument(object argument, string extra = null)
	{
		if (argument == null)
		{
			return "NULL";
		}
		Type type = argument.GetType();
		if (argument is MethodBase member)
		{
			return member.FullDescription() + ((extra != null) ? (" " + extra) : "");
		}
		if (argument is FieldInfo fieldInfo)
		{
			return $"{fieldInfo.FieldType.FullDescription()} {fieldInfo.DeclaringType.FullDescription()}::{fieldInfo.Name}";
		}
		if (type == typeof(Label))
		{
			return $"Label{((Label)argument/*cast due to .constrained prefix*/).GetHashCode()}";
		}
		if (type == typeof(Label[]))
		{
			return "Labels" + string.Join(",", ((Label[])argument).Select((Label l) => l.GetHashCode().ToString()).ToArray());
		}
		if (type == typeof(LocalBuilder))
		{
			return $"{((LocalBuilder)argument).LocalIndex} ({((LocalBuilder)argument).LocalType})";
		}
		if (type == typeof(string))
		{
			return argument.ToString().ToLiteral();
		}
		return argument.ToString().Trim();
	}

	internal void MarkLabel(Label label)
	{
		if (debug)
		{
			FileLog.LogBuffered(CodePos() + FormatArgument(label));
		}
		il.MarkLabel(label);
	}

	internal void MarkBlockBefore(ExceptionBlock block, out Label? label)
	{
		label = null;
		switch (block.blockType)
		{
		case ExceptionBlockType.BeginExceptionBlock:
			if (debug)
			{
				FileLog.LogBuffered(".try");
				FileLog.LogBuffered("{");
				FileLog.ChangeIndent(1);
			}
			label = il.BeginExceptionBlock();
			break;
		case ExceptionBlockType.BeginCatchBlock:
			if (debug)
			{
				LogIL(System.Reflection.Emit.OpCodes.Leave, new LeaveTry());
				FileLog.ChangeIndent(-1);
				FileLog.LogBuffered("} // end try");
				FileLog.LogBuffered($".catch {block.catchType}");
				FileLog.LogBuffered("{");
				FileLog.ChangeIndent(1);
			}
			il.BeginCatchBlock(block.catchType);
			break;
		case ExceptionBlockType.BeginExceptFilterBlock:
			if (debug)
			{
				LogIL(System.Reflection.Emit.OpCodes.Leave, new LeaveTry());
				FileLog.ChangeIndent(-1);
				FileLog.LogBuffered("} // end try");
				FileLog.LogBuffered(".filter");
				FileLog.LogBuffered("{");
				FileLog.ChangeIndent(1);
			}
			il.BeginExceptFilterBlock();
			break;
		case ExceptionBlockType.BeginFaultBlock:
			if (debug)
			{
				LogIL(System.Reflection.Emit.OpCodes.Leave, new LeaveTry());
				FileLog.ChangeIndent(-1);
				FileLog.LogBuffered("} // end try");
				FileLog.LogBuffered(".fault");
				FileLog.LogBuffered("{");
				FileLog.ChangeIndent(1);
			}
			il.BeginFaultBlock();
			break;
		case ExceptionBlockType.BeginFinallyBlock:
			if (debug)
			{
				LogIL(System.Reflection.Emit.OpCodes.Leave, new LeaveTry());
				FileLog.ChangeIndent(-1);
				FileLog.LogBuffered("} // end try");
				FileLog.LogBuffered(".finally");
				FileLog.LogBuffered("{");
				FileLog.ChangeIndent(1);
			}
			il.BeginFinallyBlock();
			break;
		}
	}

	internal void MarkBlockAfter(ExceptionBlock block)
	{
		ExceptionBlockType blockType = block.blockType;
		if (blockType == ExceptionBlockType.EndExceptionBlock)
		{
			if (debug)
			{
				LogIL(System.Reflection.Emit.OpCodes.Leave, new LeaveTry());
				FileLog.ChangeIndent(-1);
				FileLog.LogBuffered("} // end handler");
			}
			il.EndExceptionBlock();
		}
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode));
		LogIL(opcode);
		il.Emit(opcode);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, LocalBuilder local)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, local));
		LogIL(opcode, local);
		il.Emit(opcode, local);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, FieldInfo field)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, field));
		LogIL(opcode, field);
		il.Emit(opcode, field);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, Label[] labels)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, labels));
		LogIL(opcode, labels);
		il.Emit(opcode, labels);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, Label label)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, label));
		LogIL(opcode, label);
		il.Emit(opcode, label);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, string str)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, str));
		LogIL(opcode, str);
		il.Emit(opcode, str);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, float arg)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, arg));
		LogIL(opcode, arg);
		il.Emit(opcode, arg);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, byte arg)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, arg));
		LogIL(opcode, arg);
		il.Emit(opcode, arg);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, sbyte arg)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, arg));
		LogIL(opcode, arg);
		il.Emit(opcode, arg);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, double arg)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, arg));
		LogIL(opcode, arg);
		il.Emit(opcode, arg);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, int arg)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, arg));
		LogIL(opcode, arg);
		il.Emit(opcode, arg);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, MethodInfo meth)
	{
		if (opcode.Equals(System.Reflection.Emit.OpCodes.Call) || opcode.Equals(System.Reflection.Emit.OpCodes.Callvirt) || opcode.Equals(System.Reflection.Emit.OpCodes.Newobj))
		{
			EmitCall(opcode, meth, null);
			return;
		}
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, meth));
		LogIL(opcode, meth);
		il.Emit(opcode, meth);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, short arg)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, arg));
		LogIL(opcode, arg);
		il.Emit(opcode, arg);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, SignatureHelper signature)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, signature));
		LogIL(opcode, signature);
		il.Emit(opcode, signature);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, ConstructorInfo con)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, con));
		LogIL(opcode, con);
		il.Emit(opcode, con);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, Type cls)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, cls));
		LogIL(opcode, cls);
		il.Emit(opcode, cls);
	}

	internal void Emit(System.Reflection.Emit.OpCode opcode, long arg)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, arg));
		LogIL(opcode, arg);
		il.Emit(opcode, arg);
	}

	internal void EmitCall(System.Reflection.Emit.OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, methodInfo));
		string extra = ((optionalParameterTypes != null && optionalParameterTypes.Length != 0) ? optionalParameterTypes.Description() : null);
		LogIL(opcode, methodInfo, extra);
		il.EmitCall(opcode, methodInfo, optionalParameterTypes);
	}

	internal void EmitCalli(System.Reflection.Emit.OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, unmanagedCallConv));
		string extra = returnType.FullName + " " + parameterTypes.Description();
		LogIL(opcode, unmanagedCallConv, extra);
		il.EmitCalli(opcode, unmanagedCallConv, returnType, parameterTypes);
	}

	internal void EmitCalli(System.Reflection.Emit.OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
	{
		instructions.Add(CurrentPos(), new CodeInstruction(opcode, callingConvention));
		string extra = returnType.FullName + " " + parameterTypes.Description() + " " + optionalParameterTypes.Description();
		LogIL(opcode, callingConvention, extra);
		il.EmitCalli(opcode, callingConvention, returnType, parameterTypes, optionalParameterTypes);
	}
}
