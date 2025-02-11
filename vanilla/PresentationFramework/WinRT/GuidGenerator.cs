using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace WinRT;

internal static class GuidGenerator
{
	private static Guid wrt_pinterface_namespace = new Guid("d57af411-737b-c042-abae-878b1e16adee");

	public static Guid GetGUID(Type type)
	{
		return type.GetGuidType().GUID;
	}

	public static Guid GetIID(Type type)
	{
		type = type.GetGuidType();
		if (!type.IsGenericType)
		{
			return type.GUID;
		}
		return (Guid)type.GetField("PIID").GetValue(null);
	}

	public static string GetSignature(Type type)
	{
		Type type2 = type.FindHelperType();
		if (type2 != null)
		{
			MethodInfo method = type2.GetMethod("GetGuidSignature", BindingFlags.Static | BindingFlags.Public);
			if (method != null)
			{
				object[] parameters = new Type[0];
				return (string)method.Invoke(null, parameters);
			}
		}
		if (type == typeof(object))
		{
			return "cinterface(IInspectable)";
		}
		if (type.IsGenericType)
		{
			IEnumerable<string> values = from t in type.GetGenericArguments()
				select GetSignature(t);
			return "pinterface({" + GetGUID(type).ToString() + "};" + string.Join(";", values) + ")";
		}
		if (type.IsValueType)
		{
			switch (type.Name)
			{
			case "SByte":
				return "i1";
			case "Byte":
				return "u1";
			case "Int16":
				return "i2";
			case "UInt16":
				return "u2";
			case "Int32":
				return "i4";
			case "UInt32":
				return "u4";
			case "Int64":
				return "i8";
			case "UInt64":
				return "u8";
			case "Single":
				return "f4";
			case "Double":
				return "f8";
			case "Boolean":
				return "b1";
			case "Char":
				return "c2";
			case "Guid":
				return "g16";
			default:
				if (type.IsEnum)
				{
					bool flag = type.CustomAttributes.Any((CustomAttributeData cad) => cad.AttributeType == typeof(FlagsAttribute));
					return "enum(" + TypeExtensions.RemoveNamespacePrefix(type.FullName) + ";" + (flag ? "u4" : "i4") + ")";
				}
				if (!type.IsPrimitive)
				{
					IEnumerable<string> values2 = from fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
						select GetSignature(fi.FieldType);
					return "struct(" + TypeExtensions.RemoveNamespacePrefix(type.FullName) + ";" + string.Join(";", values2) + ")";
				}
				throw new InvalidOperationException("unsupported value type");
			}
		}
		if (type == typeof(string))
		{
			return "string";
		}
		if (Projections.TryGetDefaultInterfaceTypeForRuntimeClassType(type, out var defaultInterface))
		{
			return "rc(" + TypeExtensions.RemoveNamespacePrefix(type.FullName) + ";" + GetSignature(defaultInterface) + ")";
		}
		if (type.IsDelegate())
		{
			return "delegate({" + GetGUID(type).ToString() + "})";
		}
		return "{" + type.GUID.ToString() + "}";
	}

	private static Guid encode_guid(byte[] data)
	{
		if (BitConverter.IsLittleEndian)
		{
			byte b = data[0];
			data[0] = data[3];
			data[3] = b;
			b = data[1];
			data[1] = data[2];
			data[2] = b;
			b = data[4];
			data[4] = data[5];
			data[5] = b;
			b = data[6];
			data[6] = data[7];
			data[7] = (byte)((b & 0xF) | 0x50);
			data[8] = (byte)((data[8] & 0x3F) | 0x80);
		}
		return new Guid(data.Take(16).ToArray());
	}

	public static Guid CreateIID(Type type)
	{
		string signature = GetSignature(type);
		if (!type.IsGenericType)
		{
			return new Guid(signature);
		}
		return encode_guid(SHA1.HashData(wrt_pinterface_namespace.ToByteArray().Concat(Encoding.UTF8.GetBytes(signature)).ToArray()));
	}
}
