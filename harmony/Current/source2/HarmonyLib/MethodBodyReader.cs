using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MonoMod.Utils;
using MonoMod.Utils.Cil;

namespace HarmonyLib;

internal class MethodBodyReader
{
	private class ThisParameter : ParameterInfo
	{
		internal ThisParameter(MethodBase method)
		{
			MemberImpl = method;
			ClassImpl = method.DeclaringType;
			NameImpl = "this";
			PositionImpl = -1;
		}
	}

	private readonly ILGenerator generator;

	private readonly MethodBase method;

	private bool debug;

	private readonly Module module;

	private readonly Type[] typeArguments;

	private readonly Type[] methodArguments;

	private readonly ByteBuffer ilBytes;

	private readonly ParameterInfo this_parameter;

	private readonly ParameterInfo[] parameters;

	private readonly IList<ExceptionHandlingClause> exceptions;

	private readonly List<ILInstruction> ilInstructions;

	private readonly List<LocalVariableInfo> localVariables;

	private LocalBuilder[] variables;

	private static readonly Dictionary<OpCode, OpCode> shortJumps;

	private static readonly OpCode[] one_byte_opcodes;

	private static readonly OpCode[] two_bytes_opcodes;

	internal static List<ILInstruction> GetInstructions(ILGenerator generator, MethodBase method)
	{
		if ((object)method == null)
		{
			throw new ArgumentNullException("method");
		}
		MethodBodyReader methodBodyReader = new MethodBodyReader(method, generator);
		methodBodyReader.DeclareVariables(null);
		methodBodyReader.GenerateInstructions();
		return methodBodyReader.ilInstructions;
	}

	internal MethodBodyReader(MethodBase method, ILGenerator generator)
	{
		this.generator = generator;
		this.method = method;
		module = method.Module;
		MethodBody methodBody = method.GetMethodBody();
		if ((methodBody?.GetILAsByteArray()?.Length).GetValueOrDefault() == 0)
		{
			ilBytes = new ByteBuffer(Array.Empty<byte>());
			ilInstructions = new List<ILInstruction>();
		}
		else
		{
			byte[] iLAsByteArray = methodBody.GetILAsByteArray();
			if (iLAsByteArray == null)
			{
				throw new ArgumentException("Can not get IL bytes of method " + method.FullDescription());
			}
			ilBytes = new ByteBuffer(iLAsByteArray);
			ilInstructions = new List<ILInstruction>((iLAsByteArray.Length + 1) / 2);
		}
		Type declaringType = method.DeclaringType;
		if ((object)declaringType != null && declaringType.IsGenericType)
		{
			try
			{
				typeArguments = declaringType.GetGenericArguments();
			}
			catch
			{
				typeArguments = null;
			}
		}
		if (method.IsGenericMethod)
		{
			try
			{
				methodArguments = method.GetGenericArguments();
			}
			catch
			{
				methodArguments = null;
			}
		}
		if (!method.IsStatic)
		{
			this_parameter = new ThisParameter(method);
		}
		parameters = method.GetParameters();
		localVariables = methodBody?.LocalVariables?.ToList() ?? new List<LocalVariableInfo>();
		exceptions = methodBody?.ExceptionHandlingClauses ?? new List<ExceptionHandlingClause>();
	}

	internal void SetDebugging(bool debug)
	{
		this.debug = debug;
	}

	internal void GenerateInstructions()
	{
		while (ilBytes.position < ilBytes.buffer.Length)
		{
			int position = ilBytes.position;
			ILInstruction iLInstruction = new ILInstruction(ReadOpCode())
			{
				offset = position
			};
			ReadOperand(iLInstruction);
			ilInstructions.Add(iLInstruction);
		}
		HandleNativeMethod();
		ResolveBranches();
		ParseExceptions();
	}

