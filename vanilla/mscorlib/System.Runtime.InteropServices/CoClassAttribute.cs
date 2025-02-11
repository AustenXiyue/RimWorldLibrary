namespace System.Runtime.InteropServices;

/// <summary>Specifies the class identifier of a coclass imported from a type library.</summary>
[AttributeUsage(AttributeTargets.Interface, Inherited = false)]
[ComVisible(true)]
public sealed class CoClassAttribute : Attribute
{
	internal Type _CoClass;

	/// <summary>Gets the class identifier of the original coclass.</summary>
	/// <returns>A <see cref="T:System.Type" /> containing the class identifier of the original coclass.</returns>
	public Type CoClass => _CoClass;

	/// <summary>Initializes new instance of the <see cref="T:System.Runtime.InteropServices.CoClassAttribute" /> with the class identifier of the original coclass.</summary>
	/// <param name="coClass">A <see cref="T:System.Type" /> that contains the class identifier of the original coclass. </param>
	public CoClassAttribute(Type coClass)
	{
		_CoClass = coClass;
	}
}
