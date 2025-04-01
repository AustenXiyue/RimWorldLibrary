namespace System.Runtime.InteropServices;

/// <summary>Identifies a list of interfaces that are exposed as COM event sources for the attributed class.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ComSourceInterfacesAttribute : Attribute
{
	internal string _val;

	/// <summary>Gets the fully qualified name of the event source interface.</summary>
	/// <returns>The fully qualified name of the event source interface.</returns>
	public string Value => _val;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ComSourceInterfacesAttribute" /> class with the name of the event source interface.</summary>
	/// <param name="sourceInterfaces">A null-delimited list of fully qualified event source interface names. </param>
	public ComSourceInterfacesAttribute(string sourceInterfaces)
	{
		_val = sourceInterfaces;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ComSourceInterfacesAttribute" /> class with the type to use as a source interface.</summary>
	/// <param name="sourceInterface">The <see cref="T:System.Type" /> of the source interface. </param>
	public ComSourceInterfacesAttribute(Type sourceInterface)
	{
		_val = sourceInterface.FullName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ComSourceInterfacesAttribute" /> class with the types to use as source interfaces.</summary>
	/// <param name="sourceInterface1">The <see cref="T:System.Type" /> of the default source interface. </param>
	/// <param name="sourceInterface2">The <see cref="T:System.Type" /> of a source interface. </param>
	public ComSourceInterfacesAttribute(Type sourceInterface1, Type sourceInterface2)
	{
		_val = sourceInterface1.FullName + "\0" + sourceInterface2.FullName;
	}

	/// <summary>Initializes a new instance of the ComSourceInterfacesAttribute class with the types to use as source interfaces.</summary>
	/// <param name="sourceInterface1">The <see cref="T:System.Type" /> of the default source interface. </param>
	/// <param name="sourceInterface2">The <see cref="T:System.Type" /> of a source interface. </param>
	/// <param name="sourceInterface3">The <see cref="T:System.Type" /> of a source interface. </param>
	public ComSourceInterfacesAttribute(Type sourceInterface1, Type sourceInterface2, Type sourceInterface3)
	{
		_val = sourceInterface1.FullName + "\0" + sourceInterface2.FullName + "\0" + sourceInterface3.FullName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ComSourceInterfacesAttribute" /> class with the types to use as source interfaces.</summary>
	/// <param name="sourceInterface1">The <see cref="T:System.Type" /> of the default source interface. </param>
	/// <param name="sourceInterface2">The <see cref="T:System.Type" /> of a source interface. </param>
	/// <param name="sourceInterface3">The <see cref="T:System.Type" /> of a source interface. </param>
	/// <param name="sourceInterface4">The <see cref="T:System.Type" /> of a source interface. </param>
	public ComSourceInterfacesAttribute(Type sourceInterface1, Type sourceInterface2, Type sourceInterface3, Type sourceInterface4)
	{
		_val = sourceInterface1.FullName + "\0" + sourceInterface2.FullName + "\0" + sourceInterface3.FullName + "\0" + sourceInterface4.FullName;
	}
}
