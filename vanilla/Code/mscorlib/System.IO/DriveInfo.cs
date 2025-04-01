using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.IO;

/// <summary>Provides access to information on a drive.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class DriveInfo : ISerializable
{
	private string drive_format;

	private string path;

	/// <summary>Indicates the amount of available free space on a drive.</summary>
	/// <returns>The amount of free space available on the drive, in bytes.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">Access to the drive information is denied.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred (for example, a disk error or a drive was not ready). </exception>
	/// <filterpriority>1</filterpriority>
	public long AvailableFreeSpace
	{
		get
		{
			GetDiskFreeSpace(path, out var availableFreeSpace, out var _, out var _);
			if (availableFreeSpace <= long.MaxValue)
			{
				return (long)availableFreeSpace;
			}
			return long.MaxValue;
		}
	}

	/// <summary>Gets the total amount of free space available on a drive.</summary>
	/// <returns>The total free space available on a drive, in bytes.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">Access to the drive information is denied.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">The drive is not mapped or does not exist.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred (for example, a disk error or a drive was not ready). </exception>
	/// <filterpriority>1</filterpriority>
	public long TotalFreeSpace
	{
		get
		{
			GetDiskFreeSpace(path, out var _, out var _, out var totalFreeSpace);
			if (totalFreeSpace <= long.MaxValue)
			{
				return (long)totalFreeSpace;
			}
			return long.MaxValue;
		}
	}

	/// <summary>Gets the total size of storage space on a drive.</summary>
	/// <returns>The total size of the drive, in bytes.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">Access to the drive information is denied.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">The drive is not mapped or does not exist. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred (for example, a disk error or a drive was not ready). </exception>
	/// <filterpriority>1</filterpriority>
	public long TotalSize
	{
		get
		{
			GetDiskFreeSpace(path, out var _, out var totalSize, out var _);
			if (totalSize <= long.MaxValue)
			{
				return (long)totalSize;
			}
			return long.MaxValue;
		}
	}

	/// <summary>Gets or sets the volume label of a drive.</summary>
	/// <returns>The volume label.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred (for example, a disk error or a drive was not ready). </exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">The drive is not mapped or does not exist.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The volume label is being set on a network or CD-ROM drive.-or-Access to the drive information is denied.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonoTODO("Currently get only works on Mono/Unix; set not implemented")]
	public string VolumeLabel
	{
		get
		{
			return path;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the name of the file system, such as NTFS or FAT32.</summary>
	/// <returns>The name of the file system on the specified drive.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">Access to the drive information is denied.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">The drive does not exist or is not mapped.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred (for example, a disk error or a drive was not ready). </exception>
	/// <filterpriority>1</filterpriority>
	public string DriveFormat => drive_format;

	/// <summary>Gets the drive type.</summary>
	/// <returns>One of the <see cref="T:System.IO.DriveType" /> values. </returns>
	/// <filterpriority>1</filterpriority>
	public DriveType DriveType => (DriveType)GetDriveTypeInternal(path);

	/// <summary>Gets the name of a drive.</summary>
	/// <returns>The name of the drive.</returns>
	/// <filterpriority>1</filterpriority>
	public string Name => path;

	/// <summary>Gets the root directory of a drive.</summary>
	/// <returns>A <see cref="T:System.IO.DirectoryInfo" /> object that contains the root directory of the drive.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public DirectoryInfo RootDirectory => new DirectoryInfo(path);

	/// <summary>Gets a value indicating whether a drive is ready.</summary>
	/// <returns>true if the drive is ready; false if the drive is not ready.</returns>
	/// <filterpriority>1</filterpriority>
	public bool IsReady => Directory.Exists(Name);

	private DriveInfo(string path, string fstype)
	{
		drive_format = fstype;
		this.path = path;
	}

	/// <summary>Provides access to information on the specified drive.</summary>
	/// <param name="driveName">A valid drive path or drive letter. This can be either uppercase or lowercase, 'a' to 'z'. A null value is not valid. </param>
	/// <exception cref="T:System.ArgumentNullException">The drive letter cannot be null. </exception>
	/// <exception cref="T:System.ArgumentException">The first letter of <paramref name="driveName" /> is not an uppercase or lowercase letter from 'a' to 'z'.-or-<paramref name="driveName" /> does not refer to a valid drive.</exception>
	public DriveInfo(string driveName)
	{
		if (!Environment.IsUnix)
		{
			if (driveName == null || driveName.Length == 0)
			{
				throw new ArgumentException("The drive name is null or empty", "driveName");
			}
			if (driveName.Length >= 2 && driveName[1] != ':')
			{
				throw new ArgumentException("Invalid drive name", "driveName");
			}
			driveName = char.ToUpperInvariant(driveName[0]) + ":\\";
		}
		DriveInfo[] drives = GetDrives();
		foreach (DriveInfo driveInfo in drives)
		{
			if (driveInfo.path == driveName)
			{
				path = driveInfo.path;
				drive_format = driveInfo.drive_format;
				path = driveInfo.path;
				return;
			}
		}
		throw new ArgumentException("The drive name does not exist", "driveName");
	}

	private static void GetDiskFreeSpace(string path, out ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace)
	{
		if (!GetDiskFreeSpaceInternal(path, out availableFreeSpace, out totalSize, out totalFreeSpace, out var error))
		{
			throw MonoIO.GetException(path, error);
		}
	}

	/// <summary>Retrieves the drive names of all logical drives on a computer.</summary>
	/// <returns>An array of type <see cref="T:System.IO.DriveInfo" /> that represents the logical drives on a computer.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred (for example, a disk error or a drive was not ready). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[MonoTODO("In windows, alldrives are 'Fixed'")]
	public static DriveInfo[] GetDrives()
	{
		string[] logicalDrives = Environment.GetLogicalDrives();
		DriveInfo[] array = new DriveInfo[logicalDrives.Length];
		int num = 0;
		string[] array2 = logicalDrives;
		foreach (string rootPathName in array2)
		{
			array[num++] = new DriveInfo(rootPathName, GetDriveFormat(rootPathName));
		}
		return array;
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data needed to serialize the target object.</summary>
	/// <param name="info">The object to populate with data.</param>
	/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a drive name as a string.</summary>
	/// <returns>The name of the drive.</returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		return Name;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool GetDiskFreeSpaceInternal(string pathName, out ulong freeBytesAvail, out ulong totalNumberOfBytes, out ulong totalNumberOfFreeBytes, out MonoIOError error);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern uint GetDriveTypeInternal(string rootPathName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string GetDriveFormat(string rootPathName);
}
