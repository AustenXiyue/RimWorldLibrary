using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Runtime.InteropServices;

/// <summary>Provides a collection of static methods that return information about the common language runtime environment.</summary>
[ComVisible(true)]
public class RuntimeEnvironment
{
	/// <summary>Gets the path to the system configuration file.</summary>
	/// <returns>The path to the system configuration file.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string SystemConfigurationFile
	{
		[SecuritySafeCritical]
		get
		{
			return Environment.GetMachineConfigPath();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.RuntimeEnvironment" /> class.</summary>
	[Obsolete("Do not create instances of the RuntimeEnvironment class.  Call the static methods directly on this type instead", true)]
	public RuntimeEnvironment()
	{
	}

	/// <summary>Tests whether the specified assembly is loaded in the global assembly cache.</summary>
	/// <returns>true if the assembly is loaded in the global assembly cache; otherwise, false.</returns>
	/// <param name="a">The assembly to test. </param>
	public static bool FromGlobalAccessCache(Assembly a)
	{
		return a.GlobalAssemblyCache;
	}

	/// <summary>Gets the version number of the common language runtime that is running the current process.</summary>
	/// <returns>A string containing the version number of the common language runtime.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static string GetSystemVersion()
	{
		return Assembly.GetExecutingAssembly().ImageRuntimeVersion;
	}

	/// <summary>Returns the directory where the common language runtime is installed.</summary>
	/// <returns>A string that contains the path to the directory where the common language runtime is installed.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public static string GetRuntimeDirectory()
	{
		if (Environment.GetEnvironmentVariable("CSC_SDK_PATH_DISABLED") != null)
		{
			return null;
		}
		return GetRuntimeDirectoryImpl();
	}

	private static string GetRuntimeDirectoryImpl()
	{
		return Path.GetDirectoryName(typeof(object).Assembly.Location);
	}

	private static IntPtr GetRuntimeInterfaceImpl(Guid clsid, Guid riid)
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns the specified interface on the specified class.</summary>
	/// <returns>An unmanaged pointer to the requested interface.</returns>
	/// <param name="clsid">The identifier for the desired class.</param>
	/// <param name="riid">The identifier for the desired interface.</param>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">IUnknown::QueryInterface failure.</exception>
	[SecurityCritical]
	[ComVisible(false)]
	public static IntPtr GetRuntimeInterfaceAsIntPtr(Guid clsid, Guid riid)
	{
		return GetRuntimeInterfaceImpl(clsid, riid);
	}

	/// <summary>Returns an instance of a type that represents a COM object by a pointer to its IUnknown interface.</summary>
	/// <returns>An object that represents the specified unmanaged COM object.</returns>
	/// <param name="clsid">The identifier for the desired class.</param>
	/// <param name="riid">The identifier for the desired interface.</param>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">IUnknown::QueryInterface failure.</exception>
	[ComVisible(false)]
	[SecurityCritical]
	public static object GetRuntimeInterfaceAsObject(Guid clsid, Guid riid)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = GetRuntimeInterfaceImpl(clsid, riid);
			return Marshal.GetObjectForIUnknown(intPtr);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.Release(intPtr);
			}
		}
	}
}
