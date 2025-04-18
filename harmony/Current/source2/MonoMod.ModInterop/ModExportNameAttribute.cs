using System;

namespace MonoMod.ModInterop;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class ModExportNameAttribute : Attribute
{
	public string Name { get; }

	public ModExportNameAttribute(string name)
	{
		Name = name;
	}
}
