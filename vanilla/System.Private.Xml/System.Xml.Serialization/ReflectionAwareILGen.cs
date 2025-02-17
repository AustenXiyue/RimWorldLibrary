using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace System.Xml.Serialization;

internal sealed class ReflectionAwareILGen
{
	internal ReflectionAwareILGen()
	{
	}

	[RequiresUnreferencedCode("calls GetTypeDesc")]
	internal static void WriteReflectionInit(TypeScope scope)
	{
		foreach (Type type in scope.Types)
		{
			scope.GetTypeDesc(type);
		}
	}

	internal static void ILGenForEnumLongValue(CodeGenerator ilg, string variable)
	{
		ArgBuilder arg = ilg.GetArg(variable);
		ilg.Ldarg(arg);
		ilg.ConvertValue(arg.ArgType, typeof(long));
	}

	internal static string GetStringForTypeof(string typeFullName)
	{
		return "typeof(" + typeFullName + ")";
	}

	internal static string GetStringForMember(string obj, string memberName)
	{
		return obj + ".@" + memberName;
	}

	internal static SourceInfo GetSourceForMember(string obj, MemberMapping member, CodeGenerator ilg)
	{
		return GetSourceForMember(obj, member, member.MemberInfo, ilg);
	}

	internal static SourceInfo GetSourceForMember(string obj, MemberMapping member, MemberInfo memberInfo, CodeGenerator ilg)
	{
		return new SourceInfo(GetStringForMember(obj, member.Name), obj, memberInfo, member.TypeDesc.Type, ilg);
	}

	internal static void ILGenForEnumMember(CodeGenerator ilg, Type type, string memberName)
	{
		ilg.Ldc(Enum.Parse(type, memberName, ignoreCase: false));
	}

	internal static string GetStringForArrayMember(string arrayName, string subscript)
	{
		return arrayName + "[" + subscript + "]";
	}

	internal static string GetStringForMethod(string obj, string memberName)
	{
		return obj + "." + memberName + "(";
	}

