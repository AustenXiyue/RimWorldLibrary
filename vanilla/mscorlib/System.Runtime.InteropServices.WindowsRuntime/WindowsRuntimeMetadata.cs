using System.Collections.Generic;

namespace System.Runtime.InteropServices.WindowsRuntime;

/// <summary>Provides an event for resolving reflection-only type requests for types that are provided by Windows Metadata files, and methods for performing the resolution. </summary>
[MonoTODO]
public static class WindowsRuntimeMetadata
{
	/// <summary>Occurs when the resolution of a Windows Metadata file fails in the design environment. </summary>
	public static event EventHandler<DesignerNamespaceResolveEventArgs> DesignerNamespaceResolve;

	/// <summary>Occurs when the resolution of a Windows Metadata file fails in the reflection-only context. </summary>
	public static event EventHandler<NamespaceResolveEventArgs> ReflectionOnlyNamespaceResolve;

	/// <summary>Locates the Windows Metadata files for the specified namespace, given the specified locations to search. </summary>
	/// <returns>An enumerable list of strings that represent the Windows Metadata files that define <paramref name="namespaceName" />. </returns>
	/// <param name="namespaceName">The namespace to resolve. </param>
	/// <param name="packageGraphFilePaths">The application paths to search for Windows Metadata files, or null to search only for Windows Metadata files from the operating system installation. </param>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system version does not support the Windows Runtime. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="namespaceName" /> is null.</exception>
	public static IEnumerable<string> ResolveNamespace(string namespaceName, IEnumerable<string> packageGraphFilePaths)
	{
		throw new NotImplementedException();
	}

	/// <summary>Locates the Windows Metadata files for the specified namespace, given the specified locations to search. </summary>
	/// <returns>An enumerable list of strings that represent the Windows Metadata files that define <paramref name="namespaceName" />. </returns>
	/// <param name="namespaceName">The namespace to resolve. </param>
	/// <param name="windowsSdkFilePath">The path to search for Windows Metadata files provided by the SDK, or null to search for Windows Metadata files from the operating system installation. </param>
	/// <param name="packageGraphFilePaths">The application paths to search for Windows Metadata files. </param>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system version does not support the Windows Runtime. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="namespaceName" /> is null.</exception>
	public static IEnumerable<string> ResolveNamespace(string namespaceName, string windowsSdkFilePath, IEnumerable<string> packageGraphFilePaths)
	{
		throw new NotImplementedException();
	}
}
