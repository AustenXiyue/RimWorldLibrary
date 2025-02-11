using System;
using System.IO;
using System.Windows;
using MS.Internal.AppModel;
using MS.Internal.Interop;

namespace Microsoft.Win32;

/// <summary>Represents a common dialog that allows the user to specify a filename to save a file as. <see cref="T:Microsoft.Win32.SaveFileDialog" /> cannot be used by an application that is executing under partial trust.</summary>
public sealed class SaveFileDialog : FileDialog
{
	/// <summary>Gets or sets a value indicating whether <see cref="T:Microsoft.Win32.SaveFileDialog" /> prompts the user for permission to create a file if the user specifies a file that does not exist.</summary>
	/// <returns>true if dialog should prompt prior to saving to a filename that did not previously exist; otherwise, false. The default is false.</returns>
	public bool CreatePrompt { get; set; }

	public bool CreateTestFile
	{
		get
		{
			return !GetOption(FOS.NOTESTFILECREATE);
		}
		set
		{
			SetOption(FOS.NOTESTFILECREATE, !value);
		}
	}

	/// <summary>Gets or sets a value indicating whether <see cref="T:Microsoft.Win32.SaveFileDialog" /> displays a warning if the user specifies the name of a file that already exists.</summary>
	/// <returns>true if dialog should prompt prior to saving over a filename that previously existed; otherwise, false. The default is true.</returns>
	public bool OverwritePrompt { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SaveFileDialog" /> class.</summary>
	public SaveFileDialog()
	{
		Initialize();
	}

	/// <summary>Creates a read-write file stream for the filename selected by the user using <see cref="T:Microsoft.Win32.SaveFileDialog" />.</summary>
	/// <returns>A new <see cref="T:System.IO.Stream" /> that contains the selected file.</returns>
	/// <exception cref="T:System.InvalidOperationException">No files were selected in the dialog.</exception>
	public Stream OpenFile()
	{
		string criticalItemName = base.CriticalItemName;
		if (string.IsNullOrEmpty(criticalItemName))
		{
			throw new InvalidOperationException(SR.FileNameMustNotBeNull);
		}
		return new FileStream(criticalItemName, FileMode.Create, FileAccess.ReadWrite);
	}

	/// <summary>Resets all <see cref="T:Microsoft.Win32.SaveFileDialog" /> properties to their default values.</summary>
	public override void Reset()
	{
		base.Reset();
		Initialize();
	}

	internal override bool PromptUserIfAppropriate(string fileName)
	{
		if (!base.PromptUserIfAppropriate(fileName))
		{
			return false;
		}
		bool flag = File.Exists(fileName);
		if (CreatePrompt && !flag && !PromptFileCreate(fileName))
		{
			return false;
		}
		if (OverwritePrompt && flag && !PromptFileOverwrite(fileName))
		{
			return false;
		}
		return true;
	}

	private protected override IFileDialog CreateDialog()
	{
		return (IFileDialog)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("C0B4E2F3-BA21-4773-8DBA-335EC946EB8B")));
	}

	private void Initialize()
	{
		OverwritePrompt = true;
	}

	private bool PromptFileCreate(string fileName)
	{
		return MessageBoxWithFocusRestore(SR.Format(SR.FileDialogCreatePrompt, fileName), MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
	}

	private bool PromptFileOverwrite(string fileName)
	{
		return MessageBoxWithFocusRestore(SR.Format(SR.FileDialogOverwritePrompt, fileName), MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
	}
}
