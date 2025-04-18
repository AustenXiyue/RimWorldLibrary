using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil;

namespace HarmonyLib;

internal static class InlineSignatureParser
{
	internal static InlineSignature ImportCallSite(Module moduleFrom, byte[] data)
	{
		InlineSignature inlineSignature = new InlineSignature();
		BinaryReader reader;
		using (MemoryStream input = new MemoryStream(data, writable: false))
		{
			reader = new BinaryReader(input);
			try
			{
				ReadMethodSignature(inlineSignature);
				return inlineSignature;
			}
			finally
			{
				if (reader != null)
				{
					((IDisposable)reader).Dispose();
				}
			}
		}
		Type GetTypeDefOrRef()
		{
			uint num4 = ReadCompressedUInt32();
			uint num5 = num4 >> 2;
			return moduleFrom.ResolveType((num4 & 3) switch
			{
				0u => (int)(0x2000000 | num5), 
				1u => (int)(0x1000000 | num5), 
				2u => (int)(0x1B000000 | num5), 
				_ => 0, 
			});
		}
		int ReadCompressedInt32()
		{
			byte b3 = reader.ReadByte();
			reader.BaseStream.Seek(-1L, SeekOrigin.Current);
			int num6 = (int)ReadCompressedUInt32();
			int num7 = num6 >> 1;
			if ((num6 & 1) == 0)
			{
				return num7;
			}
			switch (b3 & 0xC0)
			{
			case 0:
			case 64:
				return num7 - 64;
			case 128:
				return num7 - 8192;
			default:
				return num7 - 268435456;
			}
		}
		uint ReadCompressedUInt32()
		{
			byte b2 = reader.ReadByte();
			if ((b2 & 0x80) == 0)
			{
				return b2;
			}
			if ((b2 & 0x40) == 0)
			{
				return (uint)(((b2 & -129) << 8) | reader.ReadByte());
			}
			return (uint)(((b2 & -193) << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte());
		}
		void ReadMethodSignature(InlineSignature method)
		{
			byte b = reader.ReadByte();
			if ((b & 0x20) != 0)
			{
				method.HasThis = true;
				b = (byte)(b & -33);
			}
			if ((b & 0x40) != 0)
			{
				method.ExplicitThis = true;
				b = (byte)(b & -65);
			}
			method.CallingConvention = (CallingConvention)(b + 1);
			if ((b & 0x10) != 0)
			{
				ReadCompressedUInt32();
			}
			uint num = ReadCompressedUInt32();
			method.ReturnType = ReadTypeSignature();
			for (int i = 0; i < num; i++)
			{
				method.Parameters.Add(ReadTypeSignature());
			}
		}
		object ReadTypeSignature()
		{
			MetadataType metadataType = (MetadataType)reader.ReadByte();
			switch (metadataType)
			{
			case MetadataType.ValueType:
			case MetadataType.Class:
				return GetTypeDefOrRef();
			case MetadataType.Pointer:
				return ((Type)ReadTypeSignature()).MakePointerType();
			case MetadataType.FunctionPointer:
			{
				InlineSignature inlineSignature2 = new InlineSignature();
				ReadMethodSignature(inlineSignature2);
				return inlineSignature2;
			}
			case MetadataType.ByReference:
				return ((Type)ReadTypeSignature()).MakePointerType();
			case (MetadataType)29:
				return ((Type)ReadTypeSignature()).MakeArrayType();
			case MetadataType.Array:
			{
				Type type2 = (Type)ReadTypeSignature();
				uint rank = ReadCompressedUInt32();
				uint num2 = ReadCompressedUInt32();
				for (int j = 0; j < num2; j++)
				{
					ReadCompressedUInt32();
				}
				uint num3 = ReadCompressedUInt32();
				for (int k = 0; k < num3; k++)
				{
					ReadCompressedInt32();
				}
				return type2.MakeArrayType((int)rank);
			}
			case MetadataType.OptionalModifier:
				return new InlineSignature.ModifierType
				{
					IsOptional = true,
					Modifier = GetTypeDefOrRef(),
					Type = ReadTypeSignature()
				};
			case MetadataType.RequiredModifier:
				return new InlineSignature.ModifierType
				{
					IsOptional = false,
					Modifier = GetTypeDefOrRef(),
					Type = ReadTypeSignature()
				};
			case MetadataType.Var:
			case MetadataType.MVar:
				throw new NotSupportedException($"Unsupported generic callsite element: {metadataType}");
			case MetadataType.GenericInstance:
			{
				reader.ReadByte();
				Type type = GetTypeDefOrRef();
				int count = (int)ReadCompressedUInt32();
				return type.MakeGenericType((from _ in Enumerable.Range(0, count)
					select (Type)ReadTypeSignature()).ToArray());
			}
			case MetadataType.Object:
				return typeof(object);
			case MetadataType.Void:
				return typeof(void);
			case MetadataType.TypedByReference:
				return typeof(TypedReference);
			case MetadataType.IntPtr:
				return typeof(IntPtr);
			case MetadataType.UIntPtr:
				return typeof(UIntPtr);
			case MetadataType.Boolean:
				return typeof(bool);
			case MetadataType.Char:
				return typeof(char);
			case MetadataType.SByte:
				return typeof(sbyte);
			case MetadataType.Byte:
				return typeof(byte);
			case MetadataType.Int16:
				return typeof(short);
			case MetadataType.UInt16:
				return typeof(ushort);
			case MetadataType.Int32:
				return typeof(int);
			case MetadataType.UInt32:
				return typeof(uint);
			case MetadataType.Int64:
				return typeof(long);
			case MetadataType.UInt64:
				return typeof(ulong);
			case MetadataType.Single:
				return typeof(float);
			case MetadataType.Double:
				return typeof(double);
			case MetadataType.String:
				return typeof(string);
			default:
				throw new NotSupportedException($"Unsupported callsite element: {metadataType}");
			}
		}
	}
}
