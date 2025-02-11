using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows the use of declarative security actions to determine host protection requirements. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
public sealed class HostProtectionAttribute : CodeAccessSecurityAttribute
{
	private HostProtectionResource _resources;

	/// <summary>Gets or sets a value indicating whether external process management is exposed.</summary>
	/// <returns>true if external process management is exposed; otherwise, false. The default is false.</returns>
	public bool ExternalProcessMgmt
	{
		get
		{
			return (_resources & HostProtectionResource.ExternalProcessMgmt) != 0;
		}
		set
		{
			if (value)
			{
				_resources |= HostProtectionResource.ExternalProcessMgmt;
			}
			else
			{
				_resources &= ~HostProtectionResource.ExternalProcessMgmt;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether external threading is exposed.</summary>
	/// <returns>true if external threading is exposed; otherwise, false. The default is false.</returns>
	public bool ExternalThreading
	{
		get
		{
			return (_resources & HostProtectionResource.ExternalThreading) != 0;
		}
		set
		{
			if (value)
			{
				_resources |= HostProtectionResource.ExternalThreading;
			}
			else
			{
				_resources &= ~HostProtectionResource.ExternalThreading;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether resources might leak memory if the operation is terminated.</summary>
	/// <returns>true if resources might leak memory on termination; otherwise, false.</returns>
	public bool MayLeakOnAbort
	{
		get
		{
			return (_resources & HostProtectionResource.MayLeakOnAbort) != 0;
		}
		set
		{
			if (value)
			{
				_resources |= HostProtectionResource.MayLeakOnAbort;
			}
			else
			{
				_resources &= ~HostProtectionResource.MayLeakOnAbort;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the security infrastructure is exposed.</summary>
	/// <returns>true if the security infrastructure is exposed; otherwise, false. The default is false.</returns>
	[ComVisible(true)]
	public bool SecurityInfrastructure
	{
		get
		{
			return (_resources & HostProtectionResource.SecurityInfrastructure) != 0;
		}
		set
		{
			if (value)
			{
				_resources |= HostProtectionResource.SecurityInfrastructure;
			}
			else
			{
				_resources &= ~HostProtectionResource.SecurityInfrastructure;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether self-affecting process management is exposed.</summary>
	/// <returns>true if self-affecting process management is exposed; otherwise, false. The default is false.</returns>
	public bool SelfAffectingProcessMgmt
	{
		get
		{
			return (_resources & HostProtectionResource.SelfAffectingProcessMgmt) != 0;
		}
		set
		{
			if (value)
			{
				_resources |= HostProtectionResource.SelfAffectingProcessMgmt;
			}
			else
			{
				_resources &= ~HostProtectionResource.SelfAffectingProcessMgmt;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether self-affecting threading is exposed.</summary>
	/// <returns>true if self-affecting threading is exposed; otherwise, false. The default is false.</returns>
	public bool SelfAffectingThreading
	{
		get
		{
			return (_resources & HostProtectionResource.SelfAffectingThreading) != 0;
		}
		set
		{
			if (value)
			{
				_resources |= HostProtectionResource.SelfAffectingThreading;
			}
			else
			{
				_resources &= ~HostProtectionResource.SelfAffectingThreading;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether shared state is exposed.</summary>
	/// <returns>true if shared state is exposed; otherwise, false. The default is false.</returns>
	public bool SharedState
	{
		get
		{
			return (_resources & HostProtectionResource.SharedState) != 0;
		}
		set
		{
			if (value)
			{
				_resources |= HostProtectionResource.SharedState;
			}
			else
			{
				_resources &= ~HostProtectionResource.SharedState;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether synchronization is exposed.</summary>
	/// <returns>true if synchronization is exposed; otherwise, false. The default is false.</returns>
	public bool Synchronization
	{
		get
		{
			return (_resources & HostProtectionResource.Synchronization) != 0;
		}
		set
		{
			if (value)
			{
				_resources |= HostProtectionResource.Synchronization;
			}
			else
			{
				_resources &= ~HostProtectionResource.Synchronization;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the user interface is exposed.</summary>
	/// <returns>true if the user interface is exposed; otherwise, false. The default is false.</returns>
	public bool UI
	{
		get
		{
			return (_resources & HostProtectionResource.UI) != 0;
		}
		set
		{
			if (value)
			{
				_resources |= HostProtectionResource.UI;
			}
			else
			{
				_resources &= ~HostProtectionResource.UI;
			}
		}
	}

	/// <summary>Gets or sets flags specifying categories of functionality that are potentially harmful to the host.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Security.Permissions.HostProtectionResource" /> values. The default is <see cref="F:System.Security.Permissions.HostProtectionResource.None" />.</returns>
	public HostProtectionResource Resources
	{
		get
		{
			return _resources;
		}
		set
		{
			_resources = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.HostProtectionAttribute" /> class with default values.</summary>
	public HostProtectionAttribute()
		: base(SecurityAction.LinkDemand)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.HostProtectionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" /> value.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="action" /> is not <see cref="F:System.Security.Permissions.SecurityAction.LinkDemand" />. </exception>
	public HostProtectionAttribute(SecurityAction action)
		: base(action)
	{
		if (action != SecurityAction.LinkDemand)
		{
			throw new ArgumentException(string.Format(Locale.GetText("Only {0} is accepted."), SecurityAction.LinkDemand), "action");
		}
	}

	/// <summary>Creates and returns a new host protection permission.</summary>
	/// <returns>An <see cref="T:System.Security.IPermission" /> that corresponds to the current attribute.</returns>
	public override IPermission CreatePermission()
	{
		return new HostProtectionPermission(_resources);
	}
}
