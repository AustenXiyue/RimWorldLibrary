using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Logs;

namespace MonoMod.Utils;

internal static class Extensions
{
	private static readonly Type t_Code = typeof(Code);

	private static readonly Type t_OpCodes = typeof(Mono.Cecil.Cil.OpCodes);

	private static readonly Dictionary<int, Mono.Cecil.Cil.OpCode> _ToLongOp = new Dictionary<int, Mono.Cecil.Cil.OpCode>();

	private static readonly Dictionary<int, Mono.Cecil.Cil.OpCode> _ToShortOp = new Dictionary<int, Mono.Cecil.Cil.OpCode>();

	private static readonly Dictionary<Type, FieldInfo> fmap_mono_assembly = new Dictionary<Type, FieldInfo>();

	private static readonly bool _MonoAssemblyNameHasArch = new AssemblyName("Dummy, ProcessorArchitecture=MSIL").ProcessorArchitecture == ProcessorArchitecture.MSIL;

	private static readonly Type? _RTDynamicMethod = typeof(DynamicMethod).GetNestedType("RTDynamicMethod", BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly Type t_ParamArrayAttribute = typeof(ParamArrayAttribute);

	private static readonly FieldInfo f_GenericParameter_position = typeof(GenericParameter).GetField("position", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("No field 'position' on GenericParameter");

	private static readonly FieldInfo f_GenericParameter_type = typeof(GenericParameter).GetField("type", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("No field 'type' on GenericParameter");

	private static readonly ConcurrentDictionary<Type, int> _GetManagedSizeCache = new ConcurrentDictionary<Type, int>(new KeyValuePair<Type, int>[1]
	{
		new KeyValuePair<Type, int>(typeof(void), 0)
	});

	private static MethodInfo? _GetManagedSizeHelper;

	private static readonly Dictionary<MethodBase, Func<IntPtr>> _GetLdftnPointerCache = new Dictionary<MethodBase, Func<IntPtr>>();

	private static readonly Type? RTDynamicMethod = typeof(DynamicMethod).GetNestedType("RTDynamicMethod", BindingFlags.NonPublic);

	private static readonly FieldInfo? RTDynamicMethod_m_owner = RTDynamicMethod?.GetField("m_owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly Type? t_StateMachineAttribute = typeof(object).Assembly.GetType("System.Runtime.CompilerServices.StateMachineAttribute");

	private static readonly PropertyInfo? p_StateMachineType = t_StateMachineAttribute?.GetProperty("StateMachineType");

	public static TypeDefinition? SafeResolve(this TypeReference? r)
	{
		try
		{
			return r?.Resolve();
		}
		catch
		{
			return null;
		}
	}

	public static FieldDefinition? SafeResolve(this FieldReference? r)
	{
		try
		{
			return r?.Resolve();
		}
		catch
		{
			return null;
		}
	}

	public static MethodDefinition? SafeResolve(this MethodReference? r)
	{
		try
		{
			return r?.Resolve();
		}
		catch
		{
			return null;
		}
	}

	public static PropertyDefinition? SafeResolve(this PropertyReference? r)
	{
		try
		{
			return r?.Resolve();
		}
		catch
		{
			return null;
		}
	}

	public static CustomAttribute? GetCustomAttribute(this Mono.Cecil.ICustomAttributeProvider cap, string attribute)
	{
		if (cap == null || !cap.HasCustomAttributes)
		{
			return null;
		}
		foreach (CustomAttribute customAttribute in cap.CustomAttributes)
		{
			if (customAttribute.AttributeType.FullName == attribute)
			{
				return customAttribute;
			}
		}
		return null;
	}

	public static bool HasCustomAttribute(this Mono.Cecil.ICustomAttributeProvider cap, string attribute)
	{
		return cap.GetCustomAttribute(attribute) != null;
	}

	public static int GetInt(this Instruction instr)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		Mono.Cecil.Cil.OpCode opCode = instr.OpCode;
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_M1)
		{
			return -1;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_0)
		{
			return 0;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_1)
		{
			return 1;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_2)
		{
			return 2;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_3)
		{
			return 3;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_4)
		{
			return 4;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_5)
		{
			return 5;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_6)
		{
			return 6;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_7)
		{
			return 7;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_8)
		{
			return 8;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_S)
		{
			return (sbyte)instr.Operand;
		}
		return (int)instr.Operand;
	}

	public static int? GetIntOrNull(this Instruction instr)
	{
		Helpers.ThrowIfArgumentNull(instr, "instr");
		Mono.Cecil.Cil.OpCode opCode = instr.OpCode;
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_M1)
		{
			return -1;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_0)
		{
			return 0;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_1)
		{
			return 1;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_2)
		{
			return 2;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_3)
		{
			return 3;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_4)
		{
			return 4;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_5)
		{
			return 5;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_6)
		{
			return 6;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_7)
		{
			return 7;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_8)
		{
			return 8;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_S)
		{
			return (sbyte)instr.Operand;
		}
		if (opCode == Mono.Cecil.Cil.OpCodes.Ldc_I4)
		{
			return (int)instr.Operand;
		}
		return null;
	}

	public static bool IsBaseMethodCall(this Mono.Cecil.Cil.MethodBody body, MethodReference? called)
	{
		Helpers.ThrowIfArgumentNull(body, "body");
		MethodDefinition method = body.Method;
		if (called == null)
		{
			return false;
		}
		TypeReference typeReference;
		for (typeReference = called.DeclaringType; typeReference is TypeSpecification typeSpecification; typeReference = typeSpecification.ElementType)
		{
		}
		string patchFullName = typeReference.GetPatchFullName();
		bool flag = false;
		try
		{
			TypeDefinition typeDefinition = method.DeclaringType;
			while ((typeDefinition = typeDefinition.BaseType?.SafeResolve()) != null)
			{
				if (typeDefinition.GetPatchFullName() == patchFullName)
				{
					flag = true;
					break;
				}
			}
		}
		catch
		{
			flag = method.DeclaringType.GetPatchFullName() == patchFullName;
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	public static bool IsCallvirt(this MethodReference method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		if (!method.HasThis)
		{
			return false;
		}
		if (method.DeclaringType.IsValueType)
		{
			return false;
		}
		return true;
	}

	public static bool IsStruct(this TypeReference type)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		if (!type.IsValueType)
		{
			return false;
		}
		if (type.IsPrimitive)
		{
			return false;
		}
		return true;
	}

	public static Mono.Cecil.Cil.OpCode ToLongOp(this Mono.Cecil.Cil.OpCode op)
	{
		string name = Enum.GetName(t_Code, op.Code);
		if (name == null || !name.EndsWith("_S", StringComparison.Ordinal))
		{
			return op;
		}
		lock (_ToLongOp)
		{
			if (_ToLongOp.TryGetValue((int)op.Code, out var value))
			{
				return value;
			}
			return _ToLongOp[(int)op.Code] = ((Mono.Cecil.Cil.OpCode?)t_OpCodes.GetField(name.Substring(0, name.Length - 2))?.GetValue(null)) ?? op;
		}
	}

	public static Mono.Cecil.Cil.OpCode ToShortOp(this Mono.Cecil.Cil.OpCode op)
	{
		string name = Enum.GetName(t_Code, op.Code);
		if (name == null || name.EndsWith("_S", StringComparison.Ordinal))
		{
			return op;
		}
		lock (_ToShortOp)
		{
			if (_ToShortOp.TryGetValue((int)op.Code, out var value))
			{
				return value;
			}
			return _ToShortOp[(int)op.Code] = ((Mono.Cecil.Cil.OpCode?)t_OpCodes.GetField(name + "_S")?.GetValue(null)) ?? op;
		}
	}

	public static void RecalculateILOffsets(this MethodDefinition method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		if (method.HasBody)
		{
			int num = 0;
			for (int i = 0; i < method.Body.Instructions.Count; i++)
			{
				Instruction instruction = method.Body.Instructions[i];
				instruction.Offset = num;
				num += instruction.GetSize();
			}
		}
	}

	public static void FixShortLongOps(this MethodDefinition method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		if (!method.HasBody)
		{
			return;
		}
		for (int i = 0; i < method.Body.Instructions.Count; i++)
		{
			Instruction instruction = method.Body.Instructions[i];
			if (instruction.Operand is Instruction)
			{
				instruction.OpCode = instruction.OpCode.ToLongOp();
			}
		}
		method.RecalculateILOffsets();
		bool flag;
		do
		{
			flag = false;
			for (int j = 0; j < method.Body.Instructions.Count; j++)
			{
				Instruction instruction2 = method.Body.Instructions[j];
				if (instruction2.Operand is Instruction instruction3)
				{
					int num = instruction3.Offset - (instruction2.Offset + instruction2.GetSize());
					if (num == (sbyte)num)
					{
						Mono.Cecil.Cil.OpCode opCode = instruction2.OpCode;
						instruction2.OpCode = instruction2.OpCode.ToShortOp();
						flag = opCode != instruction2.OpCode;
					}
				}
			}
		}
		while (flag);
	}

	public static bool Is(this MemberInfo? minfo, MemberReference? mref)
	{
		return mref.Is(minfo);
	}

	public static bool Is(this MemberReference? mref, MemberInfo? minfo)
	{
		if (mref == null)
		{
			return false;
		}
		if ((object)minfo == null)
		{
			return false;
		}
		TypeReference typeReference = mref.DeclaringType;
		if (typeReference?.FullName == "<Module>")
		{
			typeReference = null;
		}
		if (mref is GenericParameter genericParameter)
		{
			if (!(minfo is Type type))
			{
				return false;
			}
			if (!type.IsGenericParameter)
			{
				if (genericParameter.Owner is IGenericInstance genericInstance)
				{
					return genericInstance.GenericArguments[genericParameter.Position].Is(type);
				}
				return false;
			}
			return genericParameter.Position == type.GenericParameterPosition;
		}
		if (minfo.DeclaringType != null)
		{
			if (typeReference == null)
			{
				return false;
			}
			Type type2 = minfo.DeclaringType;
			if (minfo is Type && type2.IsGenericType && !type2.IsGenericTypeDefinition)
			{
				type2 = type2.GetGenericTypeDefinition();
			}
			if (!typeReference.Is(type2))
			{
				return false;
			}
		}
		else if (typeReference != null)
		{
			return false;
		}
		if (!(mref is TypeSpecification) && mref.Name != minfo.Name)
		{
			return false;
		}
		if (mref is TypeReference typeReference2)
		{
			if (!(minfo is Type type3))
			{
				return false;
			}
			if (type3.IsGenericParameter)
			{
				return false;
			}
			if (mref is GenericInstanceType genericInstanceType)
			{
				if (!type3.IsGenericType)
				{
					return false;
				}
				Collection<TypeReference> genericArguments = genericInstanceType.GenericArguments;
				Type[] genericArguments2 = type3.GetGenericArguments();
				if (genericArguments.Count != genericArguments2.Length)
				{
					return false;
				}
				for (int i = 0; i < genericArguments.Count; i++)
				{
					if (!genericArguments[i].Is(genericArguments2[i]))
					{
						return false;
					}
				}
				return genericInstanceType.ElementType.Is(type3.GetGenericTypeDefinition());
			}
			if (typeReference2.HasGenericParameters)
			{
				if (!type3.IsGenericType)
				{
					return false;
				}
				Collection<GenericParameter> genericParameters = typeReference2.GenericParameters;
				Type[] genericArguments3 = type3.GetGenericArguments();
				if (genericParameters.Count != genericArguments3.Length)
				{
					return false;
				}
				for (int j = 0; j < genericParameters.Count; j++)
				{
					if (!genericParameters[j].Is(genericArguments3[j]))
					{
						return false;
					}
				}
			}
			else if (type3.IsGenericType)
			{
				return false;
			}
			if (mref is ArrayType arrayType)
			{
				if (!type3.IsArray)
				{
					return false;
				}
				if (arrayType.Dimensions.Count == type3.GetArrayRank())
				{
					return arrayType.ElementType.Is(type3.GetElementType());
				}
				return false;
			}
			if (mref is ByReferenceType byReferenceType)
			{
				if (!type3.IsByRef)
				{
					return false;
				}
				return byReferenceType.ElementType.Is(type3.GetElementType());
			}
			if (mref is PointerType pointerType)
			{
				if (!type3.IsPointer)
				{
					return false;
				}
				return pointerType.ElementType.Is(type3.GetElementType());
			}
			if (mref is TypeSpecification typeSpecification)
			{
				return typeSpecification.ElementType.Is(type3.HasElementType ? type3.GetElementType() : type3);
			}
			if (typeReference != null)
			{
				return mref.Name == type3.Name;
			}
			return mref.FullName == type3.FullName?.Replace("+", "/", StringComparison.Ordinal);
		}
		if (minfo is Type)
		{
			return false;
		}
		MethodReference methodRef = mref as MethodReference;
		if (methodRef != null)
		{
			if (!(minfo is MethodBase methodBase))
			{
				return false;
			}
			Collection<ParameterDefinition> parameters = methodRef.Parameters;
			ParameterInfo[] parameters2 = methodBase.GetParameters();
			if (parameters.Count != parameters2.Length)
			{
				return false;
			}
			if (mref is GenericInstanceMethod genericInstanceMethod)
			{
				if (!methodBase.IsGenericMethod)
				{
					return false;
				}
				Collection<TypeReference> genericArguments4 = genericInstanceMethod.GenericArguments;
				Type[] genericArguments5 = methodBase.GetGenericArguments();
				if (genericArguments4.Count != genericArguments5.Length)
				{
					return false;
				}
				for (int k = 0; k < genericArguments4.Count; k++)
				{
					if (!genericArguments4[k].Is(genericArguments5[k]))
					{
						return false;
					}
				}
				return genericInstanceMethod.ElementMethod.Is((methodBase as MethodInfo)?.GetGenericMethodDefinition() ?? methodBase);
			}
			if (methodRef.HasGenericParameters)
			{
				if (!methodBase.IsGenericMethod)
				{
					return false;
				}
				Collection<GenericParameter> genericParameters2 = methodRef.GenericParameters;
				Type[] genericArguments6 = methodBase.GetGenericArguments();
				if (genericParameters2.Count != genericArguments6.Length)
				{
					return false;
				}
				for (int l = 0; l < genericParameters2.Count; l++)
				{
					if (!genericParameters2[l].Is(genericArguments6[l]))
					{
						return false;
					}
				}
			}
			else if (methodBase.IsGenericMethod)
			{
				return false;
			}
			Relinker relinker = null;
			relinker = (IMetadataTokenProvider paramMemberRef, IGenericParameterProvider? ctx) => (!(paramMemberRef is TypeReference paramTypeRef2)) ? paramMemberRef : ResolveParameter(paramTypeRef2);
			if (!methodRef.ReturnType.Relink(relinker, null).Is((methodBase as MethodInfo)?.ReturnType ?? typeof(void)) && !methodRef.ReturnType.Is((methodBase as MethodInfo)?.ReturnType ?? typeof(void)))
			{
				return false;
			}
			for (int m = 0; m < parameters.Count; m++)
			{
				if (!parameters[m].ParameterType.Relink(relinker, null).Is(parameters2[m].ParameterType) && !parameters[m].ParameterType.Is(parameters2[m].ParameterType))
				{
					return false;
				}
			}
			return true;
		}
		if (minfo is MethodInfo)
		{
			return false;
		}
		if (mref is FieldReference != minfo is FieldInfo)
		{
			return false;
		}
		if (mref is PropertyReference != minfo is PropertyInfo)
		{
			return false;
		}
		if (mref is EventReference != minfo is EventInfo)
		{
			return false;
		}
		return true;
		TypeReference ResolveParameter(TypeReference paramTypeRef)
		{
			if (paramTypeRef is GenericParameter genericParameter2)
			{
				if (genericParameter2.Owner is MethodReference && methodRef is GenericInstanceMethod genericInstanceMethod2)
				{
					return genericInstanceMethod2.GenericArguments[genericParameter2.Position];
				}
				if (genericParameter2.Owner is TypeReference typeReference3 && methodRef.DeclaringType is GenericInstanceType genericInstanceType2 && typeReference3.FullName == genericInstanceType2.ElementType.FullName)
				{
					return genericInstanceType2.GenericArguments[genericParameter2.Position];
				}
				return paramTypeRef;
			}
			if (paramTypeRef == methodRef.DeclaringType.GetElementType())
			{
				return methodRef.DeclaringType;
			}
			return paramTypeRef;
		}
	}

	public static IMetadataTokenProvider ImportReference(this ModuleDefinition mod, IMetadataTokenProvider mtp)
	{
		Helpers.ThrowIfArgumentNull(mod, "mod");
		if (mtp is TypeReference type)
		{
			return mod.ImportReference(type);
		}
		if (mtp is FieldReference field)
		{
			return mod.ImportReference(field);
		}
		if (mtp is MethodReference method)
		{
			return mod.ImportReference(method);
		}
		return mtp;
	}

	public static void AddRange<T>(this Collection<T> list, IEnumerable<T> other)
	{
		Helpers.ThrowIfArgumentNull(list, "list");
		foreach (T item in Helpers.ThrowIfNull(other, "other"))
		{
			list.Add(item);
		}
	}

	public static void AddRange(this IDictionary dict, IDictionary other)
	{
		Helpers.ThrowIfArgumentNull(dict, "dict");
		foreach (DictionaryEntry item in Helpers.ThrowIfNull(other, "other"))
		{
			dict.Add(item.Key, item.Value);
		}
	}

	public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> other)
	{
		Helpers.ThrowIfArgumentNull(dict, "dict");
		foreach (KeyValuePair<TKey, TValue> item in Helpers.ThrowIfNull(other, "other"))
		{
			dict.Add(item.Key, item.Value);
		}
	}

	public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dict, Dictionary<TKey, TValue> other) where TKey : notnull
	{
		Helpers.ThrowIfArgumentNull(dict, "dict");
		foreach (KeyValuePair<TKey, TValue> item in Helpers.ThrowIfNull(other, "other"))
		{
			dict.Add(item.Key, item.Value);
		}
	}

