namespace System.Runtime.InteropServices;

/// <summary>Use <see cref="T:System.Runtime.InteropServices.ComTypes.ELEMDESC" /> instead.</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
[Obsolete("Use System.Runtime.InteropServices.ComTypes.ELEMDESC instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
public struct ELEMDESC
{
	/// <summary>Use <see cref="T:System.Runtime.InteropServices.ComTypes.ELEMDESC.DESCUNION" /> instead.</summary>
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
	[ComVisible(false)]
	public struct DESCUNION
	{
		/// <summary>Contains information for remoting the element.</summary>
		[FieldOffset(0)]
		public IDLDESC idldesc;

		/// <summary>Contains information about the parameter.</summary>
		[FieldOffset(0)]
		public PARAMDESC paramdesc;
	}

	/// <summary>Identifies the type of the element.</summary>
	public TYPEDESC tdesc;

	/// <summary>Contains information about an element.</summary>
	public DESCUNION desc;
}
