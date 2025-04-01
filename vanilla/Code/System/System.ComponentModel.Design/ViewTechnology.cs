using System.Runtime.InteropServices;

namespace System.ComponentModel.Design;

/// <summary>Defines identifiers for a set of technologies that designer hosts support.</summary>
[ComVisible(true)]
public enum ViewTechnology
{
	/// <summary>Represents a mode in which the view object is passed directly to the development environment. </summary>
	[Obsolete("This value has been deprecated. Use ViewTechnology.Default instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
	Passthrough,
	/// <summary>Represents a mode in which a Windows Forms control object provides the display for the root designer. </summary>
	[Obsolete("This value has been deprecated. Use ViewTechnology.Default instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
	WindowsForms,
	/// <summary>Specifies the default view technology support. </summary>
	Default
}
