using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace System.Xml.Serialization;

internal class SourceInfo
{
	private static Regex regex = new Regex("([(][(](?<t>[^)]+)[)])?(?<a>[^[]+)[[](?<ia>.+)[]][)]?");

	private static Regex regex2 = new Regex("[(][(](?<cast>[^)]+)[)](?<arg>[^)]+)[)]");

	private static readonly Lazy<MethodInfo> iListGetItemMethod = new Lazy<MethodInfo>(() => typeof(IList).GetMethod("get_Item", CodeGenerator.InstanceBindingFlags, null, new Type[1] { typeof(int) }, null));

	public string Source;

	public readonly string Arg;

	public readonly MemberInfo MemberInfo;

	public readonly Type Type;

	public readonly CodeGenerator ILG;

	public SourceInfo(string source, string arg, MemberInfo memberInfo, Type type, CodeGenerator ilg)
	{
		Source = source;
		Arg = arg ?? source;
		MemberInfo = memberInfo;
		Type = type;
		ILG = ilg;
	}

	public SourceInfo CastTo(TypeDesc td)
	{
		return new SourceInfo("((" + td.CSharpName + ")" + Source + ")", Arg, MemberInfo, td.Type, ILG);
	}

	public void LoadAddress(Type elementType)
	{
		InternalLoad(elementType, asAddress: true);
	}

	public void Load(Type elementType)
	{
		InternalLoad(elementType);
	}

	private void InternalLoad(Type elementType, bool asAddress = false)
	{
		Match match = regex.Match(Arg);
		if (match.Success)
		{
			object variable = ILG.GetVariable(match.Groups["a"].Value);
			Type variableType = ILG.GetVariableType(variable);
			object variable2 = ILG.GetVariable(match.Groups["ia"].Value);
			if (variableType.IsArray)
			{
				ILG.Load(variable);
				ILG.Load(variable2);
				Type elementType2 = variableType.GetElementType();
				if (CodeGenerator.IsNullableGenericType(elementType2))
				{
					ILG.Ldelema(elementType2);
					ConvertNullableValue(elementType2, elementType);
					return;
				}
				if (elementType2.IsValueType)
				{
					ILG.Ldelema(elementType2);
					if (!asAddress)
					{
						ILG.Ldobj(elementType2);
					}
				}
				else
				{
					ILG.Ldelem(elementType2);
				}
				if (elementType != null)
				{
					ILG.ConvertValue(elementType2, elementType);
				}
				return;
			}
			ILG.Load(variable);
			ILG.Load(variable2);
			MethodInfo methodInfo = variableType.GetMethod("get_Item", CodeGenerator.InstanceBindingFlags, null, new Type[1] { typeof(int) }, null);
			if (methodInfo == null && typeof(IList).IsAssignableFrom(variableType))
			{
				methodInfo = iListGetItemMethod.Value;
			}
			ILG.Call(methodInfo);
			Type returnType = methodInfo.ReturnType;
			if (CodeGenerator.IsNullableGenericType(returnType))
			{
				LocalBuilder tempLocal = ILG.GetTempLocal(returnType);
				ILG.Stloc(tempLocal);
				ILG.Ldloca(tempLocal);
				ConvertNullableValue(returnType, elementType);
			}
			else
			{
				if (elementType != null && !returnType.IsAssignableFrom(elementType) && !elementType.IsAssignableFrom(returnType))
				{
					throw new CodeGeneratorConversionException(returnType, elementType, asAddress, "IsNotAssignableFrom");
				}
				Convert(returnType, elementType, asAddress);
			}
			return;
		}
		if (Source == "null")
		{
			ILG.Load(null);
			return;
		}
		Type type;
		if (Arg.StartsWith("o.@", StringComparison.Ordinal) || MemberInfo != null)
		{
			object variable3 = ILG.GetVariable(Arg.StartsWith("o.@", StringComparison.Ordinal) ? "o" : Arg);
			type = ILG.GetVariableType(variable3);
			if (type.IsValueType)
			{
				ILG.LoadAddress(variable3);
			}
			else
			{
				ILG.Load(variable3);
			}
		}
		else
		{
			object variable3 = ILG.GetVariable(Arg);
			type = ILG.GetVariableType(variable3);
			if (CodeGenerator.IsNullableGenericType(type) && type.GetGenericArguments()[0] == elementType)
			{
				ILG.LoadAddress(variable3);
				ConvertNullableValue(type, elementType);
			}
			else if (asAddress)
			{
				ILG.LoadAddress(variable3);
			}
			else
			{
				ILG.Load(variable3);
			}
		}
		if (MemberInfo != null)
		{
			Type type2 = ((MemberInfo is FieldInfo) ? ((FieldInfo)MemberInfo).FieldType : ((PropertyInfo)MemberInfo).PropertyType);
			if (CodeGenerator.IsNullableGenericType(type2))
			{
				ILG.LoadMemberAddress(MemberInfo);
				ConvertNullableValue(type2, elementType);
			}
			else
			{
				ILG.LoadMember(MemberInfo);
				Convert(type2, elementType, asAddress);
			}
			return;
		}
		match = regex2.Match(Source);
		if (match.Success)
		{
			if (asAddress)
			{
				ILG.ConvertAddress(type, Type);
			}
			else
			{
				ILG.ConvertValue(type, Type);
			}
			type = Type;
		}
		Convert(type, elementType, asAddress);
	}

	private void Convert(Type sourceType, Type targetType, bool asAddress)
	{
		if (targetType != null)
		{
			if (asAddress)
			{
				ILG.ConvertAddress(sourceType, targetType);
			}
			else
			{
				ILG.ConvertValue(sourceType, targetType);
			}
		}
	}

	private void ConvertNullableValue(Type nullableType, Type targetType)
	{
		if (targetType != nullableType)
		{
			MethodInfo method = nullableType.GetMethod("get_Value", CodeGenerator.InstanceBindingFlags, null, CodeGenerator.EmptyTypeArray, null);
			ILG.Call(method);
			if (targetType != null)
			{
				ILG.ConvertValue(method.ReturnType, targetType);
			}
		}
	}

	public static implicit operator string(SourceInfo source)
	{
		return source.Source;
	}

	public static bool operator !=(SourceInfo a, SourceInfo b)
	{
		if ((object)a != null)
		{
			return !a.Equals(b);
		}
		return (object)b != null;
	}

	public static bool operator ==(SourceInfo a, SourceInfo b)
	{
		return a?.Equals(b) ?? ((object)b == null);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return Source == null;
		}
		SourceInfo sourceInfo = obj as SourceInfo;
		if (sourceInfo != null)
		{
			return Source == sourceInfo.Source;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Source != null)
		{
			return Source.GetHashCode();
		}
		return 0;
	}
}
