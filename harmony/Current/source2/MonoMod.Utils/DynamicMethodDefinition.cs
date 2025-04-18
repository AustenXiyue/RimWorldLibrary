using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils.Cil;

namespace MonoMod.Utils;

internal sealed class DynamicMethodDefinition : IDisposable
{
	private enum TokenResolutionMode
	{
		Any,
		Type,
		Method,
		Field
	}

	private static Mono.Cecil.Cil.OpCode[] _CecilOpCodes1X;

	private static Mono.Cecil.Cil.OpCode[] _CecilOpCodes2X;

	internal static readonly bool _IsNewMonoSRE;

	internal static readonly bool _IsOldMonoSRE;

	private static bool _PreferCecil;

	internal static readonly ConstructorInfo c_DebuggableAttribute;

	internal static readonly ConstructorInfo c_UnverifiableCodeAttribute;

	internal static readonly ConstructorInfo c_IgnoresAccessChecksToAttribute;

	internal static readonly Type t__IDMDGenerator;

	internal static readonly ConcurrentDictionary<string, IDMDGenerator> _DMDGeneratorCache;

	private Guid GUID = Guid.NewGuid();

	private bool isDisposed;

	public static bool IsDynamicILAvailable => !_PreferCecil;

	public MethodBase? OriginalMethod { get; }

	public MethodDefinition Definition { get; }

	public ModuleDefinition Module { get; }

	public string? Name { get; }

	public bool Debug { get; init; }

	private static void _InitCopier()
	{
		_CecilOpCodes1X = new Mono.Cecil.Cil.OpCode[225];
		_CecilOpCodes2X = new Mono.Cecil.Cil.OpCode[31];
		FieldInfo[] fields = typeof(Mono.Cecil.Cil.OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			Mono.Cecil.Cil.OpCode opCode = (Mono.Cecil.Cil.OpCode)fields[i].GetValue(null);
			if (opCode.OpCodeType != Mono.Cecil.Cil.OpCodeType.Nternal)
			{
				if (opCode.Size == 1)
				{
					_CecilOpCodes1X[opCode.Value] = opCode;
				}
				else
				{
					_CecilOpCodes2X[opCode.Value & 0xFF] = opCode;
				}
			}
		}
	}

