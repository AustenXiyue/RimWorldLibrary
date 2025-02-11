using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>The ManagementEntity attribute indicates that a class provides management information exposed through a WMI provider.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ManagementEntityAttribute : Attribute
{
	/// <summary>Gets or sets a value that specifies whether the class represents a WMI class in a provider implemented external to the current assembly.</summary>
	/// <returns>A boolean value that is true if the class represents an external WMI class and false otherwise.</returns>
	public bool External
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets the name of the WMI class.</summary>
	/// <returns>A string that contains the name of the WMI class.</returns>
	public string Name
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Specifies whether the associated class represents a singleton WMI class.</summary>
	/// <returns>A boolean value that is true if the class represents a singleton WMI class and false otherwise.</returns>
	public bool Singleton
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.ManagementEntityAttribute" /> class. This is the default constructor.</summary>
	public ManagementEntityAttribute()
	{
	}
}
