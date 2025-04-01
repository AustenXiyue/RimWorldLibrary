namespace System.Runtime.InteropServices;

/// <summary>Supplies an explicit <see cref="T:System.Guid" /> when an automatic GUID is undesirable.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
public sealed class GuidAttribute : Attribute
{
	internal string _val;

	/// <summary>Gets the <see cref="T:System.Guid" /> of the class.</summary>
	/// <returns>The <see cref="T:System.Guid" /> of the class.</returns>
	public string Value => _val;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.GuidAttribute" /> class with the specified GUID.</summary>
	/// <param name="guid">The <see cref="T:System.Guid" /> to be assigned. </param>
	public GuidAttribute(string guid)
	{
		_val = guid;
	}
}
