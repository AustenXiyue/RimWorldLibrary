using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace HarmonyLib;

internal static class PatchInfoSerialization
{
	private class Binder : SerializationBinder
	{
		public override Type BindToType(string assemblyName, string typeName)
		{
			Type[] array = new Type[3]
			{
				typeof(PatchInfo),
				typeof(Patch[]),
				typeof(Patch)
			};
			Type[] array2 = array;
			foreach (Type type in array2)
			{
				if (typeName == type.FullName)
				{
					return type;
				}
			}
			return Type.GetType($"{typeName}, {assemblyName}");
		}
	}

	internal static readonly BinaryFormatter binaryFormatter = new BinaryFormatter
	{
		Binder = new Binder()
	};

	internal static byte[] Serialize(this PatchInfo patchInfo)
	{
		using MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, patchInfo);
		return memoryStream.GetBuffer();
	}

	internal static PatchInfo Deserialize(byte[] bytes)
	{
		using MemoryStream serializationStream = new MemoryStream(bytes);
		return (PatchInfo)binaryFormatter.Deserialize(serializationStream);
	}

	internal static int PriorityComparer(object obj, int index, int priority)
	{
		Traverse traverse = Traverse.Create(obj);
		int value = traverse.Field("priority").GetValue<int>();
		int value2 = traverse.Field("index").GetValue<int>();
		if (priority != value)
		{
			return -priority.CompareTo(value);
		}
		return index.CompareTo(value2);
	}
}
