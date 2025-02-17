namespace System.Runtime.InteropServices;

/// <summary>Indicates the type of class interface to be generated for a class exposed to COM, if an interface is generated at all.</summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = false)]
[ComVisible(true)]
public sealed class ClassInterfaceAttribute : Attribute
{
	internal ClassInterfaceType _val;

	/// <summary>Gets the <see cref="T:System.Runtime.InteropServices.ClassInterfaceType" /> value that describes which type of interface should be generated for the class.</summary>
	/// <returns>The <see cref="T:System.Runtime.InteropServices.ClassInterfaceType" /> value that describes which type of interface should be generated for the class.</returns>
	public ClassInterfaceType Value => _val;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ClassInterfaceAttribute" /> class with the specified <see cref="T:System.Runtime.InteropServices.ClassInterfaceType" /> enumeration member.</summary>
	/// <param name="classInterfaceType">One of the <see cref="T:System.Runtime.InteropServices.ClassInterfaceType" /> values that describes the type of interface that is generated for a class. </param>
	public ClassInterfaceAttribute(ClassInterfaceType classInterfaceType)
	{
		_val = classInterfaceType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ClassInterfaceAttribute" /> class with the specified <see cref="T:System.Runtime.InteropServices.ClassInterfaceType" /> enumeration value.</summary>
	/// <param name="classInterfaceType">Describes the type of interface that is generated for a class. </param>
	public ClassInterfaceAttribute(short classInterfaceType)
	{
		_val = (ClassInterfaceType)classInterfaceType;
	}
}
