using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Mono;

namespace System;

/// <summary>Provides information about, and means to manipulate, the current environment and platform. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[ComVisible(true)]
public static class Environment
{
	/// <summary>Specifies enumerated constants used to retrieve directory paths to system special folders.</summary>
	[ComVisible(true)]
	public enum SpecialFolder
	{
		/// <summary>The My Documents folder.</summary>
		MyDocuments = 5,
		/// <summary>The logical Desktop rather than the physical file system location.</summary>
		Desktop = 0,
		/// <summary>The My Computer folder. </summary>
		MyComputer = 17,
		/// <summary>The directory that contains the user's program groups.</summary>
		Programs = 2,
		/// <summary>The directory that serves as a common repository for documents.</summary>
		Personal = 5,
		/// <summary>The directory that serves as a common repository for the user's favorite items.</summary>
		Favorites = 6,
		/// <summary>The directory that corresponds to the user's Startup program group.</summary>
		Startup = 7,
		/// <summary>The directory that contains the user's most recently used documents.</summary>
		Recent = 8,
		/// <summary>The directory that contains the Send To menu items.</summary>
		SendTo = 9,
		/// <summary>The directory that contains the Start menu items.</summary>
		StartMenu = 11,
		/// <summary>The My Music folder.</summary>
		MyMusic = 13,
		/// <summary>The directory used to physically store file objects on the desktop.</summary>
		DesktopDirectory = 16,
		/// <summary>The directory that serves as a common repository for document templates.</summary>
		Templates = 21,
		/// <summary>The directory that serves as a common repository for application-specific data for the current roaming user.</summary>
		ApplicationData = 26,
		/// <summary>The directory that serves as a common repository for application-specific data that is used by the current, non-roaming user.</summary>
		LocalApplicationData = 28,
		/// <summary>The directory that serves as a common repository for temporary Internet files.</summary>
		InternetCache = 32,
		/// <summary>The directory that serves as a common repository for Internet cookies.</summary>
		Cookies = 33,
		/// <summary>The directory that serves as a common repository for Internet history items.</summary>
		History = 34,
		/// <summary>The directory that serves as a common repository for application-specific data that is used by all users.</summary>
		CommonApplicationData = 35,
		/// <summary>The System directory.</summary>
		System = 37,
		/// <summary>The program files directory.On a non-x86 system, passing <see cref="F:System.Environment.SpecialFolder.ProgramFiles" /> to the <see cref="M:System.Environment.GetFolderPath(System.Environment.SpecialFolder)" /> method returns the path for non-x86 programs. To get the x86 program files directory on a non-x86 system, use the <see cref="F:System.Environment.SpecialFolder.ProgramFilesX86" /> member. </summary>
		ProgramFiles = 38,
		/// <summary>The My Pictures folder.</summary>
		MyPictures = 39,
		/// <summary>The directory for components that are shared across applications.To get the x86 common program files directory on a non-x86 system, use the <see cref="F:System.Environment.SpecialFolder.ProgramFilesX86" /> member. </summary>
		CommonProgramFiles = 43,
		/// <summary>The file system directory that serves as a repository for videos that belong to a user.  Added in the .NET Framework 4. </summary>
		MyVideos = 14,
		/// <summary>A file system directory that contains the link objects that may exist in the My Network Places virtual folder. Added in the .NET Framework 4. </summary>
		NetworkShortcuts = 19,
		/// <summary>A virtual folder that contains fonts. Added in the .NET Framework 4. </summary>
		Fonts = 20,
		/// <summary>The file system directory that contains the programs and folders that appear on the Start menu for all users. This special folder is valid only for Windows NT systems. Added in the .NET Framework 4. </summary>
		CommonStartMenu = 22,
		/// <summary>A folder for components that are shared across applications. This special folder is valid only for Windows NT, Windows 2000, and Windows XP systems. Added in the .NET Framework 4. </summary>
		CommonPrograms = 23,
		/// <summary>The file system directory that contains the programs that appear in the Startup folder for all users. This special folder is valid only for Windows NT systems. Added in the .NET Framework 4.</summary>
		CommonStartup = 24,
		/// <summary>The file system directory that contains files and folders that appear on the desktop for all users. This special folder is valid only for Windows NT systems. Added in the .NET Framework 4. </summary>
		CommonDesktopDirectory = 25,
		/// <summary>The file system directory that contains the link objects that can exist in the Printers virtual folder. Added in the .NET Framework 4. </summary>
		PrinterShortcuts = 27,
		/// <summary>The Windows directory or SYSROOT. This corresponds to the %windir% or %SYSTEMROOT% environment variables. Added in the .NET Framework 4. </summary>
		Windows = 36,
		/// <summary>The user's profile folder. Applications should not create files or folders at this level; they should put their data under the locations referred to by <see cref="F:System.Environment.SpecialFolder.ApplicationData" />. Added in the .NET Framework 4. </summary>
		UserProfile = 40,
		/// <summary>The Windows System folder. Added in the .NET Framework 4. </summary>
		SystemX86 = 41,
		/// <summary>The Program Files folder. Added in the .NET Framework 4. On an x86 system, passing the <see cref="F:System.Environment.SpecialFolder.ProgramFilesX86" /> member to the <see cref="M:System.Environment.GetFolderPath(System.Environment.SpecialFolder)" /> method returns <see cref="F:System.String.Empty" />; use the <see cref="F:System.Environment.SpecialFolder.ProgramFiles" /> member instead. You can determine whether Windows is a 32-bit operating system by calling the <see cref="P:System.Environment.Is64BitOperatingSystem" /> property. </summary>
		ProgramFilesX86 = 42,
		/// <summary>The Program Files folder. Added in the .NET Framework 4. </summary>
		CommonProgramFilesX86 = 44,
		/// <summary>The file system directory that contains the templates that are available to all users. This special folder is valid only for Windows NT systems.  Added in the .NET Framework 4. </summary>
		CommonTemplates = 45,
		/// <summary>The file system directory that contains documents that are common to all users. This special folder is valid for Windows NT systems, Windows 95, and Windows 98 systems with Shfolder.dll installed. Added in the .NET Framework 4. </summary>
		CommonDocuments = 46,
		/// <summary>The file system directory that contains administrative tools for all users of the computer. Added in the .NET Framework 4. </summary>
		CommonAdminTools = 47,
		/// <summary>The file system directory that is used to store administrative tools for an individual user. The Microsoft Management Console (MMC) will save customized consoles to this directory, and it will roam with the user. Added in the .NET Framework 4.</summary>
		AdminTools = 48,
		/// <summary>The file system directory that serves as a repository for music files common to all users. Added in the .NET Framework 4. </summary>
		CommonMusic = 53,
		/// <summary>The file system directory that serves as a repository for image files common to all users. Added in the .NET Framework 4. </summary>
		CommonPictures = 54,
		/// <summary>The file system directory that serves as a repository for video files common to all users. Added in the .NET Framework 4. </summary>
		CommonVideos = 55,
		/// <summary>The file system directory that contains resource data. Added in the .NET Framework 4. </summary>
		Resources = 56,
		/// <summary>The file system directory that contains localized resource data. Added in the .NET Framework 4. </summary>
		LocalizedResources = 57,
		/// <summary>This value is recognized in Windows Vista for backward compatibility, but the special folder itself is no longer used. Added in the .NET Framework 4. </summary>
		CommonOemLinks = 58,
		/// <summary>The file system directory that acts as a staging area for files waiting to be written to a CD. Added in the .NET Framework 4. </summary>
		CDBurning = 59
	}

