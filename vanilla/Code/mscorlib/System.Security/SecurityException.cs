using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;

namespace System.Security;

/// <summary>The exception that is thrown when a security error is detected.</summary>
[Serializable]
[ComVisible(true)]
public class SecurityException : SystemException
{
	private string permissionState;

	private Type permissionType;

	private string _granted;

	private string _refused;

	private object _demanded;

	private IPermission _firstperm;

	private MethodInfo _method;

	private Evidence _evidence;

	private SecurityAction _action;

	private object _denyset;

	private object _permitset;

	private AssemblyName _assembly;

	private string _url;

	private SecurityZone _zone;

	/// <summary>Gets or sets the security action that caused the exception.</summary>
	/// <returns>One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values.</returns>
	[ComVisible(false)]
	public SecurityAction Action
	{
		get
		{
			return _action;
		}
		set
		{
			_action = value;
		}
	}

	/// <summary>Gets or sets the denied security permission, permission set, or permission set collection that caused a demand to fail.</summary>
	/// <returns>A permission, permission set, or permission set collection object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[ComVisible(false)]
	public object DenySetInstance
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return _denyset;
		}
		set
		{
			_denyset = value;
		}
	}

	/// <summary>Gets or sets information about the failed assembly.</summary>
	/// <returns>An <see cref="T:System.Reflection.AssemblyName" /> that identifies the failed assembly.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[ComVisible(false)]
	public AssemblyName FailedAssemblyInfo
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return _assembly;
		}
		set
		{
			_assembly = value;
		}
	}

	/// <summary>Gets or sets the information about the method associated with the exception.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> object describing the method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[ComVisible(false)]
	public MethodInfo Method
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return _method;
		}
		set
		{
			_method = value;
		}
	}

	/// <summary>Gets or sets the permission, permission set, or permission set collection that is part of the permit-only stack frame that caused a security check to fail.</summary>
	/// <returns>A permission, permission set, or permission set collection object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[ComVisible(false)]
	public object PermitOnlySetInstance
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return _permitset;
		}
		set
		{
			_permitset = value;
		}
	}

	/// <summary>Gets or sets the URL of the assembly that caused the exception.</summary>
	/// <returns>A URL that identifies the location of the assembly.</returns>
	public string Url
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return _url;
		}
		set
		{
			_url = value;
		}
	}

	/// <summary>Gets or sets the zone of the assembly that caused the exception.</summary>
	/// <returns>One of the <see cref="T:System.Security.SecurityZone" /> values that identifies the zone of the assembly that caused the exception.</returns>
	public SecurityZone Zone
	{
		get
		{
			return _zone;
		}
		set
		{
			_zone = value;
		}
	}

	/// <summary>Gets or sets the demanded security permission, permission set, or permission set collection that failed.</summary>
	/// <returns>A permission, permission set, or permission set collection object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[ComVisible(false)]
	public object Demanded
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return _demanded;
		}
		set
		{
			_demanded = value;
		}
	}

	/// <summary>Gets or sets the first permission in a permission set or permission set collection that failed the demand.</summary>
	/// <returns>An <see cref="T:System.Security.IPermission" /> object representing the first permission that failed.</returns>
	public IPermission FirstPermissionThatFailed
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return _firstperm;
		}
		set
		{
			_firstperm = value;
		}
	}

	/// <summary>Gets or sets the state of the permission that threw the exception.</summary>
	/// <returns>The state of the permission at the time the exception was thrown.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	public string PermissionState
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return permissionState;
		}
		set
		{
			permissionState = value;
		}
	}

	/// <summary>Gets or sets the type of the permission that failed.</summary>
	/// <returns>The type of the permission that failed.</returns>
	public Type PermissionType
	{
		get
		{
			return permissionType;
		}
		set
		{
			permissionType = value;
		}
	}

	/// <summary>Gets or sets the granted permission set of the assembly that caused the <see cref="T:System.Security.SecurityException" />.</summary>
	/// <returns>The XML representation of the granted set of the assembly.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	public string GrantedSet
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return _granted;
		}
		set
		{
			_granted = value;
		}
	}

	/// <summary>Gets or sets the refused permission set of the assembly that caused the <see cref="T:System.Security.SecurityException" />.</summary>
	/// <returns>The XML representation of the refused permission set of the assembly.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	public string RefusedSet
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true, ControlPolicy = true)]
		get
		{
			return _refused;
		}
		set
		{
			_refused = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityException" /> class with default properties.</summary>
	public SecurityException()
		: this(Locale.GetText("A security error has been detected."))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityException" /> class with a specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	public SecurityException(string message)
		: base(message)
	{
		base.HResult = -2146233078;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info " />is null.</exception>
	protected SecurityException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		base.HResult = -2146233078;
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Name == "PermissionState")
			{
				permissionState = (string)enumerator.Value;
				break;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public SecurityException(string message, Exception inner)
		: base(message, inner)
	{
		base.HResult = -2146233078;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityException" /> class with a specified error message and the permission type that caused the exception to be thrown.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="type">The type of the permission that caused the exception to be thrown. </param>
	public SecurityException(string message, Type type)
		: base(message)
	{
		base.HResult = -2146233078;
		permissionType = type;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityException" /> class with a specified error message, the permission type that caused the exception to be thrown, and the permission state.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="type">The type of the permission that caused the exception to be thrown. </param>
	/// <param name="state">The state of the permission that caused the exception to be thrown. </param>
	public SecurityException(string message, Type type, string state)
		: base(message)
	{
		base.HResult = -2146233078;
		permissionType = type;
		permissionState = state;
	}

	internal SecurityException(string message, PermissionSet granted, PermissionSet refused)
		: base(message)
	{
		base.HResult = -2146233078;
		_granted = granted.ToString();
		_refused = refused.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityException" /> class for an exception caused by a Deny on the stack.  </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="deny">The denied permission or permission set.</param>
	/// <param name="permitOnly">The permit-only permission or permission set.</param>
	/// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> that identifies the method that encountered the exception.</param>
	/// <param name="demanded">The demanded permission, permission set, or permission set collection.</param>
	/// <param name="permThatFailed">An <see cref="T:System.Security.IPermission" /> that identifies the permission that failed.</param>
	public SecurityException(string message, object deny, object permitOnly, MethodInfo method, object demanded, IPermission permThatFailed)
		: base(message)
	{
		base.HResult = -2146233078;
		_denyset = deny;
		_permitset = permitOnly;
		_method = method;
		_demanded = demanded;
		_firstperm = permThatFailed;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityException" /> class for an exception caused by an insufficient grant set. </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="assemblyName">An <see cref="T:System.Reflection.AssemblyName" /> that specifies the name of the assembly that caused the exception.</param>
	/// <param name="grant">A <see cref="T:System.Security.PermissionSet" /> that represents the permissions granted the assembly.</param>
	/// <param name="refused">A <see cref="T:System.Security.PermissionSet" /> that represents the refused permission or permission set.</param>
	/// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> that represents the method that encountered the exception.</param>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values.</param>
	/// <param name="demanded">The demanded permission, permission set, or permission set collection.</param>
	/// <param name="permThatFailed">An <see cref="T:System.Security.IPermission" /> that represents the permission that failed.</param>
	/// <param name="evidence">The <see cref="T:System.Security.Policy.Evidence" /> for the assembly that caused the exception.</param>
	public SecurityException(string message, AssemblyName assemblyName, PermissionSet grant, PermissionSet refused, MethodInfo method, SecurityAction action, object demanded, IPermission permThatFailed, Evidence evidence)
		: base(message)
	{
		base.HResult = -2146233078;
		_assembly = assemblyName;
		_granted = ((grant == null) ? string.Empty : grant.ToString());
		_refused = ((refused == null) ? string.Empty : refused.ToString());
		_method = method;
		_action = action;
		_demanded = demanded;
		_firstperm = permThatFailed;
		if (_firstperm != null)
		{
			permissionType = _firstperm.GetType();
		}
		_evidence = evidence;
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the <see cref="T:System.Security.SecurityException" />.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		try
		{
			info.AddValue("PermissionState", permissionState);
		}
		catch (SecurityException)
		{
		}
	}

	/// <summary>Returns a representation of the current <see cref="T:System.Security.SecurityException" />.</summary>
	/// <returns>A string representation of the current <see cref="T:System.Security.SecurityException" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(base.ToString());
		try
		{
			if (permissionType != null)
			{
				stringBuilder.AppendFormat("{0}Type: {1}", Environment.NewLine, PermissionType);
			}
			if (_method != null)
			{
				string text = _method.ToString();
				int startIndex = text.IndexOf(" ") + 1;
				stringBuilder.AppendFormat("{0}Method: {1} {2}.{3}", Environment.NewLine, _method.ReturnType.Name, _method.ReflectedType, text.Substring(startIndex));
			}
			if (permissionState != null)
			{
				stringBuilder.AppendFormat("{0}State: {1}", Environment.NewLine, PermissionState);
			}
			if (_granted != null && _granted.Length > 0)
			{
				stringBuilder.AppendFormat("{0}Granted: {1}", Environment.NewLine, GrantedSet);
			}
			if (_refused != null && _refused.Length > 0)
			{
				stringBuilder.AppendFormat("{0}Refused: {1}", Environment.NewLine, RefusedSet);
			}
			if (_demanded != null)
			{
				stringBuilder.AppendFormat("{0}Demanded: {1}", Environment.NewLine, Demanded);
			}
			if (_firstperm != null)
			{
				stringBuilder.AppendFormat("{0}Failed Permission: {1}", Environment.NewLine, FirstPermissionThatFailed);
			}
			if (_evidence != null)
			{
				stringBuilder.AppendFormat("{0}Evidences:", Environment.NewLine);
				foreach (object item in _evidence)
				{
					if (!(item is Hash))
					{
						stringBuilder.AppendFormat("{0}\t{1}", Environment.NewLine, item);
					}
				}
			}
		}
		catch (SecurityException)
		{
		}
		return stringBuilder.ToString();
	}
}
