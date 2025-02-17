using System.Runtime.InteropServices;

namespace System;

/// <summary>Indicates that the COM threading model for an application is multithreaded apartment (MTA). </summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Method)]
[ComVisible(true)]
public sealed class MTAThreadAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.MTAThreadAttribute" /> class.</summary>
	public MTAThreadAttribute()
	{
	}
}