	/// <summary>Specifies options to use for getting the path to a special folder. </summary>
	public enum SpecialFolderOption
	{
		/// <summary>The path to the folder is verified. If the folder exists, the path is returned. If the folder does not exist, an empty string is returned. This is the default behavior. </summary>
		None = 0,
		/// <summary>The path to the folder is returned without verifying whether the path exists. If the folder is located on a network, specifying this option can reduce lag time. </summary>
		DoNotVerify = 0x4000,
		/// <summary>The path to the folder is created if it does not already exist.</summary>
		Create = 0x8000
	}

	private const int mono_corlib_version = 1051100001;

	private static string nl;

	private static OperatingSystem os;

	internal static bool IsWindows8OrAbove => false;

	/// <summary>Gets the command line for this process.</summary>
	/// <returns>A string containing command-line arguments.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="Path" />
	/// </PermissionSet>
	public static string CommandLine
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] commandLineArgs = GetCommandLineArgs();
			foreach (string obj in commandLineArgs)
			{
				bool flag = false;
				string text = "";
				string text2 = obj;
				for (int j = 0; j < text2.Length; j++)
				{
					if (text.Length == 0 && char.IsWhiteSpace(text2[j]))
					{
						text = "\"";
					}
					else if (text2[j] == '"')
					{
						flag = true;
					}
				}
				if (flag && text.Length != 0)
				{
					text2 = text2.Replace("\"", "\\\"");
				}
				stringBuilder.AppendFormat("{0}{1}{0} ", text, text2);
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Length--;
			}
			return stringBuilder.ToString();
		}
	}

	/// <summary>Gets or sets the fully qualified path of the current working directory.</summary>
	/// <returns>A string containing a directory path.</returns>
	/// <exception cref="T:System.ArgumentException">Attempted to set to an empty string ("").</exception>
	/// <exception cref="T:System.ArgumentNullException">Attempted to set to null.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">Attempted to set a local path that cannot be found.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the appropriate permission.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static string CurrentDirectory
	{
		get
		{
			return Directory.GetCurrentDirectory();
		}
		set
		{
			Directory.SetCurrentDirectory(value);
		}
	}

	/// <summary>Gets a unique identifier for the current managed thread.</summary>
	/// <returns>An integer that represents a unique identifier for this managed thread.</returns>
	public static int CurrentManagedThreadId => Thread.CurrentThread.ManagedThreadId;

	/// <summary>Gets or sets the exit code of the process.</summary>
	/// <returns>A 32-bit signed integer containing the exit code. The default value is 0 (zero), which indicates that the process completed successfully.</returns>
	/// <filterpriority>1</filterpriority>
	public static extern int ExitCode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	/// <summary>Gets a value indicating whether the common language runtime (CLR) is shutting down.</summary>
	/// <returns>true if the CLR is shutting down; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public static extern bool HasShutdownStarted
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	/// <summary>Gets the NetBIOS name of this local computer.</summary>
	/// <returns>A string containing the name of this computer.</returns>
	/// <exception cref="T:System.InvalidOperationException">The name of this computer cannot be obtained.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="COMPUTERNAME" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static extern string MachineName
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[EnvironmentPermission(SecurityAction.Demand, Read = "COMPUTERNAME")]
		[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
		get;
	}

	/// <summary>Gets the newline string defined for this environment.</summary>
	/// <returns>A string containing "\r\n" for non-Unix platforms, or a string containing "\n" for Unix platforms.</returns>
	/// <filterpriority>1</filterpriority>
	public static string NewLine
	{
		get
		{
			if (nl != null)
			{
				return nl;
			}
			nl = GetNewLine();
			return nl;
		}
	}

	internal static extern PlatformID Platform
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[CompilerGenerated]
		get;
	}

	/// <summary>Gets an <see cref="T:System.OperatingSystem" /> object that contains the current platform identifier and version number.</summary>
	/// <returns>An object that contains the platform identifier and version number.</returns>
	/// <exception cref="T:System.InvalidOperationException">This property was unable to obtain the system version.-or- The obtained platform identifier is not a member of <see cref="T:System.PlatformID" /></exception>
	/// <filterpriority>1</filterpriority>
	public static OperatingSystem OSVersion
	{
		get
		{
			if (os == null)
			{
				Version version = CreateVersionFromString(GetOSVersionString());
				PlatformID platformID = Platform;
				if (platformID == PlatformID.MacOSX)
				{
					platformID = PlatformID.Unix;
				}
				os = new OperatingSystem(platformID, version);
			}
			return os;
		}
	}

	/// <summary>Gets current stack trace information.</summary>
	/// <returns>A string containing stack trace information. This value can be <see cref="F:System.String.Empty" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The requested stack trace information is out of range.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public static string StackTrace
	{
		[EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
		get
		{
			return new StackTrace(0, fNeedFileInfo: true).ToString();
		}
	}

	/// <summary>Gets the fully qualified path of the system directory.</summary>
	/// <returns>A string containing a directory path.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string SystemDirectory => GetFolderPath(SpecialFolder.System);

	/// <summary>Gets the number of milliseconds elapsed since the system started.</summary>
	/// <returns>A 32-bit signed integer containing the amount of time in milliseconds that has passed since the last time the computer was started.</returns>
	/// <filterpriority>1</filterpriority>
	public static extern int TickCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	/// <summary>Gets the network domain name associated with the current user.</summary>
	/// <returns>The network domain name associated with the current user.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system does not support retrieving the network domain name.</exception>
	/// <exception cref="T:System.InvalidOperationException">The network domain name cannot be retrieved.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="UserName;UserDomainName" />
	/// </PermissionSet>
	public static string UserDomainName
	{
		[EnvironmentPermission(SecurityAction.Demand, Read = "USERDOMAINNAME")]
		get
		{
			return MachineName;
		}
	}

	/// <summary>Gets a value indicating whether the current process is running in user interactive mode.</summary>
	/// <returns>true if the current process is running in user interactive mode; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	[MonoTODO("Currently always returns false, regardless of interactive state")]
	public static bool UserInteractive => false;

	/// <summary>Gets the user name of the person who is currently logged on to the Windows operating system.</summary>
	/// <returns>The user name of the person who is logged on to Windows.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="UserName" />
	/// </PermissionSet>
	public static extern string UserName
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[EnvironmentPermission(SecurityAction.Demand, Read = "USERNAME;USER")]
		get;
	}

	/// <summary>Gets a <see cref="T:System.Version" /> object that describes the major, minor, build, and revision numbers of the common language runtime.</summary>
	/// <returns>An object that displays the version of the common language runtime.</returns>
	/// <filterpriority>1</filterpriority>
	public static Version Version => new Version("4.0.30319.42000");

	/// <summary>Gets the amount of physical memory mapped to the process context.</summary>
	/// <returns>A 64-bit signed integer containing the number of bytes of physical memory mapped to the process context.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonoTODO("Currently always returns zero")]
	public static long WorkingSet
	{
		[EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
		get
		{
			return 0L;
		}
	}

	/// <summary>Determines whether the current operating system is a 64-bit operating system.</summary>
	/// <returns>true if the operating system is 64-bit; otherwise, false.</returns>
	public static bool Is64BitOperatingSystem => GetIs64BitOperatingSystem();

	/// <summary>Gets the number of bytes in the operating system's memory page.</summary>
	/// <returns>The number of bytes in the system memory page.</returns>
	public static int SystemPageSize => GetPageSize();

	/// <summary>Determines whether the current process is a 64-bit process.</summary>
	/// <returns>true if the process is 64-bit; otherwise, false.</returns>
	public static bool Is64BitProcess => IntPtr.Size == 8;

	/// <summary>Gets the number of processors on the current machine.</summary>
	/// <returns>The 32-bit signed integer that specifies the number of processors on the current machine. There is no default. If the current machine contains multiple processor groups, this property returns the number of logical processors that are available for use by the common language runtime (CLR).</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="NUMBER_OF_PROCESSORS" />
	/// </PermissionSet>
	public static extern int ProcessorCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[EnvironmentPermission(SecurityAction.Demand, Read = "NUMBER_OF_PROCESSORS")]
		get;
	}

	internal static bool IsRunningOnWindows => Platform < PlatformID.Unix;

	private static string GacPath
	{
		get
		{
			if (IsRunningOnWindows)
			{
				return Path.Combine(Path.Combine(new DirectoryInfo(Path.GetDirectoryName(typeof(int).Assembly.Location)).Parent.Parent.FullName, "mono"), "gac");
			}
			return Path.Combine(Path.Combine(internalGetGacPath(), "mono"), "gac");
		}
	}

	internal static bool IsUnix
	{
		get
		{
			int platform = (int)Platform;
			if (platform != 4 && platform != 128)
			{
				return platform == 6;
			}
			return true;
		}
	}

	internal static bool IsMacOS => Platform == PlatformID.MacOSX;

	internal static bool IsCLRHosted => false;

	internal static bool IsWinRTSupported => true;

	internal static string GetResourceString(string key)
	{
		return key;
	}

	internal static string GetResourceString(string key, CultureInfo culture)
	{
		return key;
	}

	internal static string GetResourceString(string key, params object[] values)
	{
		return string.Format(CultureInfo.InvariantCulture, key, values);
	}

	internal static string GetRuntimeResourceString(string key)
	{
		return key;
	}

	internal static string GetRuntimeResourceString(string key, params object[] values)
	{
		return string.Format(CultureInfo.InvariantCulture, key, values);
	}

	internal static string GetResourceStringEncodingName(int codePage)
	{
		return codePage switch
		{
			1200 => GetResourceString("Unicode"), 
			1201 => GetResourceString("Unicode (Big-Endian)"), 
			12000 => GetResourceString("Unicode (UTF-32)"), 
			12001 => GetResourceString("Unicode (UTF-32 Big-Endian)"), 
			20127 => GetResourceString("US-ASCII"), 
			65000 => GetResourceString("Unicode (UTF-7)"), 
			65001 => GetResourceString("Unicode (UTF-8)"), 
			_ => codePage.ToString(CultureInfo.InvariantCulture), 
		};
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string GetNewLine();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string GetOSVersionString();

	internal static Version CreateVersionFromString(string info)
	{
		int major = 0;
		int minor = 0;
		int build = 0;
		int revision = 0;
		int num = 1;
		int num2 = -1;
		if (info == null)
		{
			return new Version(0, 0, 0, 0);
		}
		foreach (char c in info)
		{
			if (char.IsDigit(c))
			{
				num2 = ((num2 >= 0) ? (num2 * 10 + (c - 48)) : (c - 48));
			}
			else if (num2 >= 0)
			{
				switch (num)
				{
				case 1:
					major = num2;
					break;
				case 2:
					minor = num2;
					break;
				case 3:
					build = num2;
					break;
				case 4:
					revision = num2;
					break;
				}
				num2 = -1;
				num++;
			}
			if (num == 5)
			{
				break;
			}
		}
		if (num2 >= 0)
		{
			switch (num)
			{
			case 1:
				major = num2;
				break;
			case 2:
				minor = num2;
				break;
			case 3:
				build = num2;
				break;
			case 4:
				revision = num2;
				break;
			}
		}
		return new Version(major, minor, build, revision);
	}

	/// <summary>Terminates this process and gives the underlying operating system the specified exit code.</summary>
	/// <param name="exitCode">Exit code to be given to the operating system. Use 0 (zero) to indicate that the process completed successfully.</param>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have sufficient security permission to perform this function.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public static extern void Exit(int exitCode);

	internal static void _Exit(int exitCode)
	{
		Exit(exitCode);
	}

	/// <summary>Replaces the name of each environment variable embedded in the specified string with the string equivalent of the value of the variable, then returns the resulting string.</summary>
	/// <returns>A string with each environment variable replaced by its value.</returns>
	/// <param name="name">A string containing the names of zero or more environment variables. Each environment variable is quoted with the percent sign character (%).</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string ExpandEnvironmentVariables(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		int num = name.IndexOf('%');
		if (num == -1)
		{
			return name;
		}
		int length = name.Length;
		int num2 = 0;
		if (num == length - 1 || (num2 = name.IndexOf('%', num + 1)) == -1)
		{
			return name;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(name, 0, num);
		Hashtable hashtable = null;
		do
		{
			string text = name.Substring(num + 1, num2 - num - 1);
			string text2 = GetEnvironmentVariable(text);
			if (text2 == null && IsRunningOnWindows)
			{
				if (hashtable == null)
				{
					hashtable = GetEnvironmentVariablesNoCase();
				}
				text2 = hashtable[text] as string;
			}
			int num3 = num2;
			if (text2 == null)
			{
				stringBuilder.Append('%');
				stringBuilder.Append(text);
				num2--;
			}
			else
			{
				stringBuilder.Append(text2);
			}
			int num4 = num2;
			num = name.IndexOf('%', num2 + 1);
			num2 = ((num == -1 || num2 > length - 1) ? (-1) : name.IndexOf('%', num + 1));
			int count = ((num == -1 || num2 == -1) ? (length - num4 - 1) : ((text2 == null) ? (num - num3) : (num - num4 - 1)));
			if (num >= num4 || num == -1)
			{
				stringBuilder.Append(name, num4 + 1, count);
			}
		}
		while (num2 > -1 && num2 < length);
		return stringBuilder.ToString();
	}

	/// <summary>Returns a string array containing the command-line arguments for the current process.</summary>
	/// <returns>An array of string where each element contains a command-line argument. The first element is the executable file name, and the following zero or more elements contain the remaining command-line arguments.</returns>
	/// <exception cref="T:System.NotSupportedException">The system does not support command-line arguments.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="Path" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[EnvironmentPermission(SecurityAction.Demand, Read = "PATH")]
	public static extern string[] GetCommandLineArgs();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string internalGetEnvironmentVariable_native(IntPtr variable);

	internal static string internalGetEnvironmentVariable(string variable)
	{
		if (variable == null)
		{
			return null;
		}
		using SafeStringMarshal safeStringMarshal = RuntimeMarshal.MarshalString(variable);
		return internalGetEnvironmentVariable_native(safeStringMarshal.Value);
	}

	/// <summary>Retrieves the value of an environment variable from the current process.</summary>
	/// <returns>The value of the environment variable specified by <paramref name="variable" />, or null if the environment variable is not found.</returns>
	/// <param name="variable">The name of the environment variable.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="variable" /> is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission to perform this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string GetEnvironmentVariable(string variable)
	{
		return internalGetEnvironmentVariable(variable);
	}

	private static Hashtable GetEnvironmentVariablesNoCase()
	{
		Hashtable hashtable = new Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
		string[] environmentVariableNames = GetEnvironmentVariableNames();
		foreach (string text in environmentVariableNames)
		{
			hashtable[text] = internalGetEnvironmentVariable(text);
		}
		return hashtable;
	}

	/// <summary>Retrieves all environment variable names and their values from the current process.</summary>
	/// <returns>A dictionary that contains all environment variable names and their values; otherwise, an empty dictionary if no environment variables are found.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission to perform this operation.</exception>
	/// <exception cref="T:System.OutOfMemoryException">The buffer is out of memory.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static IDictionary GetEnvironmentVariables()
	{
		StringBuilder stringBuilder = null;
		if (SecurityManager.SecurityEnabled)
		{
			stringBuilder = new StringBuilder();
		}
		Hashtable hashtable = new Hashtable();
		string[] environmentVariableNames = GetEnvironmentVariableNames();
		foreach (string text in environmentVariableNames)
		{
			hashtable[text] = internalGetEnvironmentVariable(text);
			if (stringBuilder != null)
			{
				stringBuilder.Append(text);
				stringBuilder.Append(";");
			}
		}
		if (stringBuilder != null)
		{
			new EnvironmentPermission(EnvironmentPermissionAccess.Read, stringBuilder.ToString()).Demand();
		}
		return hashtable;
	}

	/// <summary>Gets the path to the system special folder that is identified by the specified enumeration.</summary>
	/// <returns>The path to the specified system special folder, if that folder physically exists on your computer; otherwise, an empty string ("").A folder will not physically exist if the operating system did not create it, the existing folder was deleted, or the folder is a virtual directory, such as My Computer, which does not correspond to a physical path.</returns>
	/// <param name="folder">An enumerated constant that identifies a system special folder.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="folder" /> is not a member of <see cref="T:System.Environment.SpecialFolder" />.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current platform is not supported.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string GetFolderPath(SpecialFolder folder)
	{
		return GetFolderPath(folder, SpecialFolderOption.None);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string GetWindowsFolderPath(int folder);

	/// <summary>Gets the path to the system special folder that is identified by the specified enumeration, and uses a specified option for accessing special folders.</summary>
	/// <returns>The path to the specified system special folder, if that folder physically exists on your computer; otherwise, an empty string ("").A folder will not physically exist if the operating system did not create it, the existing folder was deleted, or the folder is a virtual directory, such as My Computer, which does not correspond to a physical path.</returns>
	/// <param name="folder">An enumerated constant that identifies a system special folder.</param>
	/// <param name="option">Specifies options to use for accessing a special folder.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="folder" /> is not a member of <see cref="T:System.Environment.SpecialFolder" /></exception>
	/// <exception cref="T:System.PlatformNotSupportedException">
	///   <see cref="T:System.PlatformNotSupportedException" />
	/// </exception>
	public static string GetFolderPath(SpecialFolder folder, SpecialFolderOption option)
	{
		string text = null;
		if (IsRunningOnWindows)
		{
			return GetWindowsFolderPath((int)folder);
		}
		return UnixGetFolderPath(folder, option);
	}

	private static string ReadXdgUserDir(string config_dir, string home_dir, string key, string fallback)
	{
		string text = internalGetEnvironmentVariable(key);
		if (text != null && text != string.Empty)
		{
			return text;
		}
		string path = Path.Combine(config_dir, "user-dirs.dirs");
		if (!File.Exists(path))
		{
			return Path.Combine(home_dir, fallback);
		}
		try
		{
			using StreamReader streamReader = new StreamReader(path);
			string text2;
			while ((text2 = streamReader.ReadLine()) != null)
			{
				text2 = text2.Trim();
				int num = text2.IndexOf('=');
				if (num > 8 && text2.Substring(0, num) == key)
				{
					string text3 = text2.Substring(num + 1).Trim('"');
					bool flag = false;
					if (text3.StartsWithOrdinalUnchecked("$HOME/"))
					{
						flag = true;
						text3 = text3.Substring(6);
					}
					else if (!text3.StartsWithOrdinalUnchecked("/"))
					{
						flag = true;
					}
					return flag ? Path.Combine(home_dir, text3) : text3;
				}
			}
		}
		catch (FileNotFoundException)
		{
		}
		return Path.Combine(home_dir, fallback);
	}

	internal static string UnixGetFolderPath(SpecialFolder folder, SpecialFolderOption option)
	{
		string text = internalGetHome();
		string text2 = internalGetEnvironmentVariable("XDG_DATA_HOME");
		if (text2 == null || text2 == string.Empty)
		{
			text2 = Path.Combine(text, ".local");
			text2 = Path.Combine(text2, "share");
		}
		string text3 = internalGetEnvironmentVariable("XDG_CONFIG_HOME");
		if (text3 == null || text3 == string.Empty)
		{
			text3 = Path.Combine(text, ".config");
		}
		switch (folder)
		{
		case SpecialFolder.MyComputer:
			return string.Empty;
		case SpecialFolder.MyDocuments:
			return text;
		case SpecialFolder.ApplicationData:
			return text3;
		case SpecialFolder.LocalApplicationData:
			return text2;
		case SpecialFolder.Desktop:
		case SpecialFolder.DesktopDirectory:
			return ReadXdgUserDir(text3, text, "XDG_DESKTOP_DIR", "Desktop");
		case SpecialFolder.MyMusic:
			if (Platform == PlatformID.MacOSX)
			{
				return Path.Combine(text, "Music");
			}
			return ReadXdgUserDir(text3, text, "XDG_MUSIC_DIR", "Music");
		case SpecialFolder.MyPictures:
			if (Platform == PlatformID.MacOSX)
			{
				return Path.Combine(text, "Pictures");
			}
			return ReadXdgUserDir(text3, text, "XDG_PICTURES_DIR", "Pictures");
		case SpecialFolder.Templates:
			return ReadXdgUserDir(text3, text, "XDG_TEMPLATES_DIR", "Templates");
		case SpecialFolder.MyVideos:
			return ReadXdgUserDir(text3, text, "XDG_VIDEOS_DIR", "Videos");
		case SpecialFolder.CommonTemplates:
			return "/usr/share/templates";
		case SpecialFolder.Fonts:
			if (Platform == PlatformID.MacOSX)
			{
				return Path.Combine(text, "Library", "Fonts");
			}
			return Path.Combine(text, ".fonts");
		case SpecialFolder.Favorites:
			if (Platform == PlatformID.MacOSX)
			{
				return Path.Combine(text, "Library", "Favorites");
			}
			return string.Empty;
		case SpecialFolder.ProgramFiles:
			if (Platform == PlatformID.MacOSX)
			{
				return "/Applications";
			}
			return string.Empty;
		case SpecialFolder.InternetCache:
			if (Platform == PlatformID.MacOSX)
			{
				return Path.Combine(text, "Library", "Caches");
			}
			return string.Empty;
		case SpecialFolder.UserProfile:
			return text;
		case SpecialFolder.Programs:
		case SpecialFolder.Startup:
		case SpecialFolder.Recent:
		case SpecialFolder.SendTo:
		case SpecialFolder.StartMenu:
		case SpecialFolder.NetworkShortcuts:
		case SpecialFolder.CommonStartMenu:
		case SpecialFolder.CommonPrograms:
		case SpecialFolder.CommonStartup:
		case SpecialFolder.CommonDesktopDirectory:
		case SpecialFolder.PrinterShortcuts:
		case SpecialFolder.Cookies:
		case SpecialFolder.History:
		case SpecialFolder.Windows:
		case SpecialFolder.System:
		case SpecialFolder.SystemX86:
		case SpecialFolder.ProgramFilesX86:
		case SpecialFolder.CommonProgramFiles:
		case SpecialFolder.CommonProgramFilesX86:
		case SpecialFolder.CommonDocuments:
		case SpecialFolder.CommonAdminTools:
		case SpecialFolder.AdminTools:
		case SpecialFolder.CommonMusic:
		case SpecialFolder.CommonPictures:
		case SpecialFolder.CommonVideos:
		case SpecialFolder.Resources:
		case SpecialFolder.LocalizedResources:
		case SpecialFolder.CommonOemLinks:
		case SpecialFolder.CDBurning:
			return string.Empty;
		case SpecialFolder.CommonApplicationData:
			return "/usr/share";
		default:
			throw new ArgumentException("Invalid SpecialFolder");
		}
	}

	/// <summary>Returns an array of string containing the names of the logical drives on the current computer.</summary>
	/// <returns>An array of strings where each element contains the name of a logical drive. For example, if the computer's hard drive is the first logical drive, the first element returned is "C:\".</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permissions.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
	public static string[] GetLogicalDrives()
	{
		return GetLogicalDrivesInternal();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void internalBroadcastSettingChange();

	/// <summary>Retrieves the value of an environment variable from the current process or from the Windows operating system registry key for the current user or local machine.</summary>
	/// <returns>The value of the environment variable specified by the <paramref name="variable" /> and <paramref name="target" /> parameters, or null if the environment variable is not found.</returns>
	/// <param name="variable">The name of an environment variable.</param>
	/// <param name="target">One of the <see cref="T:System.EnvironmentVariableTarget" /> values.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="variable" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not a valid <see cref="T:System.EnvironmentVariableTarget" /> value.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission to perform this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target)
	{
		switch (target)
		{
		case EnvironmentVariableTarget.Process:
			return GetEnvironmentVariable(variable);
		case EnvironmentVariableTarget.Machine:
		{
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (!IsRunningOnWindows)
			{
				return null;
			}
			using RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment");
			return registryKey2.GetValue(variable)?.ToString();
		}
		case EnvironmentVariableTarget.User:
		{
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (!IsRunningOnWindows)
			{
				return null;
			}
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Environment", writable: false);
			return registryKey.GetValue(variable)?.ToString();
		}
		default:
			throw new ArgumentException("target");
		}
	}

	/// <summary>Retrieves all environment variable names and their values from the current process, or from the Windows operating system registry key for the current user or local machine.</summary>
	/// <returns>A dictionary that contains all environment variable names and their values from the source specified by the <paramref name="target" /> parameter; otherwise, an empty dictionary if no environment variables are found.</returns>
	/// <param name="target">One of the <see cref="T:System.EnvironmentVariableTarget" /> values.</param>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission to perform this operation for the specified value of <paramref name="target" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> contains an illegal value.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static IDictionary GetEnvironmentVariables(EnvironmentVariableTarget target)
	{
		IDictionary dictionary = new Hashtable();
		switch (target)
		{
		case EnvironmentVariableTarget.Process:
			dictionary = GetEnvironmentVariables();
			break;
		case EnvironmentVariableTarget.Machine:
		{
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (!IsRunningOnWindows)
			{
				break;
			}
			using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment"))
			{
				string[] valueNames = registryKey2.GetValueNames();
				foreach (string text2 in valueNames)
				{
					dictionary.Add(text2, registryKey2.GetValue(text2));
				}
			}
			break;
		}
		case EnvironmentVariableTarget.User:
		{
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			if (!IsRunningOnWindows)
			{
				break;
			}
			using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Environment"))
			{
				string[] valueNames = registryKey.GetValueNames();
				foreach (string text in valueNames)
				{
					dictionary.Add(text, registryKey.GetValue(text));
				}
			}
			break;
		}
		default:
			throw new ArgumentException("target");
		}
		return dictionary;
	}

	/// <summary>Creates, modifies, or deletes an environment variable stored in the current process.</summary>
	/// <param name="variable">The name of an environment variable.</param>
	/// <param name="value">A value to assign to <paramref name="variable" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="variable" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="variable" /> contains a zero-length string, an initial hexadecimal zero character (0x00), or an equal sign ("="). -or-The length of <paramref name="variable" /> or <paramref name="value" /> is greater than or equal to 32,767 characters.-or-An error occurred during the execution of this operation.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission to perform this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
	public static void SetEnvironmentVariable(string variable, string value)
	{
		SetEnvironmentVariable(variable, value, EnvironmentVariableTarget.Process);
	}

	/// <summary>Creates, modifies, or deletes an environment variable stored in the current process or in the Windows operating system registry key reserved for the current user or local machine.</summary>
	/// <param name="variable">The name of an environment variable.</param>
	/// <param name="value">A value to assign to <paramref name="variable" />.</param>
	/// <param name="target">One of the <see cref="T:System.EnvironmentVariableTarget" /> values.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="variable" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="variable" /> contains a zero-length string, an initial hexadecimal zero character (0x00), or an equal sign ("="). -or-The length of <paramref name="variable" /> is greater than or equal to 32,767 characters.-or-<paramref name="target" /> is not a member of the <see cref="T:System.EnvironmentVariableTarget" /> enumeration. -or-<paramref name="target" /> is <see cref="F:System.EnvironmentVariableTarget.Machine" /> or <see cref="F:System.EnvironmentVariableTarget.User" /> and the length of <paramref name="variable" /> is greater than or equal to 255.-or-<paramref name="target" /> is <see cref="F:System.EnvironmentVariableTarget.Process" /> and the length of <paramref name="value" /> is greater than or equal to 32,767 characters. -or-An error occurred during the execution of this operation.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission to perform this operation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
	public static void SetEnvironmentVariable(string variable, string value, EnvironmentVariableTarget target)
	{
		if (variable == null)
		{
			throw new ArgumentNullException("variable");
		}
		if (variable == string.Empty)
		{
			throw new ArgumentException("String cannot be of zero length.", "variable");
		}
		if (variable.IndexOf('=') != -1)
		{
			throw new ArgumentException("Environment variable name cannot contain an equal character.", "variable");
		}
		if (variable[0] == '\0')
		{
			throw new ArgumentException("The first char in the string is the null character.", "variable");
		}
		switch (target)
		{
		case EnvironmentVariableTarget.Process:
			InternalSetEnvironmentVariable(variable, value);
			break;
		case EnvironmentVariableTarget.Machine:
		{
			if (!IsRunningOnWindows)
			{
				break;
			}
			using RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", writable: true);
			if (string.IsNullOrEmpty(value))
			{
				registryKey2.DeleteValue(variable, throwOnMissingValue: false);
			}
			else
			{
				registryKey2.SetValue(variable, value);
			}
			internalBroadcastSettingChange();
			break;
		}
		case EnvironmentVariableTarget.User:
		{
			if (!IsRunningOnWindows)
			{
				break;
			}
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Environment", writable: true);
			if (string.IsNullOrEmpty(value))
			{
				registryKey.DeleteValue(variable, throwOnMissingValue: false);
			}
			else
			{
				registryKey.SetValue(variable, value);
			}
			internalBroadcastSettingChange();
			break;
		}
		default:
			throw new ArgumentException("target");
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void InternalSetEnvironmentVariable(string variable, string value);

	/// <summary>Immediately terminates a process after writing a message to the Windows Application event log, and then includes the message in error reporting to Microsoft.</summary>
	/// <param name="message">A message that explains why the process was terminated, or null if no explanation is provided.</param>
	[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
	public static void FailFast(string message)
	{
		throw new NotImplementedException();
	}

	internal static void FailFast(string message, uint exitCode)
	{
		throw new NotImplementedException();
	}

	/// <summary>Immediately terminates a process after writing a message to the Windows Application event log, and then includes the message and exception information in error reporting to Microsoft.</summary>
	/// <param name="message">A message that explains why the process was terminated, or null if no explanation is provided.</param>
	/// <param name="exception">An exception that represents the error that caused the termination. This is typically the exception in a catch block.</param>
	[SecurityCritical]
	public static void FailFast(string message, Exception exception)
	{
		throw new ExecutionEngineException(message, exception);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool GetIs64BitOperatingSystem();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string internalGetGacPath();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string[] GetLogicalDrivesInternal();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string[] GetEnvironmentVariableNames();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string GetMachineConfigPath();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string internalGetHome();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern int GetPageSize();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string get_bundled_machine_config();

	internal static string GetBundledMachineConfig()
	{
		return get_bundled_machine_config();
	}

	internal static void TriggerCodeContractFailure(ContractFailureKind failureKind, string message, string condition, string exceptionAsString)
	{
	}

	internal static string GetStackTrace(Exception e, bool needFileInfo)
	{
		StackTrace stackTrace = ((e != null) ? new StackTrace(e, needFileInfo) : new StackTrace(needFileInfo));
		return stackTrace.ToString(System.Diagnostics.StackTrace.TraceFormat.Normal);
	}
}