	public static void InsertRange<T>(this Collection<T> list, int index, IEnumerable<T> other)
	{
		Helpers.ThrowIfArgumentNull(list, "list");
		foreach (T item in Helpers.ThrowIfNull(other, "other"))
		{
			list.Insert(index++, item);
		}
	}

	public static bool IsCompatible(this Type type, Type other)
	{
		if (!Helpers.ThrowIfNull(type, "type")._IsCompatible(Helpers.ThrowIfNull(other, "other")))
		{
			return other._IsCompatible(type);
		}
		return true;
	}

	private static bool _IsCompatible(this Type type, Type other)
	{
		if (type == other)
		{
			return true;
		}
		if (other.IsEnum && type == typeof(Enum))
		{
			return false;
		}
		if (other.IsValueType && type == typeof(ValueType))
		{
			return false;
		}
		if (type.IsAssignableFrom(other))
		{
			return true;
		}
		if (other.IsEnum && type.IsCompatible(Enum.GetUnderlyingType(other)))
		{
			return true;
		}
		if ((other.IsPointer || other.IsByRef) && type == typeof(IntPtr))
		{
			return true;
		}
		if (type.IsPointer && other.IsPointer)
		{
			return true;
		}
		if (type.IsByRef && other.IsPointer)
		{
			return true;
		}
		return false;
	}

