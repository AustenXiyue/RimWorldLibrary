namespace System.Windows.Markup;

internal struct TypeInfoKey
{
	internal string DeclaringAssembly;

	internal string TypeFullName;

	public override bool Equals(object o)
	{
		if (o is TypeInfoKey typeInfoKey)
		{
			if ((typeInfoKey.DeclaringAssembly != null) ? typeInfoKey.DeclaringAssembly.Equals(DeclaringAssembly) : (DeclaringAssembly == null))
			{
				if (typeInfoKey.TypeFullName == null)
				{
					return TypeFullName == null;
				}
				return typeInfoKey.TypeFullName.Equals(TypeFullName);
			}
			return false;
		}
		return false;
	}

	public static bool operator ==(TypeInfoKey key1, TypeInfoKey key2)
	{
		return key1.Equals(key2);
	}

	public static bool operator !=(TypeInfoKey key1, TypeInfoKey key2)
	{
		return !key1.Equals(key2);
	}

	public override int GetHashCode()
	{
		return ((DeclaringAssembly != null) ? DeclaringAssembly.GetHashCode() : 0) ^ ((TypeFullName != null) ? TypeFullName.GetHashCode() : 0);
	}

	public override string ToString()
	{
		return "TypeInfoKey: Assembly=" + ((DeclaringAssembly != null) ? DeclaringAssembly : "null") + " Type=" + ((TypeFullName != null) ? TypeFullName : "null");
	}
}
