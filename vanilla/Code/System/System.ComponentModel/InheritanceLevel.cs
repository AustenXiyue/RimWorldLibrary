namespace System.ComponentModel;

/// <summary>Defines identifiers for types of inheritance levels.</summary>
public enum InheritanceLevel
{
	/// <summary>The object is inherited.</summary>
	Inherited = 1,
	/// <summary>The object is inherited, but has read-only access.</summary>
	InheritedReadOnly,
	/// <summary>The object is not inherited.</summary>
	NotInherited
}
