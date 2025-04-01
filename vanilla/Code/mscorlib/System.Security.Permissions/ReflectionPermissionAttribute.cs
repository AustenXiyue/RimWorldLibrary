using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.ReflectionPermission" /> to be applied to code using declarative security. </summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
[ComVisible(true)]
public sealed class ReflectionPermissionAttribute : CodeAccessSecurityAttribute
{
	private ReflectionPermissionFlag flags;

	private bool memberAccess;

	private bool reflectionEmit;

	private bool typeInfo;

	/// <summary>Gets or sets the current allowed uses of reflection.</summary>
	/// <returns>One or more of the <see cref="T:System.Security.Permissions.ReflectionPermissionFlag" /> values combined using a bitwise OR.</returns>
	/// <exception cref="T:System.ArgumentException">An attempt is made to set this property to an invalid value. See <see cref="T:System.Security.Permissions.ReflectionPermissionFlag" /> for the valid values. </exception>
	public ReflectionPermissionFlag Flags
	{
		get
		{
			return flags;
		}
		set
		{
			flags = value;
			memberAccess = (flags & ReflectionPermissionFlag.MemberAccess) == ReflectionPermissionFlag.MemberAccess;
			reflectionEmit = (flags & ReflectionPermissionFlag.ReflectionEmit) == ReflectionPermissionFlag.ReflectionEmit;
			typeInfo = (flags & ReflectionPermissionFlag.TypeInformation) == ReflectionPermissionFlag.TypeInformation;
		}
	}

	/// <summary>Gets or sets a value that indicates whether invocation of operations on non-public members is allowed.</summary>
	/// <returns>true if invocation of operations on non-public members is allowed; otherwise, false.</returns>
	public bool MemberAccess
	{
		get
		{
			return memberAccess;
		}
		set
		{
			if (value)
			{
				flags |= ReflectionPermissionFlag.MemberAccess;
			}
			else
			{
				flags -= 2;
			}
			memberAccess = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether use of certain features in <see cref="N:System.Reflection.Emit" />, such as emitting debug symbols, is allowed.</summary>
	/// <returns>true if use of the affected features is allowed; otherwise, false.</returns>
	[Obsolete]
	public bool ReflectionEmit
	{
		get
		{
			return reflectionEmit;
		}
		set
		{
			if (value)
			{
				flags |= ReflectionPermissionFlag.ReflectionEmit;
			}
			else
			{
				flags -= 4;
			}
			reflectionEmit = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether restricted invocation of non-public members is allowed. Restricted invocation means that the grant set of the assembly that contains the non-public member that is being invoked must be equal to, or a subset of, the grant set of the invoking assembly. </summary>
	/// <returns>true if restricted invocation of non-public members is allowed; otherwise, false.</returns>
	public bool RestrictedMemberAccess
	{
		get
		{
			return (flags & ReflectionPermissionFlag.RestrictedMemberAccess) == ReflectionPermissionFlag.RestrictedMemberAccess;
		}
		set
		{
			if (value)
			{
				flags |= ReflectionPermissionFlag.RestrictedMemberAccess;
			}
			else
			{
				flags -= 8;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether reflection on members that are not visible is allowed.</summary>
	/// <returns>true if reflection on members that are not visible is allowed; otherwise, false.</returns>
	[Obsolete("not enforced in 2.0+")]
	public bool TypeInformation
	{
		get
		{
			return typeInfo;
		}
		set
		{
			if (value)
			{
				flags |= ReflectionPermissionFlag.TypeInformation;
			}
			else
			{
				flags--;
			}
			typeInfo = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.ReflectionPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public ReflectionPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.ReflectionPermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.ReflectionPermission" /> that corresponds to this attribute.</returns>
	public override IPermission CreatePermission()
	{
		ReflectionPermission reflectionPermission = null;
		if (base.Unrestricted)
		{
			return new ReflectionPermission(PermissionState.Unrestricted);
		}
		return new ReflectionPermission(flags);
	}
}
