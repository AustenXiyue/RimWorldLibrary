using System;
using System.IO;
using System.Windows;
using MS.Internal.AppModel;
using MS.Internal.Interop;

namespace Microsoft.Win32;

/// <summary>Represents a common dialog box that allows a user to specify a filename for one or more files to open.</summary>
public sealed class OpenFileDialog : FileDialog
{
	public bool ForcePreviewPane
	{
		get
		{
			return GetOption(FOS.FORCEPREVIEWPANEON);
		}
		set
		{
			SetOption(FOS.FORCEPREVIEWPANEON, value);
		}
	}

	/// <summary>Gets or sets an option indicating whether <see cref="T:Microsoft.Win32.OpenFileDialog" /> allows users to select multiple files.</summary>
	/// <returns>true if multiple selections are allowed; otherwise, false. The default is false.</returns>
	public bool Multiselect
	{
		get
		{
			return GetOption(FOS.ALLOWMULTISELECT);
		}
		set
		{
			SetOption(FOS.ALLOWMULTISELECT, value);
		}
	}

	/// <summary>Gets or sets a value indicating whether the read-only check box displayed by <see cref="T:Microsoft.Win32.OpenFileDialog" /> is selected.</summary>
	/// <returns>true if the checkbox is selected; otherwise, false. The default is false.</returns>
	public bool ReadOnlyChecked { get; set; }

	/// <summary>Gets or sets a value indicating whether <see cref="T:Microsoft.Win32.OpenFileDialog" /> contains a read-only check box.</summary>
	/// <returns>true if the checkbox is displayed; otherwise, false. The default is false.</returns>
	public bool ShowReadOnly { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.OpenFileDialog" /> class.</summary>
	public OpenFileDialog()
	{
		Initialize();
	}

	/// <summary>Opens a read-only stream for the file that is selected by the user using <see cref="T:Microsoft.Win32.OpenFileDialog" />.</summary>
	/// <returns>A new <see cref="T:System.IO.Stream" /> that contains the selected file.</returns>
	/// <exception cref="T:System.InvalidOperationException">No files were selected in the dialog.</exception>
	public Stream OpenFile()
	{
		string criticalItemName = base.CriticalItemName;
		if (string.IsNullOrEmpty(criticalItemName))
		{
			throw new InvalidOperationException(SR.FileNameMustNotBeNull);
		}
		return new FileStream(criticalItemName, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	/// <summary>Creates an array that contains one read-only stream for each file selected by the user using <see cref="T:Microsoft.Win32.OpenFileDialog" />.</summary>
	/// <returns>An array of multiple new <see cref="T:System.IO.Stream" /> objects that contain the selected files.</returns>
	/// <exception cref="T:System.InvalidOperationException">No files were selected in the dialog.</exception>
	public Stream[] OpenFiles()
	{
		string[] array = CloneItemNames();
		Stream[] array2 = new Stream[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			if (string.IsNullOrEmpty(text))
			{
				throw new InvalidOperationException(SR.FileNameMustNotBeNull);
			}
			array2[i] = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		return array2;
	}

	/// <summary>Resets all <see cref="T:Microsoft.Win32.OpenFileDialog" /> properties to their default values.</summary>
	public override void Reset()
	{
		base.Reset();
		Initialize();
	}

	private protected override IFileDialog CreateDialog()
	{
		return (IFileDialog)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")));
	}

	private void Initialize()
	{
		SetOption(FOS.FILEMUSTEXIST, value: true);
	}
}
