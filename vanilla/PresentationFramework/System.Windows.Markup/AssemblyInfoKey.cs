namespace System.Windows.Markup;

internal struct AssemblyInfoKey
{
	internal string AssemblyFullName;

	public override bool Equals(object o)
	{
		if (o is AssemblyInfoKey assemblyInfoKey)
		{
			if (assemblyInfoKey.AssemblyFullName == null)
			{
				return AssemblyFullName == null;
			}
			return assemblyInfoKey.AssemblyFullName.Equals(AssemblyFullName);
		}
		return false;
	}

	public static bool operator ==(AssemblyInfoKey key1, AssemblyInfoKey key2)
	{
		return key1.Equals(key2);
	}

	public static bool operator !=(AssemblyInfoKey key1, AssemblyInfoKey key2)
	{
		return !key1.Equals(key2);
	}

	public override int GetHashCode()
	{
		if (AssemblyFullName == null)
		{
			return 0;
		}
		return AssemblyFullName.GetHashCode();
	}

	public override string ToString()
	{
		return AssemblyFullName;
	}
}