	public static T GetDeclaredMember<T>(this T member) where T : MemberInfo
	{
		Helpers.ThrowIfArgumentNull(member, "member");
		if (member.DeclaringType == member.ReflectedType)
		{
			return member;
		}
		if ((object)member.DeclaringType != null)
		{
			int metadataToken = member.MetadataToken;
			MemberInfo[] members = member.DeclaringType.GetMembers((BindingFlags)(-1));
			foreach (MemberInfo memberInfo in members)
			{
				if (memberInfo.MetadataToken == metadataToken)
				{
					return (T)memberInfo;
				}
			}
		}
		return member;
	}

	public unsafe static void SetMonoCorlibInternal(this Assembly asm, bool value)
	{
		if (PlatformDetection.Runtime != RuntimeKind.Mono)
		{
			return;
		}
		Helpers.ThrowIfArgumentNull(asm, "asm");
		Type type = asm.GetType();
		if (type == null)
		{
			return;
		}
		FieldInfo value2;
		lock (fmap_mono_assembly)
		{
			if (!fmap_mono_assembly.TryGetValue(type, out value2))
			{
				value2 = type.GetField("_mono_assembly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? type.GetField("dynamic_assembly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Could not find assembly field for Mono");
				fmap_mono_assembly[type] = value2;
			}
		}
		if (value2 == null)
		{
			return;
		}
		AssemblyName name = asm.GetName();
		lock (ReflectionHelper.AssemblyCache)
		{
			WeakReference value3 = new WeakReference(asm);
			ReflectionHelper.AssemblyCache[asm.GetRuntimeHashedFullName()] = value3;
			ReflectionHelper.AssemblyCache[name.FullName] = value3;
			if (name.Name != null)
			{
				ReflectionHelper.AssemblyCache[name.Name] = value3;
			}
		}
		long num = 0L;
		object value4 = value2.GetValue(asm);
		if (!(value4 is IntPtr intPtr))
		{
			if (value4 is UIntPtr uIntPtr)
			{
				num = (long)(ulong)uIntPtr;
			}
		}
		else
		{
			num = (long)intPtr;
		}
		int num2 = IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + 20 + 4 + 4 + 4 + (_MonoAssemblyNameHasArch ? ((!ReflectionHelper.IsCoreBCL) ? ((IntPtr.Size == 4) ? 12 : 16) : ((IntPtr.Size == 4) ? 20 : 24)) : (ReflectionHelper.IsCoreBCL ? 16 : 8)) + IntPtr.Size + IntPtr.Size + 1 + 1 + 1;
		byte* ptr = (byte*)(num + num2);
		*ptr = (value ? ((byte)1) : ((byte)0));
	}

	public static bool IsDynamicMethod(this MethodBase method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		if (_RTDynamicMethod != null)
		{
			if (!(method is DynamicMethod))
			{
				return method.GetType() == _RTDynamicMethod;
			}
			return true;
		}
		if (method is DynamicMethod)
		{
			return true;
		}
		if (method.MetadataToken != 0 || !method.IsStatic || !method.IsPublic || (method.Attributes & System.Reflection.MethodAttributes.PrivateScope) != 0)
		{
			return false;
		}
		if ((object)method.DeclaringType != null)
		{
			MethodInfo[] methods = method.DeclaringType.GetMethods(BindingFlags.Static | BindingFlags.Public);
			foreach (MethodInfo methodInfo in methods)
			{
				if (method == methodInfo)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static object? SafeGetTarget(this WeakReference weak)
	{
		Helpers.ThrowIfArgumentNull(weak, "weak");
		try
		{
			return weak.Target;
		}
		catch (InvalidOperationException)
		{
			return null;
		}
	}

	public static bool SafeGetIsAlive(this WeakReference weak)
	{
		Helpers.ThrowIfArgumentNull(weak, "weak");
		try
		{
			return weak.IsAlive;
		}
		catch (InvalidOperationException)
		{
			return false;
		}
	}

	public static T CreateDelegate<T>(this MethodBase method) where T : Delegate
	{
		return (T)method.CreateDelegate(typeof(T), null);
	}

	public static T CreateDelegate<T>(this MethodBase method, object? target) where T : Delegate
	{
		return (T)method.CreateDelegate(typeof(T), target);
	}

	public static Delegate CreateDelegate(this MethodBase method, Type delegateType)
	{
		return method.CreateDelegate(delegateType, null);
	}

	public static Delegate CreateDelegate(this MethodBase method, Type delegateType, object? target)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		Helpers.ThrowIfArgumentNull(delegateType, "delegateType");
		if (!typeof(Delegate).IsAssignableFrom(delegateType))
		{
			throw new ArgumentException("Type argument must be a delegate type!");
		}
		if (method is DynamicMethod dynamicMethod)
		{
			return dynamicMethod.CreateDelegate(delegateType, target);
		}
		if (method is MethodInfo method2)
		{
			return Delegate.CreateDelegate(delegateType, target, method2);
		}
		RuntimeMethodHandle methodHandle = method.MethodHandle;
		RuntimeHelpers.PrepareMethod(methodHandle);
		IntPtr functionPointer = methodHandle.GetFunctionPointer();
		return (Delegate)Activator.CreateInstance(delegateType, target, functionPointer);
	}

	public static T? TryCreateDelegate<T>(this MethodInfo? mi) where T : Delegate
	{
		try
		{
			return ((object)mi != null) ? mi.CreateDelegate<T>() : null;
		}
		catch
		{
			return null;
		}
	}

	public static MethodDefinition? FindMethod(this TypeDefinition type, string id, bool simple = true)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		Helpers.ThrowIfArgumentNull(id, "id");
		if (simple && !id.Contains(' ', StringComparison.Ordinal))
		{
			foreach (MethodDefinition method in type.Methods)
			{
				if (method.GetID(null, null, withType: true, simple: true) == id)
				{
					return method;
				}
			}
			foreach (MethodDefinition method2 in type.Methods)
			{
				if (method2.GetID(null, null, withType: false, simple: true) == id)
				{
					return method2;
				}
			}
		}
		foreach (MethodDefinition method3 in type.Methods)
		{
			if (method3.GetID() == id)
			{
				return method3;
			}
		}
		foreach (MethodDefinition method4 in type.Methods)
		{
			if (method4.GetID(null, null, withType: false) == id)
			{
				return method4;
			}
		}
		return null;
	}

	public static MethodDefinition? FindMethodDeep(this TypeDefinition type, string id, bool simple = true)
	{
		MethodDefinition? methodDefinition = Helpers.ThrowIfNull(type, "type").FindMethod(id, simple);
		if (methodDefinition == null)
		{
			TypeReference baseType = type.BaseType;
			if (baseType == null)
			{
				return null;
			}
			TypeDefinition typeDefinition = baseType.Resolve();
			if (typeDefinition == null)
			{
				return null;
			}
			methodDefinition = typeDefinition.FindMethodDeep(id, simple);
		}
		return methodDefinition;
	}

	public static MethodInfo? FindMethod(this Type type, string id, bool simple = true)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		Helpers.ThrowIfArgumentNull(id, "id");
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		MethodInfo[] array;
		if (simple && !id.Contains(' ', StringComparison.Ordinal))
		{
			array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				if (methodInfo.GetID(null, null, withType: true, proxyMethod: false, simple: true) == id)
				{
					return methodInfo;
				}
			}
			array = methods;
			foreach (MethodInfo methodInfo2 in array)
			{
				if (methodInfo2.GetID(null, null, withType: false, proxyMethod: false, simple: true) == id)
				{
					return methodInfo2;
				}
			}
		}
		array = methods;
		foreach (MethodInfo methodInfo3 in array)
		{
			if (methodInfo3.GetID() == id)
			{
				return methodInfo3;
			}
		}
		array = methods;
		foreach (MethodInfo methodInfo4 in array)
		{
			if (methodInfo4.GetID(null, null, withType: false) == id)
			{
				return methodInfo4;
			}
		}
		return null;
	}

	public static MethodInfo? FindMethodDeep(this Type type, string id, bool simple = true)
	{
		MethodInfo? methodInfo = type.FindMethod(id, simple);
		if ((object)methodInfo == null)
		{
			Type baseType = type.BaseType;
			if ((object)baseType == null)
			{
				return null;
			}
			methodInfo = baseType.FindMethodDeep(id, simple);
		}
		return methodInfo;
	}

