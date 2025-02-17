using System;

namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
public class HarmonyArgument : Attribute
{
	public string OriginalName { get; private set; }

	public int Index { get; private set; }

	public string NewName { get; private set; }

	public HarmonyArgument(string originalName)
		: this(originalName, null)
	{
	}

	public HarmonyArgument(int index)
		: this(index, null)
	{
	}

	public HarmonyArgument(string originalName, string newName)
	{
		OriginalName = originalName;
		Index = -1;
		NewName = newName;
	}

	public HarmonyArgument(int index, string name)
	{
		OriginalName = null;
		Index = index;
		NewName = name;
	}
}
