using System;

namespace Microsoft.Win32;

/// <summary>Represents an entry in a <see cref="T:Microsoft.Win32.FileDialog" /> custom place list.</summary>
public sealed class FileDialogCustomPlace
{
	/// <summary>Gets the GUID of the known folder for the custom place.</summary>
	/// <returns>The GUID of a known folder.</returns>
	public Guid KnownFolder { get; private set; }

	/// <summary>Gets the file path for the custom place.</summary>
	/// <returns>The path for a custom place.</returns>
	public string Path { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.FileDialogCustomPlace" /> class with the specified known folder GUID. </summary>
	/// <param name="knownFolder">The GUID of a known folder.</param>
	public FileDialogCustomPlace(Guid knownFolder)
	{
		KnownFolder = knownFolder;
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.FileDialogCustomPlace" /> class with the specified path. </summary>
	/// <param name="path">The path for the folder.</param>
	public FileDialogCustomPlace(string path)
	{
		Path = path ?? "";
	}
}