	[RequiresUnreferencedCode("calls ILGenForCreateInstance")]
	internal static void ILGenForCreateInstance(CodeGenerator ilg, Type type, bool ctorInaccessible, bool cast)
	{
		if (!ctorInaccessible)
		{
			ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes);
			if (constructor != null)
			{
				ilg.New(constructor);
				return;
			}
			LocalBuilder tempLocal = ilg.GetTempLocal(type);
			ilg.Ldloca(tempLocal);
			ilg.InitObj(type);
			ilg.Ldloc(tempLocal);
		}
		else
		{
			ILGenForCreateInstance(ilg, type, cast ? type : null);
		}
	}

	[RequiresUnreferencedCode("calls GetType")]
	internal static void ILGenForCreateInstance(CodeGenerator ilg, Type type, Type cast)
	{
		if (type == typeof(DBNull))
		{
			FieldInfo field = type.GetField("Value", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			ilg.LoadMember(field);
			return;
		}
		if (type.FullName == "System.Xml.Linq.XElement")
		{
			Type type2 = type.Assembly.GetType("System.Xml.Linq.XName");
			if (type2 != null)
			{
				MethodInfo method = type2.GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[1] { typeof(string) });
				ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, new Type[1] { type2 });
				if (method != null && constructor != null)
				{
					ilg.Ldstr("default");
					ilg.Call(method);
					ilg.New(constructor);
					return;
				}
			}
		}
		Label label = ilg.DefineLabel();
		Label label2 = ilg.DefineLabel();
		ilg.Ldc(type);
		MethodInfo method2 = typeof(IntrospectionExtensions).GetMethod("GetTypeInfo", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[1] { typeof(Type) });
		ilg.Call(method2);
		LocalBuilder localBuilder = ilg.DeclareLocal(typeof(IEnumerator<>).MakeGenericType(typeof(ConstructorInfo)), "e");
		MethodInfo method3 = typeof(TypeInfo).GetMethod("get_DeclaredConstructors");
		MethodInfo method4 = typeof(IEnumerable<>).MakeGenericType(typeof(ConstructorInfo)).GetMethod("GetEnumerator");
		ilg.Call(method3);
		ilg.Call(method4);
		ilg.Stloc(localBuilder);
		ilg.WhileBegin();
		MethodInfo method5 = typeof(IEnumerator).GetMethod("get_Current");
		ilg.Ldloc(localBuilder);
		ilg.Call(method5);
		LocalBuilder localBuilder2 = ilg.DeclareLocal(typeof(ConstructorInfo), "constructorInfo");
		ilg.Stloc(localBuilder2);
		ilg.Ldloc(localBuilder2);
		MethodInfo method6 = typeof(ConstructorInfo).GetMethod("get_IsStatic");
		ilg.Call(method6);
		ilg.Brtrue(label2);
		ilg.Ldloc(localBuilder2);
		MethodInfo method7 = typeof(ConstructorInfo).GetMethod("GetParameters");
		ilg.Call(method7);
		ilg.Ldlen();
		ilg.Ldc(0);
		ilg.Cne();
		ilg.Brtrue(label2);
		MethodInfo method8 = typeof(ConstructorInfo).GetMethod("Invoke", new Type[1] { typeof(object[]) });
		ilg.Ldloc(localBuilder2);
		ilg.Load(null);
		ilg.Call(method8);
		ilg.Br(label);
		ilg.MarkLabel(label2);
		ilg.WhileBeginCondition();
		MethodInfo method9 = typeof(IEnumerator).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes);
		ilg.Ldloc(localBuilder);
		ilg.Call(method9);
		ilg.WhileEndCondition();
		ilg.WhileEnd();
		MethodInfo method10 = typeof(Activator).GetMethod("CreateInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[1] { typeof(Type) });
		ilg.Ldc(type);
		ilg.Call(method10);
		ilg.MarkLabel(label);
		if (cast != null)
		{
			ilg.ConvertValue(method10.ReturnType, cast);
		}
	}

	[RequiresUnreferencedCode("calls LoadMember")]
	internal static void WriteLocalDecl(string variableName, SourceInfo initValue)
	{
		Type type = initValue.Type;
		LocalBuilder localBuilder = initValue.ILG.DeclareOrGetLocal(type, variableName);
		if (initValue.Source != null)
		{
			if (initValue == "null")
			{
				initValue.ILG.Load(null);
			}
			else if (initValue.Arg.StartsWith("o.@", StringComparison.Ordinal))
			{
				initValue.ILG.LoadMember(initValue.ILG.GetLocal("o"), initValue.MemberInfo);
			}
			else if (initValue.Source.EndsWith(']'))
			{
				initValue.Load(initValue.Type);
			}
			else if (initValue.Source == "fixup.Source" || initValue.Source == "e.Current")
			{
				string[] array = initValue.Source.Split('.');
				object variable = initValue.ILG.GetVariable(array[0]);
				PropertyInfo property = CodeGenerator.GetVariableType(variable).GetProperty(array[1]);
				initValue.ILG.LoadMember(variable, property);
				initValue.ILG.ConvertValue(property.PropertyType, localBuilder.LocalType);
			}
			else
			{
				object variable2 = initValue.ILG.GetVariable(initValue.Arg);
				initValue.ILG.Load(variable2);
				initValue.ILG.ConvertValue(CodeGenerator.GetVariableType(variable2), localBuilder.LocalType);
			}
			initValue.ILG.Stloc(localBuilder);
		}
	}

	[RequiresUnreferencedCode("calls ILGenForCreateInstance")]
	internal static void WriteCreateInstance(string source, bool ctorInaccessible, Type type, CodeGenerator ilg)
	{
		LocalBuilder local = ilg.DeclareOrGetLocal(type, source);
		ILGenForCreateInstance(ilg, type, ctorInaccessible, ctorInaccessible);
		ilg.Stloc(local);
	}

	[RequiresUnreferencedCode("calls Load")]
	internal static void WriteInstanceOf(SourceInfo source, Type type, CodeGenerator ilg)
	{
		source.Load(typeof(object));
		ilg.IsInst(type);
		ilg.Load(null);
		ilg.Cne();
	}

	[RequiresUnreferencedCode("calls Load")]
	internal static void WriteArrayLocalDecl(string typeName, string variableName, SourceInfo initValue, TypeDesc arrayTypeDesc)
	{
		Type type = ((typeName == arrayTypeDesc.CSharpName) ? arrayTypeDesc.Type : arrayTypeDesc.Type.MakeArrayType());
		LocalBuilder localBuilder = initValue.ILG.DeclareOrGetLocal(type, variableName);
		initValue.Load(localBuilder.LocalType);
		initValue.ILG.Stloc(localBuilder);
	}

	internal static void WriteTypeCompare(string variable, Type type, CodeGenerator ilg)
	{
		ilg.Ldloc(typeof(Type), variable);
		ilg.Ldc(type);
		ilg.Ceq();
	}

	internal static void WriteArrayTypeCompare(string variable, Type arrayType, CodeGenerator ilg)
	{
		ilg.Ldloc(typeof(Type), variable);
		ilg.Ldc(arrayType);
		ilg.Ceq();
	}

	[return: NotNullIfNotNull("value")]
	internal static string GetQuotedCSharpString(string value)
	{
		if (value != null)
		{
			return "@\"" + GetCSharpString(value) + "\"";
		}
		return null;
	}

	[return: NotNullIfNotNull("value")]
	internal static string GetCSharpString(string value)
	{
		if (value == null)
		{
			return null;
		}
		int i;
		for (i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if ((c < ' ' || c == '"') ? true : false)
			{
				break;
			}
		}
		if (i == value.Length)
		{
			return value;
		}
		Span<char> initialBuffer = stackalloc char[128];
		System.Text.ValueStringBuilder valueStringBuilder = new System.Text.ValueStringBuilder(initialBuffer);
		valueStringBuilder.Append(value.AsSpan(0, i));
		for (; i < value.Length; i++)
		{
			char c2 = value[i];
			if (c2 < ' ')
			{
				switch (c2)
				{
				case '\r':
					valueStringBuilder.Append("\\r");
					continue;
				case '\n':
					valueStringBuilder.Append("\\n");
					continue;
				case '\t':
					valueStringBuilder.Append("\\t");
					continue;
				}
				byte b = (byte)c2;
				valueStringBuilder.Append("\\x");
				valueStringBuilder.Append(System.HexConverter.ToCharUpper(b >> 4));
				valueStringBuilder.Append(System.HexConverter.ToCharUpper(b));
			}
			else if (c2 == '"')
			{
				valueStringBuilder.Append("\"\"");
			}
			else
			{
				valueStringBuilder.Append(c2);
			}
		}
		return valueStringBuilder.ToString();
	}
}
