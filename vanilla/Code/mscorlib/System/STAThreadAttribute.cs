using System.Runtime.InteropServices;

namespace System;

/// <summary>Indicates that the COM threading model for an application is single-threaded apartment (STA). </summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Method)]
[ComVisible(true)]
public sealed class STAThreadAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.STAThreadAttribute" /> class.</summary>
	public STAThreadAttribute()
	{
	}
}
