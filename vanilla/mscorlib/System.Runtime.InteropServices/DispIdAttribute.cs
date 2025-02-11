namespace System.Runtime.InteropServices;

/// <summary>Specifies the COM dispatch identifier (DISPID) of a method, field, or property.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event, Inherited = false)]
[ComVisible(true)]
public sealed class DispIdAttribute : Attribute
{
	internal int _val;

	/// <summary>Gets the DISPID for the member.</summary>
	/// <returns>The DISPID for the member.</returns>
	public int Value => _val;

	/// <summary>Initializes a new instance of the DispIdAttribute class with the specified DISPID.</summary>
	/// <param name="dispId">The DISPID for the member. </param>
	public DispIdAttribute(int dispId)
	{
		_val = dispId;
	}
}
