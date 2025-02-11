using System.Runtime.InteropServices;

namespace System.Security;

/// <summary>Allows an assembly to be called by partially trusted code. Without this declaration, only fully trusted callers are able to use the assembly. This class cannot be inherited.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class AllowPartiallyTrustedCallersAttribute : Attribute
{
	private PartialTrustVisibilityLevel _visibilityLevel;

	/// <summary>Gets or sets the default partial trust visibility for code that is marked with the <see cref="T:System.Security.AllowPartiallyTrustedCallersAttribute" /> (APTCA) attribute.</summary>
	/// <returns>One of the enumeration values. The default is <see cref="F:System.Security.PartialTrustVisibilityLevel.VisibleToAllHosts" />. </returns>
	public PartialTrustVisibilityLevel PartialTrustVisibilityLevel
	{
		get
		{
			return _visibilityLevel;
		}
		set
		{
			_visibilityLevel = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AllowPartiallyTrustedCallersAttribute" /> class. </summary>
	public AllowPartiallyTrustedCallersAttribute()
	{
	}
}