	private static void _CopyMethodToDefinition(MethodBase from, MethodDefinition into)
	{
		Module moduleFrom = from.Module;
		System.Reflection.MethodBody methodBody = from.GetMethodBody() ?? throw new NotSupportedException("Body-less method");
		byte[] buffer = methodBody.GetILAsByteArray() ?? throw new InvalidOperationException();
		ModuleDefinition moduleTo = into.Module;
		Mono.Cecil.Cil.MethodBody bodyTo = into.Body;
		bodyTo.GetILProcessor();
		Type[] typeArguments = null;
		Type declaringType = from.DeclaringType;
		if ((object)declaringType != null && declaringType.IsGenericType)
		{
			typeArguments = from.DeclaringType.GetGenericArguments();
		}
		Type[] methodArguments = null;
		if (from.IsGenericMethod)
		{
			methodArguments = from.GetGenericArguments();
		}
		foreach (LocalVariableInfo localVariable in methodBody.LocalVariables)
		{
			TypeReference typeReference = moduleTo.ImportReference(localVariable.LocalType);
			if (localVariable.IsPinned)
			{
				typeReference = new PinnedType(typeReference);
			}
			bodyTo.Variables.Add(new VariableDefinition(typeReference));
		}
		using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(buffer)))
		{
			Instruction instruction = null;
			Instruction instruction2 = null;
			while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
			{
				int offset2 = (int)binaryReader.BaseStream.Position;
				instruction = Instruction.Create(Mono.Cecil.Cil.OpCodes.Nop);
				byte b = binaryReader.ReadByte();
				instruction.OpCode = ((b != 254) ? _CecilOpCodes1X[b] : _CecilOpCodes2X[binaryReader.ReadByte()]);
				instruction.Offset = offset2;
				if (instruction2 != null)
				{
					instruction2.Next = instruction;
				}
				instruction.Previous = instruction2;
				ReadOperand(binaryReader, instruction);
				bodyTo.Instructions.Add(instruction);
				instruction2 = instruction;
			}
		}
		foreach (Instruction instruction4 in bodyTo.Instructions)
		{
			switch (instruction4.OpCode.OperandType)
			{
			case Mono.Cecil.Cil.OperandType.InlineBrTarget:
			case Mono.Cecil.Cil.OperandType.ShortInlineBrTarget:
				instruction4.Operand = GetInstruction((int)instruction4.Operand);
				break;
			case Mono.Cecil.Cil.OperandType.InlineSwitch:
			{
				int[] array = (int[])instruction4.Operand;
				Instruction[] array2 = new Instruction[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = GetInstruction(array[i]);
				}
				instruction4.Operand = array2;
				break;
			}
			}
		}
		foreach (ExceptionHandlingClause exceptionHandlingClause in methodBody.ExceptionHandlingClauses)
		{
			Mono.Cecil.Cil.ExceptionHandler exceptionHandler = new Mono.Cecil.Cil.ExceptionHandler((ExceptionHandlerType)exceptionHandlingClause.Flags);
			bodyTo.ExceptionHandlers.Add(exceptionHandler);
			exceptionHandler.TryStart = GetInstruction(exceptionHandlingClause.TryOffset);
			exceptionHandler.TryEnd = GetInstruction(exceptionHandlingClause.TryOffset + exceptionHandlingClause.TryLength);
			exceptionHandler.FilterStart = ((exceptionHandler.HandlerType != ExceptionHandlerType.Filter) ? null : GetInstruction(exceptionHandlingClause.FilterOffset));
			exceptionHandler.HandlerStart = GetInstruction(exceptionHandlingClause.HandlerOffset);
			exceptionHandler.HandlerEnd = GetInstruction(exceptionHandlingClause.HandlerOffset + exceptionHandlingClause.HandlerLength);
			exceptionHandler.CatchType = ((exceptionHandler.HandlerType != 0) ? null : ((exceptionHandlingClause.CatchType == null) ? null : moduleTo.ImportReference(exceptionHandlingClause.CatchType)));
		}
		Instruction? GetInstruction(int offset)
		{
			int num3 = bodyTo.Instructions.Count - 1;
			if (offset < 0 || offset > bodyTo.Instructions[num3].Offset)
			{
				return null;
			}
			int num4 = 0;
			int num5 = num3;
			while (num4 <= num5)
			{
				int num6 = num4 + (num5 - num4) / 2;
				Instruction instruction3 = bodyTo.Instructions[num6];
				if (offset == instruction3.Offset)
				{
					return instruction3;
				}
				if (offset < instruction3.Offset)
				{
					num5 = num6 - 1;
				}
				else
				{
					num4 = num6 + 1;
				}
			}
			return null;
		}
		void ReadOperand(BinaryReader reader, Instruction instr)
		{
			switch (instr.OpCode.OperandType)
			{
			case Mono.Cecil.Cil.OperandType.InlineNone:
				instr.Operand = null;
				break;
			case Mono.Cecil.Cil.OperandType.InlineSwitch:
			{
				int num = reader.ReadInt32();
				int num2 = (int)reader.BaseStream.Position + 4 * num;
				int[] array3 = new int[num];
				for (int j = 0; j < num; j++)
				{
					array3[j] = reader.ReadInt32() + num2;
				}
				instr.Operand = array3;
				break;
			}
			case Mono.Cecil.Cil.OperandType.ShortInlineBrTarget:
			{
				int num2 = reader.ReadSByte();
				instr.Operand = (int)reader.BaseStream.Position + num2;
				break;
			}
			case Mono.Cecil.Cil.OperandType.InlineBrTarget:
			{
				int num2 = reader.ReadInt32();
				instr.Operand = (int)reader.BaseStream.Position + num2;
				break;
			}
			case Mono.Cecil.Cil.OperandType.ShortInlineI:
				instr.Operand = ((instr.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_S) ? ((object)reader.ReadSByte()) : ((object)reader.ReadByte()));
				break;
			case Mono.Cecil.Cil.OperandType.InlineI:
				instr.Operand = reader.ReadInt32();
				break;
			case Mono.Cecil.Cil.OperandType.ShortInlineR:
				instr.Operand = reader.ReadSingle();
				break;
			case Mono.Cecil.Cil.OperandType.InlineR:
				instr.Operand = reader.ReadDouble();
				break;
			case Mono.Cecil.Cil.OperandType.InlineI8:
				instr.Operand = reader.ReadInt64();
				break;
			case Mono.Cecil.Cil.OperandType.InlineSig:
				instr.Operand = moduleTo.ImportCallSite(moduleFrom, moduleFrom.ResolveSignature(reader.ReadInt32()));
				break;
			case Mono.Cecil.Cil.OperandType.InlineString:
				instr.Operand = moduleFrom.ResolveString(reader.ReadInt32());
				break;
			case Mono.Cecil.Cil.OperandType.InlineTok:
				instr.Operand = ResolveTokenAs(reader.ReadInt32(), TokenResolutionMode.Any);
				break;
			case Mono.Cecil.Cil.OperandType.InlineType:
				instr.Operand = ResolveTokenAs(reader.ReadInt32(), TokenResolutionMode.Type);
				break;
			case Mono.Cecil.Cil.OperandType.InlineMethod:
				instr.Operand = ResolveTokenAs(reader.ReadInt32(), TokenResolutionMode.Method);
				break;
			case Mono.Cecil.Cil.OperandType.InlineField:
				instr.Operand = ResolveTokenAs(reader.ReadInt32(), TokenResolutionMode.Field);
				break;
			case Mono.Cecil.Cil.OperandType.InlineVar:
			case Mono.Cecil.Cil.OperandType.ShortInlineVar:
			{
				int index = ((instr.OpCode.OperandType == Mono.Cecil.Cil.OperandType.ShortInlineVar) ? reader.ReadByte() : reader.ReadInt16());
				instr.Operand = bodyTo.Variables[index];
				break;
			}
			case Mono.Cecil.Cil.OperandType.InlineArg:
			case Mono.Cecil.Cil.OperandType.ShortInlineArg:
			{
				int index = ((instr.OpCode.OperandType == Mono.Cecil.Cil.OperandType.ShortInlineArg) ? reader.ReadByte() : reader.ReadInt16());
				instr.Operand = into.Parameters[index];
				break;
			}
			default:
				throw new NotSupportedException("Unsupported opcode $" + instr.OpCode.Name);
			}
		}
		MemberReference ResolveTokenAs(int token, TokenResolutionMode resolveMode)
		{
			try
			{
				switch (resolveMode)
				{
				case TokenResolutionMode.Type:
				{
					Type type2 = moduleFrom.ResolveType(token, typeArguments, methodArguments);
					type2.FixReflectionCacheAuto();
					return moduleTo.ImportReference(type2);
				}
				case TokenResolutionMode.Method:
				{
					MethodBase methodBase2 = moduleFrom.ResolveMethod(token, typeArguments, methodArguments);
					methodBase2?.GetRealDeclaringType()?.FixReflectionCacheAuto();
					return moduleTo.ImportReference(methodBase2);
				}
				case TokenResolutionMode.Field:
				{
					FieldInfo fieldInfo2 = moduleFrom.ResolveField(token, typeArguments, methodArguments);
					fieldInfo2?.GetRealDeclaringType()?.FixReflectionCacheAuto();
					return moduleTo.ImportReference(fieldInfo2);
				}
				case TokenResolutionMode.Any:
				{
					MemberInfo memberInfo = moduleFrom.ResolveMember(token, typeArguments, methodArguments);
					if (memberInfo is Type type)
					{
						type.FixReflectionCacheAuto();
						return moduleTo.ImportReference(type);
					}
					if (memberInfo is MethodBase methodBase)
					{
						methodBase.GetRealDeclaringType()?.FixReflectionCacheAuto();
						return moduleTo.ImportReference(methodBase);
					}
					if (!(memberInfo is FieldInfo fieldInfo))
					{
						throw new NotSupportedException($"Invalid resolved member type {memberInfo?.GetType()}");
					}
					fieldInfo.GetRealDeclaringType()?.FixReflectionCacheAuto();
					return moduleTo.ImportReference(fieldInfo);
				}
				default:
					throw new NotSupportedException($"Invalid TokenResolutionMode {resolveMode}");
				}
			}
			catch (MissingMemberException)
			{
				string location = moduleFrom.Assembly.Location;
				if (!File.Exists(location))
				{
					throw;
				}
				using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(location, new ReaderParameters
				{
					ReadingMode = ReadingMode.Deferred
				});
				MemberReference memberReference = (MemberReference)assemblyDefinition.Modules.First((ModuleDefinition m) => m.Name == moduleFrom.Name).LookupToken(token);
				return resolveMode switch
				{
					TokenResolutionMode.Type => (TypeReference)memberReference, 
					TokenResolutionMode.Method => (MethodReference)memberReference, 
					TokenResolutionMode.Field => (FieldReference)memberReference, 
					TokenResolutionMode.Any => memberReference, 
					_ => throw new NotSupportedException($"Invalid TokenResolutionMode {resolveMode}"), 
				};
			}
		}
	}

	static DynamicMethodDefinition()
	{
		_CecilOpCodes1X = null;
		_CecilOpCodes2X = null;
		_IsNewMonoSRE = PlatformDetection.Runtime == RuntimeKind.Mono && typeof(DynamicMethod).GetField("il_info", BindingFlags.Instance | BindingFlags.NonPublic) != null;
		_IsOldMonoSRE = PlatformDetection.Runtime == RuntimeKind.Mono && !_IsNewMonoSRE && typeof(DynamicMethod).GetField("ilgen", BindingFlags.Instance | BindingFlags.NonPublic) != null;
		_PreferCecil = (PlatformDetection.Runtime == RuntimeKind.Mono && !_IsNewMonoSRE && !_IsOldMonoSRE) || (PlatformDetection.Runtime != RuntimeKind.Mono && typeof(ILGenerator).Assembly.GetType("System.Reflection.Emit.DynamicILGenerator")?.GetField("m_scope", BindingFlags.Instance | BindingFlags.NonPublic) == null);
		c_DebuggableAttribute = typeof(DebuggableAttribute).GetConstructor(new Type[1] { typeof(DebuggableAttribute.DebuggingModes) });
		c_UnverifiableCodeAttribute = typeof(UnverifiableCodeAttribute).GetConstructor(ArrayEx.Empty<Type>());
		c_IgnoresAccessChecksToAttribute = typeof(IgnoresAccessChecksToAttribute).GetConstructor(new Type[1] { typeof(string) });
		t__IDMDGenerator = typeof(IDMDGenerator);
		_DMDGeneratorCache = new ConcurrentDictionary<string, IDMDGenerator>();
		_InitCopier();
	}

	private static bool GetDefaultDebugValue()
	{
		bool isEnabled;
		return Switches.TryGetSwitchEnabled("DMDDebug", out isEnabled) && isEnabled;
	}

	public DynamicMethodDefinition(MethodBase method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		OriginalMethod = method;
		Debug = GetDefaultDebugValue();
		LoadFromMethod(method, out ModuleDefinition Module, out MethodDefinition def);
		this.Module = Module;
		Definition = def;
	}

	public DynamicMethodDefinition(string name, Type? returnType, Type[] parameterTypes)
	{
		Helpers.ThrowIfArgumentNull(name, "name");
		Helpers.ThrowIfArgumentNull(parameterTypes, "parameterTypes");
		Name = name;
		OriginalMethod = null;
		Debug = GetDefaultDebugValue();
		_CreateDynModule(name, returnType, parameterTypes, out ModuleDefinition Module, out MethodDefinition Definition);
		this.Module = Module;
		this.Definition = Definition;
	}

	[MemberNotNull("Definition")]
	public ILProcessor GetILProcessor()
	{
		if (Definition == null)
		{
			throw new InvalidOperationException();
		}
		return Definition.Body.GetILProcessor();
	}

	[MemberNotNull("Definition")]
	public ILGenerator GetILGenerator()
	{
		if (Definition == null)
		{
			throw new InvalidOperationException();
		}
		return new CecilILGenerator(Definition.Body.GetILProcessor()).GetProxy();
	}

	private void _CreateDynModule(string name, Type? returnType, Type[] parameterTypes, out ModuleDefinition Module, out MethodDefinition Definition)
	{
		ModuleDefinition moduleDefinition = (Module = ModuleDefinition.CreateModule($"DMD:DynModule<{name}>?{GetHashCode()}", new ModuleParameters
		{
			Kind = ModuleKind.Dll,
			ReflectionImporterProvider = MMReflectionImporter.ProviderNoDefault
		}));
		TypeDefinition typeDefinition = new TypeDefinition("", $"DMD<{name}>?{GetHashCode()}", Mono.Cecil.TypeAttributes.Public);
		moduleDefinition.Types.Add(typeDefinition);
		MethodDefinition methodDefinition = (Definition = new MethodDefinition(name, Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.HideBySig, (returnType != null) ? moduleDefinition.ImportReference(returnType) : moduleDefinition.TypeSystem.Void));
		foreach (Type type in parameterTypes)
		{
			methodDefinition.Parameters.Add(new ParameterDefinition(moduleDefinition.ImportReference(type)));
		}
		typeDefinition.Methods.Add(methodDefinition);
	}

	private void LoadFromMethod(MethodBase orig, out ModuleDefinition Module, out MethodDefinition def)
	{
		ParameterInfo[] parameters = orig.GetParameters();
		int num = 0;
		Type[] array;
		if (!orig.IsStatic)
		{
			num++;
			array = new Type[parameters.Length + 1];
			array[0] = orig.GetThisParamType();
		}
		else
		{
			array = new Type[parameters.Length];
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i + num] = parameters[i].ParameterType;
		}
		_CreateDynModule(orig.GetID(null, null, withType: true, proxyMethod: false, simple: true), (orig as MethodInfo)?.ReturnType, array, out Module, out def);
		_CopyMethodToDefinition(orig, def);
		if (!orig.IsStatic)
		{
			def.Parameters[0].Name = "this";
		}
		for (int j = 0; j < parameters.Length; j++)
		{
			def.Parameters[j + num].Name = parameters[j].Name;
		}
	}

	public MethodInfo Generate()
	{
		return Generate(null);
	}

	public MethodInfo Generate(object? context)
	{
		object value;
		string text = (Switches.TryGetSwitchValue("DMDType", out value) ? (value as string) : null);
		if (text != null)
		{
			if (text.Equals("dynamicmethod", StringComparison.OrdinalIgnoreCase) || text.Equals("dm", StringComparison.OrdinalIgnoreCase))
			{
				return DMDGenerator<DMDEmitDynamicMethodGenerator>.Generate(this, context);
			}
			if (text.Equals("cecil", StringComparison.OrdinalIgnoreCase) || text.Equals("md", StringComparison.OrdinalIgnoreCase))
			{
				return DMDGenerator<DMDCecilGenerator>.Generate(this, context);
			}
			if (text.Equals("methodbuilder", StringComparison.OrdinalIgnoreCase) || text.Equals("mb", StringComparison.OrdinalIgnoreCase))
			{
				return DMDGenerator<DMDEmitMethodBuilderGenerator>.Generate(this, context);
			}
		}
		if (text != null)
		{
			Type type = ReflectionHelper.GetType(text);
			if (type != null)
			{
				if (!t__IDMDGenerator.IsCompatible(type))
				{
					throw new ArgumentException("Invalid DMDGenerator type: " + text);
				}
				return _DMDGeneratorCache.GetOrAdd(text, (string _) => (IDMDGenerator)Activator.CreateInstance(type)).Generate(this, context);
			}
		}
		if (_PreferCecil)
		{
			return DMDGenerator<DMDCecilGenerator>.Generate(this, context);
		}
		if (Debug)
		{
			return DMDGenerator<DMDEmitMethodBuilderGenerator>.Generate(this, context);
		}
		if (Definition.Body.ExceptionHandlers.Any((Mono.Cecil.Cil.ExceptionHandler eh) => eh.HandlerType == ExceptionHandlerType.Fault || eh.HandlerType == ExceptionHandlerType.Filter))
		{
			return DMDGenerator<DMDEmitMethodBuilderGenerator>.Generate(this, context);
		}
		return DMDGenerator<DMDEmitDynamicMethodGenerator>.Generate(this, context);
	}

	public void Dispose()
	{
		if (!isDisposed)
		{
			isDisposed = true;
			Module?.Dispose();
		}
	}

	public string GetDumpName(string type)
	{
		return $"DMDASM.{GUID.GetHashCode():X8}{(string.IsNullOrEmpty(type) ? "" : ("." + type))}";
	}
}
