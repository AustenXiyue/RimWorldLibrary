namespace System.IO;

/// <summary>Specifies whether the underlying handle is inheritable by child processes.</summary>
[Serializable]
public enum HandleInheritability
{
	/// <summary>Specifies that the handle is not inheritable by child processes.</summary>
	None,
	/// <summary>Specifies that the handle is inheritable by child processes.</summary>
	Inheritable
}
