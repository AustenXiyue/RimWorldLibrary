namespace System.CodeDom;

/// <summary>Defines member attribute identifiers for class members.</summary>
public enum MemberAttributes
{
	/// <summary>An abstract member.</summary>
	Abstract = 1,
	/// <summary>A member that cannot be overridden in a derived class.</summary>
	Final = 2,
	/// <summary>A static member. In Visual Basic, this is equivalent to the Shared keyword.</summary>
	Static = 3,
	/// <summary>A member that overrides a base class member.</summary>
	Override = 4,
	/// <summary>A constant member.</summary>
	Const = 5,
	/// <summary>A new member.</summary>
	New = 16,
	/// <summary>An overloaded member. Some languages, such as Visual Basic, require overloaded members to be explicitly indicated.</summary>
	Overloaded = 256,
	/// <summary>A member that is accessible to any class within the same assembly.</summary>
	Assembly = 4096,
	/// <summary>A member that is accessible within its class, and derived classes in the same assembly.</summary>
	FamilyAndAssembly = 8192,
	/// <summary>A member that is accessible within the family of its class and derived classes.</summary>
	Family = 12288,
	/// <summary>A member that is accessible within its class, its derived classes in any assembly, and any class in the same assembly.</summary>
	FamilyOrAssembly = 16384,
	/// <summary>A private member.</summary>
	Private = 20480,
	/// <summary>A public member.</summary>
	Public = 24576,
	/// <summary>An access mask.</summary>
	AccessMask = 61440,
	/// <summary>A scope mask.</summary>
	ScopeMask = 15,
	/// <summary>A VTable mask.</summary>
	VTableMask = 240
}
