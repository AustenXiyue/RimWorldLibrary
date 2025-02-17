using System;

namespace MonoMod.ModInterop;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
internal sealed class ModImportNameAttribute : Attribute
{
	public string Name { get; }

	public ModImportNameAttribute(string name)
	{
		Name = name;
	}
}
