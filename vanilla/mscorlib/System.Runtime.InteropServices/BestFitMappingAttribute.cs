namespace System.Runtime.InteropServices;

/// <summary>Controls whether Unicode characters are converted to the closest matching ANSI characters.</summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
[ComVisible(true)]
public sealed class BestFitMappingAttribute : Attribute
{
	internal bool _bestFitMapping;

	/// <summary>Enables or disables the throwing of an exception on an unmappable Unicode character that is converted to an ANSI '?' character.</summary>
	public bool ThrowOnUnmappableChar;

	/// <summary>Gets the best-fit mapping behavior when converting Unicode characters to ANSI characters.</summary>
	/// <returns>true if best-fit mapping is enabled; otherwise, false. The default is true.</returns>
	public bool BestFitMapping => _bestFitMapping;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.BestFitMappingAttribute" /> class set to the value of the <see cref="P:System.Runtime.InteropServices.BestFitMappingAttribute.BestFitMapping" /> property.</summary>
	/// <param name="BestFitMapping">true to indicate that best-fit mapping is enabled; otherwise, false. The default is true. </param>
	public BestFitMappingAttribute(bool BestFitMapping)
	{
		_bestFitMapping = BestFitMapping;
	}
}