	public static PropertyDefinition? FindProperty(this TypeDefinition type, string name)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		foreach (PropertyDefinition property in type.Properties)
		{
			if (property.Name == name)
			{
				return property;
			}
		}
		return null;
	}

	public static PropertyDefinition? FindPropertyDeep(this TypeDefinition type, string name)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		PropertyDefinition? propertyDefinition = type.FindProperty(name);
		if (propertyDefinition == null)
		{
			TypeReference baseType = type.BaseType;
			if (baseType == null)
			{
				return null;
			}
			TypeDefinition typeDefinition = baseType.Resolve();
			if (typeDefinition == null)
			{
				return null;
			}
			propertyDefinition = typeDefinition.FindPropertyDeep(name);
		}
		return propertyDefinition;
	}

	public static FieldDefinition? FindField(this TypeDefinition type, string name)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		foreach (FieldDefinition field in type.Fields)
		{
			if (field.Name == name)
			{
				return field;
			}
		}
		return null;
	}

	public static FieldDefinition? FindFieldDeep(this TypeDefinition type, string name)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		FieldDefinition? fieldDefinition = type.FindField(name);
		if (fieldDefinition == null)
		{
			TypeReference baseType = type.BaseType;
			if (baseType == null)
			{
				return null;
			}
			TypeDefinition typeDefinition = baseType.Resolve();
			if (typeDefinition == null)
			{
				return null;
			}
			fieldDefinition = typeDefinition.FindFieldDeep(name);
		}
		return fieldDefinition;
	}

	public static EventDefinition? FindEvent(this TypeDefinition type, string name)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		foreach (EventDefinition @event in type.Events)
		{
			if (@event.Name == name)
			{
				return @event;
			}
		}
		return null;
	}

	public static EventDefinition? FindEventDeep(this TypeDefinition type, string name)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		EventDefinition? eventDefinition = type.FindEvent(name);
		if (eventDefinition == null)
		{
			TypeReference baseType = type.BaseType;
			if (baseType == null)
			{
				return null;
			}
			TypeDefinition typeDefinition = baseType.Resolve();
			if (typeDefinition == null)
			{
				return null;
			}
			eventDefinition = typeDefinition.FindEventDeep(name);
		}
		return eventDefinition;
	}

	public static string GetID(this MethodReference method, string? name = null, string? type = null, bool withType = true, bool simple = false)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		StringBuilder stringBuilder = new StringBuilder();
		if (simple)
		{
			if (withType && (type != null || method.DeclaringType != null))
			{
				stringBuilder.Append(type ?? method.DeclaringType.GetPatchFullName()).Append("::");
			}
			stringBuilder.Append(name ?? method.Name);
			return stringBuilder.ToString();
		}
		stringBuilder.Append(method.ReturnType.GetPatchFullName()).Append(' ');
		if (withType && (type != null || method.DeclaringType != null))
		{
			stringBuilder.Append(type ?? method.DeclaringType.GetPatchFullName()).Append("::");
		}
		stringBuilder.Append(name ?? method.Name);
		if (method is GenericInstanceMethod genericInstanceMethod && genericInstanceMethod.GenericArguments.Count != 0)
		{
			stringBuilder.Append('<');
			Collection<TypeReference> genericArguments = genericInstanceMethod.GenericArguments;
			for (int i = 0; i < genericArguments.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(genericArguments[i].GetPatchFullName());
			}
			stringBuilder.Append('>');
		}
		else if (method.GenericParameters.Count != 0)
		{
			stringBuilder.Append('<');
			Collection<GenericParameter> genericParameters = method.GenericParameters;
			for (int j = 0; j < genericParameters.Count; j++)
			{
				if (j > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(genericParameters[j].Name);
			}
			stringBuilder.Append('>');
		}
		stringBuilder.Append('(');
		if (method.HasParameters)
		{
			Collection<ParameterDefinition> parameters = method.Parameters;
			for (int k = 0; k < parameters.Count; k++)
			{
				ParameterDefinition parameterDefinition = parameters[k];
				if (k > 0)
				{
					stringBuilder.Append(',');
				}
				if (parameterDefinition.ParameterType.IsSentinel)
				{
					stringBuilder.Append("...,");
				}
				stringBuilder.Append(parameterDefinition.ParameterType.GetPatchFullName());
			}
		}
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}

	public static string GetID(this Mono.Cecil.CallSite method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(method.ReturnType.GetPatchFullName()).Append(' ');
		stringBuilder.Append('(');
		if (method.HasParameters)
		{
			Collection<ParameterDefinition> parameters = method.Parameters;
			for (int i = 0; i < parameters.Count; i++)
			{
				ParameterDefinition parameterDefinition = parameters[i];
				if (i > 0)
				{
					stringBuilder.Append(',');
				}
				if (parameterDefinition.ParameterType.IsSentinel)
				{
					stringBuilder.Append("...,");
				}
				stringBuilder.Append(parameterDefinition.ParameterType.GetPatchFullName());
			}
		}
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}

	public static string GetID(this MethodBase method, string? name = null, string? type = null, bool withType = true, bool proxyMethod = false, bool simple = false)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		while (method is MethodInfo methodInfo && method.IsGenericMethod && !method.IsGenericMethodDefinition)
		{
			method = methodInfo.GetGenericMethodDefinition();
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (simple)
		{
			if (withType && (type != null || method.DeclaringType != null))
			{
				stringBuilder.Append(type ?? method.DeclaringType.FullName).Append("::");
			}
			stringBuilder.Append(name ?? method.Name);
			return stringBuilder.ToString();
		}
		stringBuilder.Append((method as MethodInfo)?.ReturnType?.FullName ?? "System.Void").Append(' ');
		if (withType && (type != null || method.DeclaringType != null))
		{
			stringBuilder.Append(type ?? method.DeclaringType.FullName?.Replace("+", "/", StringComparison.Ordinal)).Append("::");
		}
		stringBuilder.Append(name ?? method.Name);
		if (method.ContainsGenericParameters)
		{
			stringBuilder.Append('<');
			Type[] genericArguments = method.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(genericArguments[i].Name);
			}
			stringBuilder.Append('>');
		}
		stringBuilder.Append('(');
		ParameterInfo[] parameters = method.GetParameters();
		for (int j = (proxyMethod ? 1 : 0); j < parameters.Length; j++)
		{
			ParameterInfo parameterInfo = parameters[j];
			if (j > (proxyMethod ? 1 : 0))
			{
				stringBuilder.Append(',');
			}
			bool flag;
			try
			{
				flag = parameterInfo.GetCustomAttributes(t_ParamArrayAttribute, inherit: false).Length != 0;
			}
			catch (NotSupportedException)
			{
				flag = false;
			}
			if (flag)
			{
				stringBuilder.Append("...,");
			}
			stringBuilder.Append(parameterInfo.ParameterType.FullName);
		}
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}

	public static string GetPatchName(this MemberReference mr)
	{
		Helpers.ThrowIfArgumentNull(mr, "mr");
		Mono.Cecil.ICustomAttributeProvider obj = mr as Mono.Cecil.ICustomAttributeProvider;
		return ((obj != null) ? obj.GetPatchName() : null) ?? mr.Name;
	}

	public static string GetPatchFullName(this MemberReference mr)
	{
		Helpers.ThrowIfArgumentNull(mr, "mr");
		Mono.Cecil.ICustomAttributeProvider obj = mr as Mono.Cecil.ICustomAttributeProvider;
		return ((obj != null) ? obj.GetPatchFullName(mr) : null) ?? mr.FullName;
	}

	private static string GetPatchName(this Mono.Cecil.ICustomAttributeProvider cap)
	{
		Helpers.ThrowIfArgumentNull(cap, "cap");
		CustomAttribute customAttribute = cap.GetCustomAttribute("MonoMod.MonoModPatch");
		string text;
		if (customAttribute != null)
		{
			text = (string)customAttribute.ConstructorArguments[0].Value;
			int num = text.LastIndexOf('.');
			if (num != -1 && num != text.Length - 1)
			{
				text = text.Substring(num + 1);
			}
			return text;
		}
		text = ((MemberReference)cap).Name;
		if (!text.StartsWith("patch_", StringComparison.Ordinal))
		{
			return text;
		}
		return text.Substring(6);
	}

	private static string GetPatchFullName(this Mono.Cecil.ICustomAttributeProvider cap, MemberReference mr)
	{
		Helpers.ThrowIfArgumentNull(cap, "cap");
		Helpers.ThrowIfArgumentNull(mr, "mr");
		if (cap is TypeReference typeReference)
		{
			CustomAttribute customAttribute = cap.GetCustomAttribute("MonoMod.MonoModPatch");
			string text;
			if (customAttribute != null)
			{
				text = (string)customAttribute.ConstructorArguments[0].Value;
			}
			else
			{
				text = ((MemberReference)cap).Name;
				text = (text.StartsWith("patch_", StringComparison.Ordinal) ? text.Substring(6) : text);
			}
			if (text.StartsWith("global::", StringComparison.Ordinal))
			{
				text = text.Substring(8);
			}
			else if (!text.Contains('.', StringComparison.Ordinal) && !text.Contains('/', StringComparison.Ordinal))
			{
				if (!string.IsNullOrEmpty(typeReference.Namespace))
				{
					text = typeReference.Namespace + "." + text;
				}
				else if (typeReference.IsNested)
				{
					text = typeReference.DeclaringType.GetPatchFullName() + "/" + text;
				}
			}
			if (mr is TypeSpecification typeSpecification)
			{
				List<TypeSpecification> list = new List<TypeSpecification>();
				TypeSpecification typeSpecification2 = typeSpecification;
				do
				{
					list.Add(typeSpecification2);
				}
				while ((typeSpecification2 = typeSpecification2.ElementType as TypeSpecification) != null);
				StringBuilder stringBuilder = new StringBuilder(text.Length + list.Count * 4);
				stringBuilder.Append(text);
				for (int num = list.Count - 1; num > -1; num--)
				{
					typeSpecification2 = list[num];
					if (typeSpecification2.IsByReference)
					{
						stringBuilder.Append('&');
					}
					else if (typeSpecification2.IsPointer)
					{
						stringBuilder.Append('*');
					}
					else if (!typeSpecification2.IsPinned && !typeSpecification2.IsSentinel)
					{
						if (typeSpecification2.IsArray)
						{
							ArrayType arrayType = (ArrayType)typeSpecification2;
							if (arrayType.IsVector)
							{
								stringBuilder.Append("[]");
							}
							else
							{
								stringBuilder.Append('[');
								for (int i = 0; i < arrayType.Dimensions.Count; i++)
								{
									if (i > 0)
									{
										stringBuilder.Append(',');
									}
									stringBuilder.Append(arrayType.Dimensions[i].ToString());
								}
								stringBuilder.Append(']');
							}
						}
						else if (typeSpecification2.IsRequiredModifier)
						{
							stringBuilder.Append("modreq(").Append(((RequiredModifierType)typeSpecification2).ModifierType).Append(')');
						}
						else if (typeSpecification2.IsOptionalModifier)
						{
							stringBuilder.Append("modopt(").Append(((OptionalModifierType)typeSpecification2).ModifierType).Append(')');
						}
						else if (typeSpecification2.IsGenericInstance)
						{
							GenericInstanceType genericInstanceType = (GenericInstanceType)typeSpecification2;
							stringBuilder.Append('<');
							for (int j = 0; j < genericInstanceType.GenericArguments.Count; j++)
							{
								if (j > 0)
								{
									stringBuilder.Append(',');
								}
								stringBuilder.Append(genericInstanceType.GenericArguments[j].GetPatchFullName());
							}
							stringBuilder.Append('>');
						}
						else
						{
							if (!typeSpecification2.IsFunctionPointer)
							{
								throw new NotSupportedException($"MonoMod can't handle TypeSpecification: {typeReference.FullName} ({typeReference.GetType()})");
							}
							FunctionPointerType functionPointerType = (FunctionPointerType)typeSpecification2;
							stringBuilder.Append(' ').Append(functionPointerType.ReturnType.GetPatchFullName()).Append(" *(");
							if (functionPointerType.HasParameters)
							{
								for (int k = 0; k < functionPointerType.Parameters.Count; k++)
								{
									ParameterDefinition parameterDefinition = functionPointerType.Parameters[k];
									if (k > 0)
									{
										stringBuilder.Append(',');
									}
									if (parameterDefinition.ParameterType.IsSentinel)
									{
										stringBuilder.Append("...,");
									}
									stringBuilder.Append(parameterDefinition.ParameterType.FullName);
								}
							}
							stringBuilder.Append(')');
						}
					}
				}
				text = stringBuilder.ToString();
			}
			return text;
		}
		if (cap is FieldReference fieldReference)
		{
			return $"{fieldReference.FieldType.GetPatchFullName()} {fieldReference.DeclaringType.GetPatchFullName()}::{cap.GetPatchName()}";
		}
		if (cap is MethodReference)
		{
			throw new InvalidOperationException("GetPatchFullName not supported on MethodReferences - use GetID instead");
		}
		throw new InvalidOperationException($"GetPatchFullName not supported on type {cap.GetType()}");
	}

	[return: NotNullIfNotNull("o")]
	public static MethodDefinition? Clone(this MethodDefinition? o, MethodDefinition? c = null)
	{
		if (o == null)
		{
			return null;
		}
		if (c == null)
		{
			c = new MethodDefinition(o.Name, o.Attributes, o.ReturnType);
		}
		c.Name = o.Name;
		c.Attributes = o.Attributes;
		c.ReturnType = o.ReturnType;
		c.DeclaringType = o.DeclaringType;
		c.MetadataToken = c.MetadataToken;
		c.Body = o.Body?.Clone(c);
		c.Attributes = o.Attributes;
		c.ImplAttributes = o.ImplAttributes;
		c.PInvokeInfo = o.PInvokeInfo;
		c.IsPreserveSig = o.IsPreserveSig;
		c.IsPInvokeImpl = o.IsPInvokeImpl;
		foreach (GenericParameter genericParameter in o.GenericParameters)
		{
			c.GenericParameters.Add(genericParameter.Clone());
		}
		foreach (ParameterDefinition parameter in o.Parameters)
		{
			c.Parameters.Add(parameter.Clone());
		}
		foreach (CustomAttribute customAttribute in o.CustomAttributes)
		{
			c.CustomAttributes.Add(customAttribute.Clone());
		}
		foreach (MethodReference @override in o.Overrides)
		{
			c.Overrides.Add(@override);
		}
		if (c.Body != null)
		{
			foreach (Instruction instruction in c.Body.Instructions)
			{
				int index;
				if (instruction.Operand is GenericParameter item && (index = o.GenericParameters.IndexOf(item)) != -1)
				{
					instruction.Operand = c.GenericParameters[index];
				}
				else if (instruction.Operand is ParameterDefinition item2 && (index = o.Parameters.IndexOf(item2)) != -1)
				{
					instruction.Operand = c.Parameters[index];
				}
			}
		}
		return c;
	}

	[return: NotNullIfNotNull("bo")]
	public static Mono.Cecil.Cil.MethodBody? Clone(this Mono.Cecil.Cil.MethodBody? bo, MethodDefinition m)
	{
		Helpers.ThrowIfArgumentNull(m, "m");
		if (bo == null)
		{
			return null;
		}
		Mono.Cecil.Cil.MethodBody bc = new Mono.Cecil.Cil.MethodBody(m);
		bc.MaxStackSize = bo.MaxStackSize;
		bc.InitLocals = bo.InitLocals;
		bc.LocalVarToken = bo.LocalVarToken;
		bc.Instructions.AddRange(bo.Instructions.Select(delegate(Instruction o)
		{
			Instruction instruction = Instruction.Create(Mono.Cecil.Cil.OpCodes.Nop);
			instruction.OpCode = o.OpCode;
			instruction.Operand = o.Operand;
			instruction.Offset = o.Offset;
			return instruction;
		}));
		foreach (Instruction instruction2 in bc.Instructions)
		{
			if (instruction2.Operand is Instruction item)
			{
				instruction2.Operand = bc.Instructions[bo.Instructions.IndexOf(item)];
			}
			else if (instruction2.Operand is Instruction[] source)
			{
				instruction2.Operand = source.Select((Instruction i) => bc.Instructions[bo.Instructions.IndexOf(i)]).ToArray();
			}
		}
		bc.ExceptionHandlers.AddRange(bo.ExceptionHandlers.Select((Mono.Cecil.Cil.ExceptionHandler o) => new Mono.Cecil.Cil.ExceptionHandler(o.HandlerType)
		{
			TryStart = ((o.TryStart == null) ? null : bc.Instructions[bo.Instructions.IndexOf(o.TryStart)]),
			TryEnd = ((o.TryEnd == null) ? null : bc.Instructions[bo.Instructions.IndexOf(o.TryEnd)]),
			FilterStart = ((o.FilterStart == null) ? null : bc.Instructions[bo.Instructions.IndexOf(o.FilterStart)]),
			HandlerStart = ((o.HandlerStart == null) ? null : bc.Instructions[bo.Instructions.IndexOf(o.HandlerStart)]),
			HandlerEnd = ((o.HandlerEnd == null) ? null : bc.Instructions[bo.Instructions.IndexOf(o.HandlerEnd)]),
			CatchType = o.CatchType
		}));
		bc.Variables.AddRange(bo.Variables.Select((VariableDefinition o) => new VariableDefinition(o.VariableType)));
		m.CustomDebugInformations.AddRange(bo.Method.CustomDebugInformations.Select(delegate(CustomDebugInformation o)
		{
			if (o is AsyncMethodBodyDebugInformation asyncMethodBodyDebugInformation)
			{
				AsyncMethodBodyDebugInformation asyncMethodBodyDebugInformation2 = new AsyncMethodBodyDebugInformation();
				if (asyncMethodBodyDebugInformation.CatchHandler.Offset >= 0)
				{
					asyncMethodBodyDebugInformation2.CatchHandler = (asyncMethodBodyDebugInformation.CatchHandler.IsEndOfMethod ? default(InstructionOffset) : new InstructionOffset(ResolveInstrOff(asyncMethodBodyDebugInformation.CatchHandler.Offset)));
				}
				asyncMethodBodyDebugInformation2.Yields.AddRange(asyncMethodBodyDebugInformation.Yields.Select((InstructionOffset off) => (!off.IsEndOfMethod) ? new InstructionOffset(ResolveInstrOff(off.Offset)) : default(InstructionOffset)));
				asyncMethodBodyDebugInformation2.Resumes.AddRange(asyncMethodBodyDebugInformation.Resumes.Select((InstructionOffset off) => (!off.IsEndOfMethod) ? new InstructionOffset(ResolveInstrOff(off.Offset)) : default(InstructionOffset)));
				asyncMethodBodyDebugInformation2.ResumeMethods.AddRange(asyncMethodBodyDebugInformation.ResumeMethods);
				return asyncMethodBodyDebugInformation2;
			}
			if (o is StateMachineScopeDebugInformation stateMachineScopeDebugInformation)
			{
				StateMachineScopeDebugInformation stateMachineScopeDebugInformation2 = new StateMachineScopeDebugInformation();
				stateMachineScopeDebugInformation2.Scopes.AddRange(stateMachineScopeDebugInformation.Scopes.Select((StateMachineScope s) => new StateMachineScope(ResolveInstrOff(s.Start.Offset), s.End.IsEndOfMethod ? null : ResolveInstrOff(s.End.Offset))));
				return stateMachineScopeDebugInformation2;
			}
			return o;
		}));
		m.DebugInformation.SequencePoints.AddRange(bo.Method.DebugInformation.SequencePoints.Select((SequencePoint o) => new SequencePoint(ResolveInstrOff(o.Offset), o.Document)
		{
			StartLine = o.StartLine,
			StartColumn = o.StartColumn,
			EndLine = o.EndLine,
			EndColumn = o.EndColumn
		}));
		return bc;
		Instruction ResolveInstrOff(int off)
		{
			for (int j = 0; j < bo.Instructions.Count; j++)
			{
				if (bo.Instructions[j].Offset == off)
				{
					return bc.Instructions[j];
				}
			}
			throw new ArgumentException($"Invalid instruction offset {off}");
		}
	}

	public static GenericParameter Update(this GenericParameter param, int position, GenericParameterType type)
	{
		f_GenericParameter_position.SetValue(param, position);
		f_GenericParameter_type.SetValue(param, type);
		return param;
	}

	public static GenericParameter? ResolveGenericParameter(this IGenericParameterProvider provider, GenericParameter orig)
	{
		Helpers.ThrowIfArgumentNull(provider, "provider");
		Helpers.ThrowIfArgumentNull(orig, "orig");
		if (provider is GenericParameter genericParameter && genericParameter.Name == orig.Name)
		{
			return genericParameter;
		}
		foreach (GenericParameter genericParameter2 in provider.GenericParameters)
		{
			if (genericParameter2.Name == orig.Name)
			{
				return genericParameter2;
			}
		}
		int position = orig.Position;
		if (provider is MethodReference && orig.DeclaringMethod != null)
		{
			if (position < provider.GenericParameters.Count)
			{
				return provider.GenericParameters[position];
			}
			return orig.Clone().Update(position, GenericParameterType.Method);
		}
		if (provider is TypeReference && orig.DeclaringType != null)
		{
			if (position < provider.GenericParameters.Count)
			{
				return provider.GenericParameters[position];
			}
			return orig.Clone().Update(position, GenericParameterType.Type);
		}
		object obj = (provider as TypeSpecification)?.ElementType.ResolveGenericParameter(orig);
		if (obj == null)
		{
			MemberReference obj2 = provider as MemberReference;
			if (obj2 == null)
			{
				return null;
			}
			TypeReference declaringType = obj2.DeclaringType;
			if (declaringType == null)
			{
				return null;
			}
			obj = declaringType.ResolveGenericParameter(orig);
		}
		return (GenericParameter?)obj;
	}

	[return: NotNullIfNotNull("mtp")]
	public static IMetadataTokenProvider? Relink(this IMetadataTokenProvider? mtp, Relinker relinker, IGenericParameterProvider context)
	{
		if (!(mtp is TypeReference type))
		{
			if (!(mtp is GenericParameterConstraint constraint))
			{
				if (!(mtp is MethodReference method))
				{
					if (!(mtp is FieldReference field))
					{
						if (!(mtp is ParameterDefinition param))
						{
							if (!(mtp is Mono.Cecil.CallSite method2))
							{
								if (mtp == null)
								{
									return null;
								}
								throw new InvalidOperationException($"MonoMod can't handle metadata token providers of the type {mtp.GetType()}");
							}
							return method2.Relink(relinker, context);
						}
						return param.Relink(relinker, context);
					}
					return field.Relink(relinker, context);
				}
				return method.Relink(relinker, context);
			}
			return constraint.Relink(relinker, context);
		}
		return type.Relink(relinker, context);
	}

	[return: NotNullIfNotNull("type")]
	public static TypeReference? Relink(this TypeReference? type, Relinker relinker, IGenericParameterProvider? context)
	{
		if (type == null)
		{
			return null;
		}
		Helpers.ThrowIfArgumentNull(relinker, "relinker");
		if (type is TypeSpecification typeSpecification)
		{
			TypeReference type2 = typeSpecification.ElementType.Relink(relinker, context);
			if (type.IsSentinel)
			{
				return new SentinelType(type2);
			}
			if (type.IsByReference)
			{
				return new ByReferenceType(type2);
			}
			if (type.IsPointer)
			{
				return new PointerType(type2);
			}
			if (type.IsPinned)
			{
				return new PinnedType(type2);
			}
			if (type.IsArray)
			{
				ArrayType arrayType = new ArrayType(type2, ((ArrayType)type).Rank);
				for (int i = 0; i < arrayType.Rank; i++)
				{
					arrayType.Dimensions[i] = ((ArrayType)type).Dimensions[i];
				}
				return arrayType;
			}
			if (type.IsRequiredModifier)
			{
				return new RequiredModifierType(((RequiredModifierType)type).ModifierType.Relink(relinker, context), type2);
			}
			if (type.IsOptionalModifier)
			{
				return new OptionalModifierType(((OptionalModifierType)type).ModifierType.Relink(relinker, context), type2);
			}
			if (type.IsGenericInstance)
			{
				GenericInstanceType genericInstanceType = new GenericInstanceType(type2);
				{
					foreach (TypeReference genericArgument in ((GenericInstanceType)type).GenericArguments)
					{
						genericInstanceType.GenericArguments.Add(genericArgument?.Relink(relinker, context));
					}
					return genericInstanceType;
				}
			}
			if (type.IsFunctionPointer)
			{
				FunctionPointerType functionPointerType = (FunctionPointerType)type;
				functionPointerType.ReturnType = functionPointerType.ReturnType.Relink(relinker, context);
				for (int j = 0; j < functionPointerType.Parameters.Count; j++)
				{
					functionPointerType.Parameters[j].ParameterType = functionPointerType.Parameters[j].ParameterType.Relink(relinker, context);
				}
				return functionPointerType;
			}
			throw new NotSupportedException($"MonoMod can't handle TypeSpecification: {type.FullName} ({type.GetType()})");
		}
		if (type.IsGenericParameter && context != null)
		{
			GenericParameter genericParameter = context.ResolveGenericParameter((GenericParameter)type) ?? throw new RelinkTargetNotFoundException($"{"MonoMod relinker failed finding"} {type.FullName} (context: {context})", type, context);
			for (int k = 0; k < genericParameter.Constraints.Count; k++)
			{
				if (!MultiTargetShims.GetConstraintType(genericParameter.Constraints[k]).IsGenericInstance)
				{
					genericParameter.Constraints[k] = genericParameter.Constraints[k].Relink(relinker, context);
				}
			}
			return genericParameter;
		}
		return (TypeReference)relinker(type, context);
	}

	[return: NotNullIfNotNull("constraint")]
	public static GenericParameterConstraint? Relink(this GenericParameterConstraint? constraint, Relinker relinker, IGenericParameterProvider context)
	{
		if (constraint == null)
		{
			return null;
		}
		GenericParameterConstraint genericParameterConstraint = new GenericParameterConstraint(constraint.ConstraintType.Relink(relinker, context));
		foreach (CustomAttribute customAttribute in constraint.CustomAttributes)
		{
			genericParameterConstraint.CustomAttributes.Add(customAttribute.Relink(relinker, context));
		}
		return genericParameterConstraint;
	}

	public static IMetadataTokenProvider Relink(this MethodReference method, Relinker relinker, IGenericParameterProvider context)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		Helpers.ThrowIfArgumentNull(relinker, "relinker");
		if (method.IsGenericInstance)
		{
			GenericInstanceMethod obj = (GenericInstanceMethod)method;
			GenericInstanceMethod genericInstanceMethod = new GenericInstanceMethod((MethodReference)obj.ElementMethod.Relink(relinker, context));
			foreach (TypeReference genericArgument in obj.GenericArguments)
			{
				genericInstanceMethod.GenericArguments.Add(genericArgument.Relink(relinker, context));
			}
			return (MethodReference)relinker(genericInstanceMethod, context);
		}
		MethodReference methodReference = new MethodReference(method.Name, method.ReturnType, method.DeclaringType.Relink(relinker, context));
		methodReference.CallingConvention = method.CallingConvention;
		methodReference.ExplicitThis = method.ExplicitThis;
		methodReference.HasThis = method.HasThis;
		foreach (GenericParameter genericParameter in method.GenericParameters)
		{
			methodReference.GenericParameters.Add(genericParameter.Relink(relinker, context));
		}
		methodReference.ReturnType = methodReference.ReturnType?.Relink(relinker, methodReference);
		foreach (ParameterDefinition parameter in method.Parameters)
		{
			parameter.ParameterType = parameter.ParameterType.Relink(relinker, method);
			methodReference.Parameters.Add(parameter);
		}
		return (MethodReference)relinker(methodReference, context);
	}

	public static Mono.Cecil.CallSite Relink(this Mono.Cecil.CallSite method, Relinker relinker, IGenericParameterProvider context)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		Helpers.ThrowIfArgumentNull(relinker, "relinker");
		Mono.Cecil.CallSite callSite = new Mono.Cecil.CallSite(method.ReturnType);
		callSite.CallingConvention = method.CallingConvention;
		callSite.ExplicitThis = method.ExplicitThis;
		callSite.HasThis = method.HasThis;
		callSite.ReturnType = callSite.ReturnType?.Relink(relinker, context);
		foreach (ParameterDefinition parameter in method.Parameters)
		{
			parameter.ParameterType = parameter.ParameterType.Relink(relinker, context);
			callSite.Parameters.Add(parameter);
		}
		return (Mono.Cecil.CallSite)relinker(callSite, context);
	}

	public static IMetadataTokenProvider Relink(this FieldReference field, Relinker relinker, IGenericParameterProvider context)
	{
		Helpers.ThrowIfArgumentNull(field, "field");
		Helpers.ThrowIfArgumentNull(relinker, "relinker");
		TypeReference typeReference = field.DeclaringType.Relink(relinker, context);
		return relinker(new FieldReference(field.Name, field.FieldType.Relink(relinker, typeReference), typeReference), context);
	}

	public static ParameterDefinition Relink(this ParameterDefinition param, Relinker relinker, IGenericParameterProvider context)
	{
		Helpers.ThrowIfArgumentNull(param, "param");
		Helpers.ThrowIfArgumentNull(relinker, "relinker");
		param = (param.Method as MethodReference)?.Parameters[param.Index] ?? param;
		ParameterDefinition parameterDefinition = new ParameterDefinition(param.Name, param.Attributes, param.ParameterType.Relink(relinker, context))
		{
			IsIn = param.IsIn,
			IsLcid = param.IsLcid,
			IsOptional = param.IsOptional,
			IsOut = param.IsOut,
			IsReturnValue = param.IsReturnValue,
			MarshalInfo = param.MarshalInfo
		};
		if (param.HasConstant)
		{
			parameterDefinition.Constant = param.Constant;
		}
		return parameterDefinition;
	}

	public static ParameterDefinition Clone(this ParameterDefinition param)
	{
		Helpers.ThrowIfArgumentNull(param, "param");
		ParameterDefinition parameterDefinition = new ParameterDefinition(param.Name, param.Attributes, param.ParameterType)
		{
			IsIn = param.IsIn,
			IsLcid = param.IsLcid,
			IsOptional = param.IsOptional,
			IsOut = param.IsOut,
			IsReturnValue = param.IsReturnValue,
			MarshalInfo = param.MarshalInfo
		};
		if (param.HasConstant)
		{
			parameterDefinition.Constant = param.Constant;
		}
		foreach (CustomAttribute customAttribute in param.CustomAttributes)
		{
			parameterDefinition.CustomAttributes.Add(customAttribute.Clone());
		}
		return parameterDefinition;
	}

	public static CustomAttribute Relink(this CustomAttribute attrib, Relinker relinker, IGenericParameterProvider context)
	{
		Helpers.ThrowIfArgumentNull(attrib, "attrib");
		Helpers.ThrowIfArgumentNull(relinker, "relinker");
		CustomAttribute customAttribute = new CustomAttribute((MethodReference)attrib.Constructor.Relink(relinker, context));
		foreach (CustomAttributeArgument constructorArgument in attrib.ConstructorArguments)
		{
			customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(constructorArgument.Type.Relink(relinker, context), constructorArgument.Value));
		}
		foreach (Mono.Cecil.CustomAttributeNamedArgument field in attrib.Fields)
		{
			customAttribute.Fields.Add(new Mono.Cecil.CustomAttributeNamedArgument(field.Name, new CustomAttributeArgument(field.Argument.Type.Relink(relinker, context), field.Argument.Value)));
		}
		foreach (Mono.Cecil.CustomAttributeNamedArgument property in attrib.Properties)
		{
			customAttribute.Properties.Add(new Mono.Cecil.CustomAttributeNamedArgument(property.Name, new CustomAttributeArgument(property.Argument.Type.Relink(relinker, context), property.Argument.Value)));
		}
		return customAttribute;
	}

	public static CustomAttribute Clone(this CustomAttribute attrib)
	{
		Helpers.ThrowIfArgumentNull(attrib, "attrib");
		CustomAttribute customAttribute = new CustomAttribute(attrib.Constructor);
		foreach (CustomAttributeArgument constructorArgument in attrib.ConstructorArguments)
		{
			customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(constructorArgument.Type, constructorArgument.Value));
		}
		foreach (Mono.Cecil.CustomAttributeNamedArgument field in attrib.Fields)
		{
			customAttribute.Fields.Add(new Mono.Cecil.CustomAttributeNamedArgument(field.Name, new CustomAttributeArgument(field.Argument.Type, field.Argument.Value)));
		}
		foreach (Mono.Cecil.CustomAttributeNamedArgument property in attrib.Properties)
		{
			customAttribute.Properties.Add(new Mono.Cecil.CustomAttributeNamedArgument(property.Name, new CustomAttributeArgument(property.Argument.Type, property.Argument.Value)));
		}
		return customAttribute;
	}

	public static GenericParameter Relink(this GenericParameter param, Relinker relinker, IGenericParameterProvider context)
	{
		Helpers.ThrowIfArgumentNull(param, "param");
		Helpers.ThrowIfArgumentNull(relinker, "relinker");
		GenericParameter genericParameter = new GenericParameter(param.Name, param.Owner)
		{
			Attributes = param.Attributes
		}.Update(param.Position, param.Type);
		foreach (CustomAttribute customAttribute in param.CustomAttributes)
		{
			genericParameter.CustomAttributes.Add(customAttribute.Relink(relinker, context));
		}
		foreach (GenericParameterConstraint constraint in param.Constraints)
		{
			genericParameter.Constraints.Add(constraint.Relink(relinker, context));
		}
		return genericParameter;
	}

	public static GenericParameter Clone(this GenericParameter param)
	{
		Helpers.ThrowIfArgumentNull(param, "param");
		GenericParameter genericParameter = new GenericParameter(param.Name, param.Owner)
		{
			Attributes = param.Attributes
		}.Update(param.Position, param.Type);
		foreach (CustomAttribute customAttribute in param.CustomAttributes)
		{
			genericParameter.CustomAttributes.Add(customAttribute.Clone());
		}
		foreach (GenericParameterConstraint constraint in param.Constraints)
		{
			genericParameter.Constraints.Add(constraint);
		}
		return genericParameter;
	}

	public static int GetManagedSize(this Type t)
	{
		return _GetManagedSizeCache.GetOrAdd(Helpers.ThrowIfNull(t, "t"), ComputeManagedSize);
	}

	private static int ComputeManagedSize(Type t)
	{
		MethodInfo methodInfo = _GetManagedSizeHelper;
		if ((object)methodInfo == null)
		{
			methodInfo = (_GetManagedSizeHelper = typeof(Unsafe).GetMethod("SizeOf"));
		}
		return methodInfo.MakeGenericMethod(t).CreateDelegate<Func<int>>()();
	}

	public static Type GetThisParamType(this MethodBase method)
	{
		Type type = Helpers.ThrowIfNull(method, "method").DeclaringType;
		if (type.IsValueType)
		{
			type = type.MakeByRefType();
		}
		return type;
	}

	public static IntPtr GetLdftnPointer(this MethodBase m)
	{
		Helpers.ThrowIfArgumentNull(m, "m");
		if (_GetLdftnPointerCache.TryGetValue(m, out Func<IntPtr> value))
		{
			return value();
		}
		FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(17, 1);
		handler.AppendLiteral("GetLdftnPointer<");
		handler.AppendFormatted(m);
		handler.AppendLiteral(">");
		using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(DebugFormatter.Format(ref handler), typeof(IntPtr), Type.EmptyTypes);
		ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
		iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ldftn, dynamicMethodDefinition.Definition.Module.ImportReference(m));
		iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ret);
		lock (_GetLdftnPointerCache)
		{
			Func<IntPtr> func2 = (_GetLdftnPointerCache[m] = dynamicMethodDefinition.Generate().CreateDelegate<Func<IntPtr>>());
			return func2();
		}
	}

	public static string ToHexadecimalString(this byte[] data)
	{
		return BitConverter.ToString(data).Replace("-", string.Empty, StringComparison.Ordinal);
	}

	public static T? InvokePassing<T>(this MulticastDelegate md, T val, params object?[] args)
	{
		if ((object)md == null)
		{
			return val;
		}
		Helpers.ThrowIfArgumentNull(args, "args");
		object[] array = new object[args.Length + 1];
		array[0] = val;
		Array.Copy(args, 0, array, 1, args.Length);
		Delegate[] invocationList = md.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			array[0] = invocationList[i].DynamicInvoke(array);
		}
		return (T)array[0];
	}

	public static bool InvokeWhileTrue(this MulticastDelegate md, params object[] args)
	{
		if ((object)md == null)
		{
			return true;
		}
		Helpers.ThrowIfArgumentNull(args, "args");
		Delegate[] invocationList = md.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			if (!(bool)invocationList[i].DynamicInvoke(args))
			{
				return false;
			}
		}
		return true;
	}

	public static bool InvokeWhileFalse(this MulticastDelegate md, params object[] args)
	{
		if ((object)md == null)
		{
			return false;
		}
		Helpers.ThrowIfArgumentNull(args, "args");
		Delegate[] invocationList = md.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			if ((bool)invocationList[i].DynamicInvoke(args))
			{
				return true;
			}
		}
		return false;
	}

	public static T? InvokeWhileNull<T>(this MulticastDelegate? md, params object[] args) where T : class
	{
		if ((object)md == null)
		{
			return null;
		}
		Helpers.ThrowIfArgumentNull(args, "args");
		Delegate[] invocationList = md.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			T val = (T)invocationList[i].DynamicInvoke(args);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	public static string SpacedPascalCase(this string input)
	{
		Helpers.ThrowIfArgumentNull(input, "input");
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (i > 0 && char.IsUpper(c))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	public static string ReadNullTerminatedString(this BinaryReader stream)
	{
		Helpers.ThrowIfArgumentNull(stream, "stream");
		string text = "";
		char c;
		while ((c = stream.ReadChar()) != 0)
		{
			text += c;
		}
		return text;
	}

	public static void WriteNullTerminatedString(this BinaryWriter stream, string text)
	{
		Helpers.ThrowIfArgumentNull(stream, "stream");
		Helpers.ThrowIfArgumentNull(text, "text");
		if (text != null)
		{
			foreach (char ch in text)
			{
				stream.Write(ch);
			}
		}
		stream.Write('\0');
	}

	private static MethodBase GetRealMethod(MethodBase method)
	{
		if ((object)RTDynamicMethod_m_owner != null && method.GetType() == RTDynamicMethod)
		{
			return (MethodBase)RTDynamicMethod_m_owner.GetValue(method);
		}
		return method;
	}

	public static T CastDelegate<T>(this Delegate source) where T : Delegate
	{
		return (T)Helpers.ThrowIfNull(source, "source").CastDelegate(typeof(T));
	}

	[return: NotNullIfNotNull("source")]
	public static Delegate? CastDelegate(this Delegate? source, Type type)
	{
		if ((object)source == null)
		{
			return null;
		}
		Helpers.ThrowIfArgumentNull(type, "type");
		if (type.IsAssignableFrom(source.GetType()))
		{
			return source;
		}
		Delegate[] invocationList = source.GetInvocationList();
		if (invocationList.Length == 1)
		{
			return GetRealMethod(invocationList[0].Method).CreateDelegate(type, invocationList[0].Target);
		}
		Delegate[] array = new Delegate[invocationList.Length];
		for (int i = 0; i < invocationList.Length; i++)
		{
			array[i] = GetRealMethod(invocationList[i].Method).CreateDelegate(type, invocationList[i].Target);
		}
		return Delegate.Combine(array);
	}

	public static bool TryCastDelegate<T>(this Delegate source, [MaybeNullWhen(false)] out T result) where T : Delegate
	{
		if ((object)source == null)
		{
			result = null;
			return false;
		}
		if (source is T val)
		{
			result = val;
			return true;
		}
		Delegate result2;
		bool result3 = source.TryCastDelegate(typeof(T), out result2);
		result = (T)result2;
		return result3;
	}

	public static bool TryCastDelegate(this Delegate source, Type type, [MaybeNullWhen(false)] out Delegate? result)
	{
		result = null;
		if ((object)source == null)
		{
			return false;
		}
		try
		{
			result = source.CastDelegate(type);
			return true;
		}
		catch (Exception value)
		{
			bool isEnabled;
			MMDbgLog.DebugLogWarningStringHandler message = new MMDbgLog.DebugLogWarningStringHandler(43, 3, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Exception thrown in TryCastDelegate(");
				message.AppendFormatted(source.GetType());
				message.AppendLiteral(" -> ");
				message.AppendFormatted(type);
				message.AppendLiteral("): ");
				message.AppendFormatted(value);
			}
			MMDbgLog.Warning(ref message);
			return false;
		}
	}

	public static MethodInfo? GetStateMachineTarget(this MethodInfo method)
	{
		if ((object)p_StateMachineType == null || (object)t_StateMachineAttribute == null)
		{
			return null;
		}
		Helpers.ThrowIfArgumentNull(method, "method");
		object[] customAttributes = method.GetCustomAttributes(inherit: false);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			Attribute attribute = (Attribute)customAttributes[i];
			if (t_StateMachineAttribute.IsCompatible(attribute.GetType()))
			{
				return (p_StateMachineType.GetValue(attribute, null) as Type)?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
		}
		return null;
	}

	public static MethodBase GetActualGenericMethodDefinition(this MethodInfo method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		return (method.IsGenericMethod ? method.GetGenericMethodDefinition() : method).GetUnfilledMethodOnGenericType();
	}

	public static MethodBase GetUnfilledMethodOnGenericType(this MethodBase method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		if (method.DeclaringType != null && method.DeclaringType.IsGenericType)
		{
			Type genericTypeDefinition = method.DeclaringType.GetGenericTypeDefinition();
			method = MethodBase.GetMethodFromHandle(method.MethodHandle, genericTypeDefinition.TypeHandle);
		}
		return method;
	}

	public static bool Is(this MemberReference member, string fullName)
	{
		Helpers.ThrowIfArgumentNull(fullName, "fullName");
		if (member == null)
		{
			return false;
		}
		return member.FullName.Replace("+", "/", StringComparison.Ordinal) == fullName.Replace("+", "/", StringComparison.Ordinal);
	}

	public static bool Is(this MemberReference member, string typeFullName, string name)
	{
		Helpers.ThrowIfArgumentNull(typeFullName, "typeFullName");
		Helpers.ThrowIfArgumentNull(name, "name");
		if (member == null)
		{
			return false;
		}
		if (member.DeclaringType.FullName.Replace("+", "/", StringComparison.Ordinal) == typeFullName.Replace("+", "/", StringComparison.Ordinal))
		{
			return member.Name == name;
		}
		return false;
	}

	public static bool Is(this MemberReference member, Type type, string name)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		Helpers.ThrowIfArgumentNull(name, "name");
		if (member == null)
		{
			return false;
		}
		if (member.DeclaringType.FullName.Replace("+", "/", StringComparison.Ordinal) == type.FullName?.Replace("+", "/", StringComparison.Ordinal))
		{
			return member.Name == name;
		}
		return false;
	}

	public static bool Is(this MethodReference method, string fullName)
	{
		Helpers.ThrowIfArgumentNull(fullName, "fullName");
		if (method == null)
		{
			return false;
		}
		if (fullName.Contains(' ', StringComparison.Ordinal))
		{
			if (method.GetID(null, null, withType: true, simple: true).Replace("+", "/", StringComparison.Ordinal) == fullName.Replace("+", "/", StringComparison.Ordinal))
			{
				return true;
			}
			if (method.GetID().Replace("+", "/", StringComparison.Ordinal) == fullName.Replace("+", "/", StringComparison.Ordinal))
			{
				return true;
			}
		}
		return method.FullName.Replace("+", "/", StringComparison.Ordinal) == fullName.Replace("+", "/", StringComparison.Ordinal);
	}

	public static bool Is(this MethodReference method, string typeFullName, string name)
	{
		Helpers.ThrowIfArgumentNull(typeFullName, "typeFullName");
		Helpers.ThrowIfArgumentNull(name, "name");
		if (method == null)
		{
			return false;
		}
		if (name.Contains(' ', StringComparison.Ordinal) && method.DeclaringType.FullName.Replace("+", "/", StringComparison.Ordinal) == typeFullName.Replace("+", "/", StringComparison.Ordinal) && method.GetID(null, null, withType: false).Replace("+", "/", StringComparison.Ordinal) == name.Replace("+", "/", StringComparison.Ordinal))
		{
			return true;
		}
		if (method.DeclaringType.FullName.Replace("+", "/", StringComparison.Ordinal) == typeFullName.Replace("+", "/", StringComparison.Ordinal))
		{
			return method.Name == name;
		}
		return false;
	}

	public static bool Is(this MethodReference method, Type type, string name)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		Helpers.ThrowIfArgumentNull(name, "name");
		if (method == null)
		{
			return false;
		}
		if (name.Contains(' ', StringComparison.Ordinal) && method.DeclaringType.FullName.Replace("+", "/", StringComparison.Ordinal) == type.FullName?.Replace("+", "/", StringComparison.Ordinal) && method.GetID(null, null, withType: false).Replace("+", "/", StringComparison.Ordinal) == name.Replace("+", "/", StringComparison.Ordinal))
		{
			return true;
		}
		if (method.DeclaringType.FullName.Replace("+", "/", StringComparison.Ordinal) == type.FullName?.Replace("+", "/", StringComparison.Ordinal))
		{
			return method.Name == name;
		}
		return false;
	}

	public static void ReplaceOperands(this ILProcessor il, object? from, object? to)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		foreach (Instruction instruction in il.Body.Instructions)
		{
			if (instruction.Operand?.Equals(from) ?? (from == null))
			{
				instruction.Operand = to;
			}
		}
	}

	public static FieldReference Import(this ILProcessor il, FieldInfo field)
	{
		return Helpers.ThrowIfNull(il, "il").Body.Method.Module.ImportReference(field);
	}

	public static MethodReference Import(this ILProcessor il, MethodBase method)
	{
		return Helpers.ThrowIfNull(il, "il").Body.Method.Module.ImportReference(method);
	}

	public static TypeReference Import(this ILProcessor il, Type type)
	{
		return Helpers.ThrowIfNull(il, "il").Body.Method.Module.ImportReference(type);
	}

	public static MemberReference Import(this ILProcessor il, MemberInfo member)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		Helpers.ThrowIfArgumentNull(member, "member");
		if (!(member is FieldInfo field))
		{
			if (!(member is MethodBase method))
			{
				if (member is Type type)
				{
					return il.Import(type);
				}
				throw new NotSupportedException("Unsupported member type " + member.GetType().FullName);
			}
			return il.Import(method);
		}
		return il.Import(field);
	}

	public static Instruction Create(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, FieldInfo field)
	{
		return Helpers.ThrowIfNull(il, "il").Create(opcode, il.Import(field));
	}

	public static Instruction Create(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, MethodBase method)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		if (method is DynamicMethod)
		{
			return il.Create(opcode, (object)method);
		}
		return il.Create(opcode, il.Import(method));
	}

	public static Instruction Create(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, Type type)
	{
		return Helpers.ThrowIfNull(il, "il").Create(opcode, il.Import(type));
	}

	public static Instruction Create(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, object operand)
	{
		Instruction instruction = Helpers.ThrowIfNull(il, "il").Create(Mono.Cecil.Cil.OpCodes.Nop);
		instruction.OpCode = opcode;
		instruction.Operand = operand;
		return instruction;
	}

	public static Instruction Create(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, MemberInfo member)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		Helpers.ThrowIfArgumentNull(member, "member");
		if (!(member is FieldInfo field))
		{
			if (!(member is MethodBase method))
			{
				if (member is Type type)
				{
					return il.Create(opcode, type);
				}
				throw new NotSupportedException("Unsupported member type " + member.GetType().FullName);
			}
			return il.Create(opcode, method);
		}
		return il.Create(opcode, field);
	}

	public static void Emit(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, FieldInfo field)
	{
		Helpers.ThrowIfNull(il, "il").Emit(opcode, il.Import(field));
	}

	public static void Emit(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, MethodBase method)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		Helpers.ThrowIfArgumentNull(method, "method");
		if (method is DynamicMethod)
		{
			il.Emit(opcode, (object)method);
		}
		else
		{
			il.Emit(opcode, il.Import(method));
		}
	}

	public static void Emit(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, Type type)
	{
		Helpers.ThrowIfNull(il, "il").Emit(opcode, il.Import(type));
	}

	public static void Emit(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, MemberInfo member)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		Helpers.ThrowIfArgumentNull(member, "member");
		if (!(member is FieldInfo field))
		{
			if (!(member is MethodBase method))
			{
				if (!(member is Type type))
				{
					throw new NotSupportedException("Unsupported member type " + member.GetType().FullName);
				}
				il.Emit(opcode, type);
			}
			else
			{
				il.Emit(opcode, method);
			}
		}
		else
		{
			il.Emit(opcode, field);
		}
	}

	public static void Emit(this ILProcessor il, Mono.Cecil.Cil.OpCode opcode, object operand)
	{
		Helpers.ThrowIfNull(il, "il").Append(il.Create(opcode, operand));
	}
}
