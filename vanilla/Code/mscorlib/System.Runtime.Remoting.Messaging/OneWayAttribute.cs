using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Marks a method as one way, without a return value and out or ref parameters.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Method)]
public class OneWayAttribute : Attribute
{
	/// <summary>Creates an instance of <see cref="T:System.Runtime.Remoting.Messaging.OneWayAttribute" />.</summary>
	public OneWayAttribute()
	{
	}
}
