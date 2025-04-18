using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Represents an abstraction of an access control entry (ACE) that defines an access rule for a file or directory. This class cannot be inherited.</summary>
public sealed class FileSystemAccessRule : AccessRule
{
	/// <summary>Gets the <see cref="T:System.Security.AccessControl.FileSystemRights" /> flags associated with the current <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> object.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.FileSystemRights" /> flags associated with the current <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> object.</returns>
	public FileSystemRights FileSystemRights => (FileSystemRights)base.AccessMask;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> class using a reference to a user account, a value that specifies the type of operation associated with the access rule, and a value that specifies whether to allow or deny the operation. </summary>
	/// <param name="identity">An <see cref="T:System.Security.Principal.IdentityReference" /> object that encapsulates a reference to a user account.</param>
	/// <param name="fileSystemRights">One of the <see cref="T:System.Security.AccessControl.FileSystemRights" /> values that specifies the type of operation associated with the access rule. </param>
	/// <param name="type">One of the <see cref="T:System.Security.AccessControl.AccessControlType" /> values that specifies whether to allow or deny the operation.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="identity" /> parameter is not an <see cref="T:System.Security.Principal.IdentityReference" /> object.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="identity" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">An incorrect enumeration was passed to the <paramref name="type " />parameter.</exception>
	public FileSystemAccessRule(IdentityReference identity, FileSystemRights fileSystemRights, AccessControlType type)
		: this(identity, fileSystemRights, InheritanceFlags.None, PropagationFlags.None, type)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> class using the name of a user account, a value that specifies the type of operation associated with the access rule, and a value that describes whether to allow or deny the operation. </summary>
	/// <param name="identity">The name of a user account.</param>
	/// <param name="fileSystemRights">One of the <see cref="T:System.Security.AccessControl.FileSystemRights" /> values that specifies the type of operation associated with the access rule. </param>
	/// <param name="type">One of the <see cref="T:System.Security.AccessControl.AccessControlType" /> values that specifies whether to allow or deny the operation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="identity" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">An incorrect enumeration was passed to the <paramref name="type " />parameter.</exception>
	public FileSystemAccessRule(string identity, FileSystemRights fileSystemRights, AccessControlType type)
		: this(new NTAccount(identity), fileSystemRights, InheritanceFlags.None, PropagationFlags.None, type)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> class using a reference to a user account, a value that specifies the type of operation associated with the access rule, a value that determines how rights are inherited, a value that determines how rights are propagated, and a value that specifies whether to allow or deny the operation.</summary>
	/// <param name="identity">An <see cref="T:System.Security.Principal.IdentityReference" /> object that encapsulates a reference to a user account.</param>
	/// <param name="fileSystemRights">One of the <see cref="T:System.Security.AccessControl.FileSystemRights" /> values that specifies the type of operation associated with the access rule.</param>
	/// <param name="inheritanceFlags">One of the <see cref="T:System.Security.AccessControl.InheritanceFlags" /> values that specifies how access masks are propagated to child objects.</param>
	/// <param name="propagationFlags">One of the <see cref="T:System.Security.AccessControl.PropagationFlags" /> values that specifies how Access Control Entries (ACEs) are propagated to child objects.</param>
	/// <param name="type">One of the <see cref="T:System.Security.AccessControl.AccessControlType" /> values that specifies whether to allow or deny the operation.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="identity" /> parameter is not an <see cref="T:System.Security.Principal.IdentityReference" /> object.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="identity" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">An incorrect enumeration was passed to the <paramref name="type " />parameter.-or-An incorrect enumeration was passed to the <paramref name="inheritanceFlags " />parameter.-or-An incorrect enumeration was passed to the <paramref name="propagationFlags " />parameter.</exception>
	public FileSystemAccessRule(IdentityReference identity, FileSystemRights fileSystemRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		: this(identity, fileSystemRights, isInherited: false, inheritanceFlags, propagationFlags, type)
	{
	}

	internal FileSystemAccessRule(IdentityReference identity, FileSystemRights fileSystemRights, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		: base(identity, (int)fileSystemRights, isInherited, inheritanceFlags, propagationFlags, type)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> class using the name of a user account, a value that specifies the type of operation associated with the access rule, a value that determines how rights are inherited, a value that determines how rights are propagated, and a value that specifies whether to allow or deny the operation.</summary>
	/// <param name="identity">The name of a user account.</param>
	/// <param name="fileSystemRights">One of the <see cref="T:System.Security.AccessControl.FileSystemRights" /> values that specifies the type of operation associated with the access rule.</param>
	/// <param name="inheritanceFlags">One of the <see cref="T:System.Security.AccessControl.InheritanceFlags" /> values that specifies how access masks are propagated to child objects.</param>
	/// <param name="propagationFlags">One of the <see cref="T:System.Security.AccessControl.PropagationFlags" /> values that specifies how Access Control Entries (ACEs) are propagated to child objects.</param>
	/// <param name="type">One of the <see cref="T:System.Security.AccessControl.AccessControlType" /> values that specifies whether to allow or deny the operation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="identity" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">An incorrect enumeration was passed to the <paramref name="type " />parameter.-or-An incorrect enumeration was passed to the <paramref name="inheritanceFlags " />parameter.-or-An incorrect enumeration was passed to the <paramref name="propagationFlags " />parameter.</exception>
	public FileSystemAccessRule(string identity, FileSystemRights fileSystemRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		: this(new NTAccount(identity), fileSystemRights, inheritanceFlags, propagationFlags, type)
	{
	}
}
