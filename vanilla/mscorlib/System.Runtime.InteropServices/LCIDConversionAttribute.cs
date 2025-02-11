namespace System.Runtime.InteropServices;

/// <summary>Indicates that a method's unmanaged signature expects a locale identifier (LCID) parameter.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[ComVisible(true)]
public sealed class LCIDConversionAttribute : Attribute
{
	internal int _val;

	/// <summary>Gets the position of the LCID argument in the unmanaged signature.</summary>
	/// <returns>The position of the LCID argument in the unmanaged signature, where 0 is the first argument.</returns>
	public int Value => _val;

	/// <summary>Initializes a new instance of the LCIDConversionAttribute class with the position of the LCID in the unmanaged signature.</summary>
	/// <param name="lcid">Indicates the position of the LCID argument in the unmanaged signature, where 0 is the first argument. </param>
	public LCIDConversionAttribute(int lcid)
	{
		_val = lcid;
	}
}
