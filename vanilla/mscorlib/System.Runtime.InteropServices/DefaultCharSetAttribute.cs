namespace System.Runtime.InteropServices;

/// <summary>Specifies the value of the <see cref="T:System.Runtime.InteropServices.CharSet" /> enumeration. This class cannot be inherited.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Module, Inherited = false)]
public sealed class DefaultCharSetAttribute : Attribute
{
	internal CharSet _CharSet;

	/// <summary>Gets the default value of <see cref="T:System.Runtime.InteropServices.CharSet" /> for any call to <see cref="T:System.Runtime.InteropServices.DllImportAttribute" />.</summary>
	/// <returns>The default value of <see cref="T:System.Runtime.InteropServices.CharSet" /> for any call to <see cref="T:System.Runtime.InteropServices.DllImportAttribute" />.</returns>
	public CharSet CharSet => _CharSet;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.DefaultCharSetAttribute" /> class with the specified <see cref="T:System.Runtime.InteropServices.CharSet" /> value.</summary>
	/// <param name="charSet">One of the <see cref="T:System.Runtime.InteropServices.CharSet" /> values.</param>
	public DefaultCharSetAttribute(CharSet charSet)
	{
		_CharSet = charSet;
	}
}