	internal void HandleNativeMethod()
	{
		if (!(method is MethodInfo methodInfo))
		{
			return;
		}
		DllImportAttribute dllImportAttribute = methodInfo.GetCustomAttributes(inherit: false).OfType<DllImportAttribute>().FirstOrDefault();
		if (dllImportAttribute != null)
		{
			string assemblyName = (methodInfo.DeclaringType?.FullName ?? "").Replace(".", "_") + "_" + methodInfo.Name;
			AssemblyName assemblyName2 = new AssemblyName(assemblyName);
			AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName2, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName2.Name);
			TypeBuilder typeBuilder = moduleBuilder.DefineType("NativeMethodHolder", TypeAttributes.Public | TypeAttributes.UnicodeClass);
			MethodBuilder methodBuilder = typeBuilder.DefinePInvokeMethod(methodInfo.Name, dllImportAttribute.Value, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl, CallingConventions.Standard, methodInfo.ReturnType, (from x in methodInfo.GetParameters()
				select x.ParameterType).ToArray(), dllImportAttribute.CallingConvention, dllImportAttribute.CharSet);
			methodBuilder.SetImplementationFlags(methodBuilder.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);
			Type type = typeBuilder.CreateType();
			MethodInfo operand = type.GetMethod(methodInfo.Name);
			int num = method.GetParameters().Length;
			for (int i = 0; i < num; i++)
			{
				ilInstructions.Add(new ILInstruction(OpCodes.Ldarg, i)
				{
					offset = 0
				});
			}
			ilInstructions.Add(new ILInstruction(OpCodes.Call, operand)
			{
				offset = num
			});
			ilInstructions.Add(new ILInstruction(OpCodes.Ret)
			{
				offset = num + 5
			});
		}
	}

	internal void DeclareVariables(LocalBuilder[] existingVariables)
	{
		if (generator == null)
		{
			return;
		}
		if (existingVariables != null)
		{
			variables = existingVariables;
			return;
		}
		variables = localVariables.Select((LocalVariableInfo lvi) => generator.DeclareLocal(lvi.LocalType, lvi.IsPinned)).ToArray();
	}

	private void ResolveBranches()
	{
		foreach (ILInstruction ilInstruction in ilInstructions)
		{
			switch (ilInstruction.opcode.OperandType)
			{
			case OperandType.InlineBrTarget:
			case OperandType.ShortInlineBrTarget:
				ilInstruction.operand = GetInstruction((int)ilInstruction.operand, isEndOfInstruction: false);
				break;
			case OperandType.InlineSwitch:
			{
				int[] array = (int[])ilInstruction.operand;
				ILInstruction[] array2 = new ILInstruction[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = GetInstruction(array[i], isEndOfInstruction: false);
				}
				ilInstruction.operand = array2;
				break;
			}
			}
		}
	}

	private void ParseExceptions()
	{
		foreach (ExceptionHandlingClause exception in exceptions)
		{
			int tryOffset = exception.TryOffset;
			int handlerOffset = exception.HandlerOffset;
			int offset = exception.HandlerOffset + exception.HandlerLength - 1;
			ILInstruction instruction = GetInstruction(tryOffset, isEndOfInstruction: false);
			instruction.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock));
			ILInstruction instruction2 = GetInstruction(offset, isEndOfInstruction: true);
			instruction2.blocks.Add(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock));
			switch (exception.Flags)
			{
			case ExceptionHandlingClauseOptions.Filter:
			{
				ILInstruction instruction6 = GetInstruction(exception.FilterOffset, isEndOfInstruction: false);
				instruction6.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginExceptFilterBlock));
				break;
			}
			case ExceptionHandlingClauseOptions.Finally:
			{
				ILInstruction instruction5 = GetInstruction(handlerOffset, isEndOfInstruction: false);
				instruction5.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock));
				break;
			}
			case ExceptionHandlingClauseOptions.Clause:
			{
				ILInstruction instruction4 = GetInstruction(handlerOffset, isEndOfInstruction: false);
				instruction4.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, exception.CatchType));
				break;
			}
			case ExceptionHandlingClauseOptions.Fault:
			{
				ILInstruction instruction3 = GetInstruction(handlerOffset, isEndOfInstruction: false);
				instruction3.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginFaultBlock));
				break;
			}
			}
		}
	}

	private bool EndsInDeadCode(List<CodeInstruction> list)
	{
		int count = list.Count;
		if (count < 2 || list.Last().opcode != OpCodes.Throw)
		{
			return false;
		}
		return list.GetRange(0, count - 1).All((CodeInstruction code) => code.opcode != OpCodes.Ret);
	}

	internal List<CodeInstruction> FinalizeILCodes(Emitter emitter, List<MethodInfo> transpilers, List<Label> endLabels, out bool hasReturnCode, out bool methodEndsInDeadCode)
	{
		hasReturnCode = false;
		methodEndsInDeadCode = false;
		if (generator == null)
		{
			return null;
		}
		foreach (ILInstruction ilInstruction in ilInstructions)
		{
			switch (ilInstruction.opcode.OperandType)
			{
			case OperandType.InlineSwitch:
				if (ilInstruction.operand is ILInstruction[] array)
				{
					List<Label> list = new List<Label>();
					ILInstruction[] array2 = array;
					foreach (ILInstruction iLInstruction2 in array2)
					{
						Label item = generator.DefineLabel();
						iLInstruction2.labels.Add(item);
						list.Add(item);
					}
					ilInstruction.argument = list.ToArray();
				}
				break;
			case OperandType.InlineBrTarget:
			case OperandType.ShortInlineBrTarget:
				if (ilInstruction.operand is ILInstruction iLInstruction)
				{
					Label label2 = generator.DefineLabel();
					iLInstruction.labels.Add(label2);
					ilInstruction.argument = label2;
				}
				break;
			}
		}
		CodeTranspiler codeTranspiler = new CodeTranspiler(ilInstructions);
		transpilers.Do(codeTranspiler.Add);
		List<CodeInstruction> result = codeTranspiler.GetResult(generator, method);
		if (emitter == null)
		{
			return result;
		}
		emitter.LogComment("start original");
		if (debug)
		{
			List<string> buffer = FileLog.GetBuffer(clear: true);
			emitter.LogAllLocalVariables();
			FileLog.LogBuffered(buffer);
		}
		hasReturnCode = result.Any((CodeInstruction code) => code.opcode == OpCodes.Ret);
		methodEndsInDeadCode = EndsInDeadCode(result);
		while (true)
		{
			CodeInstruction codeInstruction2 = result.LastOrDefault();
			if (codeInstruction2 == null || codeInstruction2.opcode != OpCodes.Ret)
			{
				break;
			}
			endLabels.AddRange(codeInstruction2.labels);
			result.RemoveAt(result.Count - 1);
		}
		result.Do(delegate(CodeInstruction codeInstruction)
		{
			codeInstruction.labels.Do(delegate(Label label)
			{
				emitter.MarkLabel(label);
			});
			codeInstruction.blocks.Do(delegate(ExceptionBlock block)
			{
				emitter.MarkBlockBefore(block, out var _);
			});
			OpCode opCode = codeInstruction.opcode;
			object obj = codeInstruction.operand;
			if (opCode == OpCodes.Ret)
			{
				Label label4 = generator.DefineLabel();
				opCode = OpCodes.Br;
				obj = label4;
				endLabels.Add(label4);
			}
			if (shortJumps.TryGetValue(opCode, out var value))
			{
				opCode = value;
			}
			switch (opCode.OperandType)
			{
			case OperandType.InlineNone:
				emitter.Emit(opCode);
				break;
			case OperandType.InlineSig:
			{
				CecilILGenerator proxiedShim = generator.GetProxiedShim<CecilILGenerator>();
				if (proxiedShim == null)
				{
					throw new NotSupportedException();
				}
				if (obj == null)
				{
					throw new Exception($"Wrong null argument: {codeInstruction}");
				}
				if (!(obj is ICallSiteGenerator))
				{
					throw new Exception($"Wrong Emit argument type {obj.GetType()} in {codeInstruction}");
				}
				emitter.AddInstruction(opCode, obj);
				emitter.LogIL(opCode, obj);
				proxiedShim.Emit(opCode, (ICallSiteGenerator)obj);
				break;
			}
			default:
				if (obj == null)
				{
					throw new Exception($"Wrong null argument: {codeInstruction}");
				}
				emitter.AddInstruction(opCode, obj);
				emitter.LogIL(opCode, obj);
				generator.DynEmit(opCode, obj);
				break;
			}
			codeInstruction.blocks.Do(delegate(ExceptionBlock block)
			{
				emitter.MarkBlockAfter(block);
			});
		});
		emitter.LogComment("end original" + (methodEndsInDeadCode ? " (has dead code end)" : ""));
		return result;
	}

	private static void GetMemberInfoValue(MemberInfo info, out object result)
	{
		result = null;
		switch (info.MemberType)
		{
		case MemberTypes.Constructor:
			result = (ConstructorInfo)info;
			break;
		case MemberTypes.Event:
			result = (EventInfo)info;
			break;
		case MemberTypes.Field:
			result = (FieldInfo)info;
			break;
		case MemberTypes.Method:
			result = (MethodInfo)info;
			break;
		case MemberTypes.TypeInfo:
		case MemberTypes.NestedType:
			result = (Type)info;
			break;
		case MemberTypes.Property:
			result = (PropertyInfo)info;
			break;
		}
	}

	private void ReadOperand(ILInstruction instruction)
	{
		switch (instruction.opcode.OperandType)
		{
		case OperandType.InlineNone:
			instruction.argument = null;
			break;
		case OperandType.InlineSwitch:
		{
			int num3 = ilBytes.ReadInt32();
			int num4 = ilBytes.position + 4 * num3;
			int[] array2 = new int[num3];
			for (int i = 0; i < num3; i++)
			{
				array2[i] = ilBytes.ReadInt32() + num4;
			}
			instruction.operand = array2;
			break;
		}
		case OperandType.ShortInlineBrTarget:
		{
			sbyte b2 = (sbyte)ilBytes.ReadByte();
			instruction.operand = b2 + ilBytes.position;
			break;
		}
		case OperandType.InlineBrTarget:
		{
			int num5 = ilBytes.ReadInt32();
			instruction.operand = num5 + ilBytes.position;
			break;
		}
		case OperandType.ShortInlineI:
			if (instruction.opcode == OpCodes.Ldc_I4_S)
			{
				sbyte b4 = (sbyte)ilBytes.ReadByte();
				instruction.operand = b4;
				instruction.argument = (sbyte)instruction.operand;
			}
			else
			{
				byte b5 = ilBytes.ReadByte();
				instruction.operand = b5;
				instruction.argument = (byte)instruction.operand;
			}
			break;
		case OperandType.InlineI:
		{
			int num8 = ilBytes.ReadInt32();
			instruction.operand = num8;
			instruction.argument = (int)instruction.operand;
			break;
		}
		case OperandType.ShortInlineR:
		{
			float num2 = ilBytes.ReadSingle();
			instruction.operand = num2;
			instruction.argument = (float)instruction.operand;
			break;
		}
		case OperandType.InlineR:
		{
			double num7 = ilBytes.ReadDouble();
			instruction.operand = num7;
			instruction.argument = (double)instruction.operand;
			break;
		}
		case OperandType.InlineI8:
		{
			long num6 = ilBytes.ReadInt64();
			instruction.operand = num6;
			instruction.argument = (long)instruction.operand;
			break;
		}
		case OperandType.InlineSig:
		{
			int metadataToken3 = ilBytes.ReadInt32();
			byte[] array3 = module.ResolveSignature(metadataToken3);
			InlineSignature value = (InlineSignature)(instruction.argument = (instruction.operand = InlineSignatureParser.ImportCallSite(module, array3)));
			Debugger.Log(0, "TEST", "METHOD " + method.FullDescription() + "\n");
			Debugger.Log(0, "TEST", "Signature Blob = " + array3.Select((byte b) => $"0x{b:x02}").Aggregate((string a, string b) => a + " " + b) + "\n");
			Debugger.Log(0, "TEST", $"Signature = {value}\n");
			Debugger.Break();
			break;
		}
		case OperandType.InlineString:
		{
			int metadataToken6 = ilBytes.ReadInt32();
			instruction.operand = module.ResolveString(metadataToken6);
			instruction.argument = (string)instruction.operand;
			break;
		}
		case OperandType.InlineTok:
		{
			int metadataToken5 = ilBytes.ReadInt32();
			instruction.operand = module.ResolveMember(metadataToken5, typeArguments, methodArguments);
			((MemberInfo)instruction.operand).DeclaringType?.FixReflectionCacheAuto();
			GetMemberInfoValue((MemberInfo)instruction.operand, out instruction.argument);
			break;
		}
		case OperandType.InlineType:
		{
			int metadataToken4 = ilBytes.ReadInt32();
			instruction.operand = module.ResolveType(metadataToken4, typeArguments, methodArguments);
			((Type)instruction.operand).FixReflectionCacheAuto();
			instruction.argument = (Type)instruction.operand;
			break;
		}
		case OperandType.InlineMethod:
		{
			int metadataToken2 = ilBytes.ReadInt32();
			instruction.operand = module.ResolveMethod(metadataToken2, typeArguments, methodArguments);
			((MemberInfo)instruction.operand).DeclaringType?.FixReflectionCacheAuto();
			if (instruction.operand is ConstructorInfo)
			{
				instruction.argument = (ConstructorInfo)instruction.operand;
			}
			else
			{
				instruction.argument = (MethodInfo)instruction.operand;
			}
			break;
		}
		case OperandType.InlineField:
		{
			int metadataToken = ilBytes.ReadInt32();
			instruction.operand = module.ResolveField(metadataToken, typeArguments, methodArguments);
			((MemberInfo)instruction.operand).DeclaringType?.FixReflectionCacheAuto();
			instruction.argument = (FieldInfo)instruction.operand;
			break;
		}
		case OperandType.ShortInlineVar:
		{
			byte b3 = ilBytes.ReadByte();
			if (TargetsLocalVariable(instruction.opcode))
			{
				LocalVariableInfo localVariable2 = GetLocalVariable(b3);
				if (localVariable2 == null)
				{
					instruction.argument = b3;
					break;
				}
				instruction.operand = localVariable2;
				LocalBuilder[] array4 = variables;
				instruction.argument = ((array4 != null) ? array4[localVariable2.LocalIndex] : null) ?? localVariable2;
			}
			else
			{
				instruction.operand = GetParameter(b3);
				instruction.argument = b3;
			}
			break;
		}
		case OperandType.InlineVar:
		{
			short num = ilBytes.ReadInt16();
			if (TargetsLocalVariable(instruction.opcode))
			{
				LocalVariableInfo localVariable = GetLocalVariable(num);
				if (localVariable == null)
				{
					instruction.argument = num;
					break;
				}
				instruction.operand = localVariable;
				LocalBuilder[] array = variables;
				instruction.argument = ((array != null) ? array[localVariable.LocalIndex] : null) ?? localVariable;
			}
			else
			{
				instruction.operand = GetParameter(num);
				instruction.argument = num;
			}
			break;
		}
		default:
			throw new NotSupportedException();
		}
	}

	private ILInstruction GetInstruction(int offset, bool isEndOfInstruction)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", offset, $"Instruction offset {offset} is less than 0");
		}
		int num = ilInstructions.Count - 1;
		ILInstruction iLInstruction = ilInstructions[num];
		if (offset > iLInstruction.offset + iLInstruction.GetSize() - 1)
		{
			throw new ArgumentOutOfRangeException("offset", offset, $"Instruction offset {offset} is outside valid range 0 - {iLInstruction.offset + iLInstruction.GetSize() - 1}");
		}
		int num2 = 0;
		int num3 = num;
		while (num2 <= num3)
		{
			int num4 = num2 + (num3 - num2) / 2;
			iLInstruction = ilInstructions[num4];
			if (isEndOfInstruction)
			{
				if (offset == iLInstruction.offset + iLInstruction.GetSize() - 1)
				{
					return iLInstruction;
				}
			}
			else if (offset == iLInstruction.offset)
			{
				return iLInstruction;
			}
			if (offset < iLInstruction.offset)
			{
				num3 = num4 - 1;
			}
			else
			{
				num2 = num4 + 1;
			}
		}
		throw new Exception($"Cannot find instruction for {offset:X4}");
	}

	private static bool TargetsLocalVariable(OpCode opcode)
	{
		return opcode.Name.Contains("loc");
	}

	private LocalVariableInfo GetLocalVariable(int index)
	{
		return localVariables?[index];
	}

	private ParameterInfo GetParameter(int index)
	{
		if (index == 0)
		{
			return this_parameter;
		}
		return parameters[index - 1];
	}

	private OpCode ReadOpCode()
	{
		byte b = ilBytes.ReadByte();
		if (b == 254)
		{
			return two_bytes_opcodes[ilBytes.ReadByte()];
		}
		return one_byte_opcodes[b];
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	static MethodBodyReader()
	{
		shortJumps = new Dictionary<OpCode, OpCode>
		{
			{
				OpCodes.Leave_S,
				OpCodes.Leave
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
				OpCodes.Beq_S,
				OpCodes.Beq
			},
			{
				OpCodes.Bge_S,
				OpCodes.Bge
			},
			{
				OpCodes.Bgt_S,
				OpCodes.Bgt
			},
			{
				OpCodes.Ble_S,
				OpCodes.Ble
			},
			{
				OpCodes.Blt_S,
				OpCodes.Blt
			},
			{
				OpCodes.Bne_Un_S,
				OpCodes.Bne_Un
			},
			{
				OpCodes.Bge_Un_S,
				OpCodes.Bge_Un
			},
			{
				OpCodes.Bgt_Un_S,
				OpCodes.Bgt_Un
			},
			{
				OpCodes.Ble_Un_S,
				OpCodes.Ble_Un
			},
			{
				OpCodes.Br_S,
				OpCodes.Br
			},
			{
				OpCodes.Blt_Un_S,
				OpCodes.Blt_Un
			}
		};
		one_byte_opcodes = new OpCode[225];
		two_bytes_opcodes = new OpCode[31];
		FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			OpCode opCode = (OpCode)fieldInfo.GetValue(null);
			if (opCode.OpCodeType != OpCodeType.Nternal)
			{
				if (opCode.Size == 1)
				{
					one_byte_opcodes[opCode.Value] = opCode;
				}
				else
				{
					two_bytes_opcodes[opCode.Value & 0xFF] = opCode;
				}
			}
		}
	}
}
