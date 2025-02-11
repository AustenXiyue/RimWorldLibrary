using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.FileIOPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class FileIOPermissionAttribute : CodeAccessSecurityAttribute
{
	private string append;

	private string path;

	private string read;

	private string write;

	private FileIOPermissionAccess allFiles;

	private FileIOPermissionAccess allLocalFiles;

	private string changeAccessControl;

	private string viewAccessControl;

	/// <summary>Gets or sets full access for the file or directory that is specified by the string value.</summary>
	/// <returns>The absolute path of the file or directory for full access.</returns>
	/// <exception cref="T:System.NotSupportedException">The get method is not supported for this property.</exception>
	[Obsolete("use newer properties")]
	public string All
	{
		get
		{
			throw new NotSupportedException("All");
		}
		set
		{
			append = value;
			path = value;
			read = value;
			write = value;
		}
	}

	/// <summary>Gets or sets append access for the file or directory that is specified by the string value.</summary>
	/// <returns>The absolute path of the file or directory for append access.</returns>
	public string Append
	{
		get
		{
			return append;
		}
		set
		{
			append = value;
		}
	}

	/// <summary>Gets or sets the file or directory to which to grant path discovery.</summary>
	/// <returns>The absolute path of the file or directory.</returns>
	public string PathDiscovery
	{
		get
		{
			return path;
		}
		set
		{
			path = value;
		}
	}

	/// <summary>Gets or sets read access for the file or directory specified by the string value.</summary>
	/// <returns>The absolute path of the file or directory for read access.</returns>
	public string Read
	{
		get
		{
			return read;
		}
		set
		{
			read = value;
		}
	}

	/// <summary>Gets or sets write access for the file or directory specified by the string value.</summary>
	/// <returns>The absolute path of the file or directory for write access.</returns>
	public string Write
	{
		get
		{
			return write;
		}
		set
		{
			write = value;
		}
	}

	/// <summary>Gets or sets the permitted access to all files.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Security.Permissions.FileIOPermissionAccess" /> values that represents the permissions for all files. The default is <see cref="F:System.Security.Permissions.FileIOPermissionAccess.NoAccess" />.</returns>
	public FileIOPermissionAccess AllFiles
	{
		get
		{
			return allFiles;
		}
		set
		{
			allFiles = value;
		}
	}

	/// <summary>Gets or sets the permitted access to all local files.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Security.Permissions.FileIOPermissionAccess" /> values that represents the permissions for all local files. The default is <see cref="F:System.Security.Permissions.FileIOPermissionAccess.NoAccess" />.</returns>
	public FileIOPermissionAccess AllLocalFiles
	{
		get
		{
			return allLocalFiles;
		}
		set
		{
			allLocalFiles = value;
		}
	}

	/// <summary>Gets or sets the file or directory in which access control information can be changed.</summary>
	/// <returns>The absolute path of the file or directory in which access control information can be changed.</returns>
	public string ChangeAccessControl
	{
		get
		{
			return changeAccessControl;
		}
		set
		{
			changeAccessControl = value;
		}
	}

	/// <summary>Gets or sets the file or directory in which access control information can be viewed.</summary>
	/// <returns>The absolute path of the file or directory in which access control information can be viewed.</returns>
	public string ViewAccessControl
	{
		get
		{
			return viewAccessControl;
		}
		set
		{
			viewAccessControl = value;
		}
	}

	/// <summary>Gets or sets the file or directory in which file data can be viewed and modified.</summary>
	/// <returns>The absolute path of the file or directory in which file data can be viewed and modified.</returns>
	/// <exception cref="T:System.NotSupportedException">The get accessor is called. The accessor is provided only for C# compiler compatibility.</exception>
	public string ViewAndModify
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			append = value;
			path = value;
			read = value;
			write = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.FileIOPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="action" /> parameter is not a valid <see cref="T:System.Security.Permissions.SecurityAction" />. </exception>
	public FileIOPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.FileIOPermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.FileIOPermission" /> that corresponds to this attribute.</returns>
	/// <exception cref="T:System.ArgumentException">The path information for a file or directory for which access is to be secured contains invalid characters or wildcard specifiers. </exception>
	public override IPermission CreatePermission()
	{
		FileIOPermission fileIOPermission = null;
		if (base.Unrestricted)
		{
			fileIOPermission = new FileIOPermission(PermissionState.Unrestricted);
		}
		else
		{
			fileIOPermission = new FileIOPermission(PermissionState.None);
			if (append != null)
			{
				fileIOPermission.AddPathList(FileIOPermissionAccess.Append, append);
			}
			if (path != null)
			{
				fileIOPermission.AddPathList(FileIOPermissionAccess.PathDiscovery, path);
			}
			if (read != null)
			{
				fileIOPermission.AddPathList(FileIOPermissionAccess.Read, read);
			}
			if (write != null)
			{
				fileIOPermission.AddPathList(FileIOPermissionAccess.Write, write);
			}
		}
		return fileIOPermission;
	}
}
