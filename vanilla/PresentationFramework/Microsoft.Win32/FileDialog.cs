using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Interop;

namespace Microsoft.Win32;

/// <summary>An abstract base class that encapsulates functionality that is common to file dialogs, including <see cref="T:Microsoft.Win32.OpenFileDialog" /> and <see cref="T:Microsoft.Win32.SaveFileDialog" />.</summary>
public abstract class FileDialog : CommonItemDialog
{
	private string _defaultExtension;

	private string _filter;

	private int _filterIndex;

	/// <summary>Gets a string that only contains the file name for the selected file.</summary>
	/// <returns>A <see cref="T:System.String" /> that only contains the file name for the selected file. The default is <see cref="F:System.String.Empty" />, which is also the value when either no file is selected or a directory is selected.</returns>
	public string SafeFileName
	{
		get
		{
			string text = Path.GetFileName(base.CriticalItemName);
			if (text == null)
			{
				text = string.Empty;
			}
			return text;
		}
	}

	/// <summary>Gets an array that contains one safe file name for each selected file.</summary>
	/// <returns>An array of <see cref="T:System.String" /> that contains one safe file name for each selected file. The default is an array with a single item whose value is <see cref="F:System.String.Empty" />.</returns>
	public string[] SafeFileNames
	{
		get
		{
			string[] array = CloneItemNames();
			string[] array2 = new string[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Path.GetFileName(array[i]);
				if (array2[i] == null)
				{
					array2[i] = string.Empty;
				}
			}
			return array2;
		}
	}

	/// <summary>Gets or sets a string containing the full path of the file selected in a file dialog.</summary>
	/// <returns>A <see cref="T:System.String" /> that is the full path of the file selected in the file dialog. The default is <see cref="F:System.String.Empty" />.</returns>
	public string FileName
	{
		get
		{
			return base.CriticalItemName;
		}
		set
		{
			if (value == null)
			{
				base.MutableItemNames = null;
				return;
			}
			base.MutableItemNames = new string[1] { value };
		}
	}

	/// <summary>Gets an array that contains one file name for each selected file.</summary>
	/// <returns>An array of <see cref="T:System.String" /> that contains one file name for each selected file. The default is an array with a single item whose value is <see cref="F:System.String.Empty" />.</returns>
	public string[] FileNames => CloneItemNames();

	/// <summary>Gets or sets a value indicating whether a file dialog automatically adds an extension to a file name if the user omits an extension.</summary>
	/// <returns>true if extensions are added; otherwise, false. The default is true.</returns>
	public bool AddExtension { get; set; }

	/// <summary>Gets or sets a value indicating whether a file dialog displays a warning if the user specifies a file name that does not exist.</summary>
	/// <returns>true if warnings are displayed; otherwise, false. The default in this base class is false.</returns>
	public bool CheckFileExists
	{
		get
		{
			return GetOption(FOS.FILEMUSTEXIST);
		}
		set
		{
			SetOption(FOS.FILEMUSTEXIST, value);
		}
	}

	/// <summary>Gets or sets a value that specifies whether warnings are displayed if the user types invalid paths and file names.</summary>
	/// <returns>true if warnings are displayed; otherwise, false. The default is true.</returns>
	public bool CheckPathExists
	{
		get
		{
			return GetOption(FOS.PATHMUSTEXIST);
		}
		set
		{
			SetOption(FOS.PATHMUSTEXIST, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the default extension string to use to filter the list of files that are displayed.</summary>
	/// <returns>The default extension string. The default is <see cref="F:System.String.Empty" />.</returns>
	public string DefaultExt
	{
		get
		{
			if (_defaultExtension != null)
			{
				return _defaultExtension;
			}
			return string.Empty;
		}
		set
		{
			if (value != null)
			{
				if (value.StartsWith(".", StringComparison.Ordinal))
				{
					value = value.Substring(1);
				}
				else if (value.Length == 0)
				{
					value = null;
				}
			}
			_defaultExtension = value;
		}
	}

	/// <summary>Gets or sets the filter string that determines what types of files are displayed from either the <see cref="T:Microsoft.Win32.OpenFileDialog" /> or <see cref="T:Microsoft.Win32.SaveFileDialog" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the filter. The default is <see cref="F:System.String.Empty" />, which means that no filter is applied and all file types are displayed.</returns>
	/// <exception cref="T:System.ArgumentException">The filter string is invalid.</exception>
	public string Filter
	{
		get
		{
			if (_filter != null)
			{
				return _filter;
			}
			return string.Empty;
		}
		set
		{
			if (string.CompareOrdinal(value, _filter) == 0)
			{
				return;
			}
			string text = value;
			if (!string.IsNullOrEmpty(text))
			{
				if (text.Split('|').Length % 2 != 0)
				{
					throw new ArgumentException(SR.FileDialogInvalidFilter);
				}
			}
			else
			{
				text = null;
			}
			_filter = text;
		}
	}

	/// <summary>Gets or sets the index of the filter currently selected in a file dialog.</summary>
	/// <returns>The <see cref="T:System.Int32" /> that is the index of the selected filter. The default is 1.</returns>
	public int FilterIndex
	{
		get
		{
			return _filterIndex;
		}
		set
		{
			_filterIndex = value;
		}
	}

	/// <summary>This property is not implemented.  </summary>
	/// <returns>Not implemented.</returns>
	public bool RestoreDirectory
	{
		get
		{
			return GetOption(FOS.NOCHANGEDIR);
		}
		set
		{
			SetOption(FOS.NOCHANGEDIR, value);
		}
	}

	/// <summary>Occurs when the user selects a file name by either clicking the Open button of the <see cref="T:Microsoft.Win32.OpenFileDialog" /> or the Save button of the <see cref="T:Microsoft.Win32.SaveFileDialog" />.</summary>
	public event CancelEventHandler FileOk;

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.FileDialog" /> class.</summary>
	private protected FileDialog()
	{
		Initialize();
	}

	/// <summary>Sets all properties of a file dialog back to their initial values.</summary>
	public override void Reset()
	{
		base.Reset();
		Initialize();
	}

	/// <summary>Returns a string that represents a file dialog.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of <see cref="T:Microsoft.Win32.FileDialog" /> that contains the full pathname for any files selected from either <see cref="T:Microsoft.Win32.OpenFileDialog" />, <see cref="T:Microsoft.Win32.SaveFileDialog" />.</returns>
	public override string ToString()
	{
		return base.ToString() + ", FileName: " + FileName;
	}

	protected override void OnItemOk(CancelEventArgs e)
	{
		if (this.FileOk != null)
		{
			this.FileOk(this, e);
		}
	}

	internal virtual bool PromptUserIfAppropriate(string fileName)
	{
		bool flag = true;
		if (GetOption(FOS.FILEMUSTEXIST))
		{
			try
			{
				flag = File.Exists(Path.GetFullPath(fileName));
			}
			catch (PathTooLongException)
			{
				flag = false;
			}
			if (!flag)
			{
				PromptFileNotFound(fileName);
			}
		}
		return flag;
	}

	private protected override void PrepareDialog(IFileDialog dialog)
	{
		base.PrepareDialog(dialog);
		dialog.SetDefaultExtension(DefaultExt);
		COMDLG_FILTERSPEC[] filterItems = GetFilterItems(Filter);
		if (filterItems.Length != 0)
		{
			dialog.SetFileTypes((uint)filterItems.Length, filterItems);
			dialog.SetFileTypeIndex((uint)FilterIndex);
		}
	}

	private protected override bool TryHandleItemOk(IFileDialog dialog, out object restoreState)
	{
		restoreState = _filterIndex;
		uint fileTypeIndex = dialog.GetFileTypeIndex();
		_filterIndex = (int)fileTypeIndex;
		return ProcessFileNames();
	}

	private protected override void RevertItemOk(object state)
	{
		_filterIndex = (int)state;
	}

	private void Initialize()
	{
		AddExtension = true;
		_defaultExtension = null;
		_filter = null;
		_filterIndex = 1;
	}

	private bool ProcessFileNames()
	{
		if (!GetOption(FOS.NOVALIDATE))
		{
			string[] filterExtensions = GetFilterExtensions();
			for (int i = 0; i < base.MutableItemNames.Length; i++)
			{
				string text = base.MutableItemNames[i];
				if (AddExtension && !Path.HasExtension(text))
				{
					for (int j = 0; j < filterExtensions.Length; j++)
					{
						Invariant.Assert(!filterExtensions[j].StartsWith(".", StringComparison.Ordinal), "FileDialog.GetFilterExtensions should not return things starting with '.'");
						string extension = Path.GetExtension(text);
						Invariant.Assert(extension.Length == 0 || extension.StartsWith(".", StringComparison.Ordinal), "Path.GetExtension should return something that starts with '.'");
						string text2 = ((MemoryExtensions.IndexOfAny(filterExtensions[j], '*', '?') == -1) ? string.Concat(text.AsSpan(0, text.Length - extension.Length), ".", filterExtensions[j]) : text.Substring(0, text.Length - extension.Length));
						if (!GetOption(FOS.FILEMUSTEXIST) || File.Exists(text2))
						{
							text = text2;
							break;
						}
					}
					base.MutableItemNames[i] = text;
				}
				if (!PromptUserIfAppropriate(text))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void PromptFileNotFound(string fileName)
	{
		MessageBoxWithFocusRestore(SR.Format(SR.FileDialogFileNotFound, fileName), MessageBoxButton.OK, MessageBoxImage.Exclamation);
	}

	private static COMDLG_FILTERSPEC[] GetFilterItems(string filter)
	{
		List<COMDLG_FILTERSPEC> list = new List<COMDLG_FILTERSPEC>();
		if (!string.IsNullOrEmpty(filter))
		{
			string[] array = filter.Split('|');
			if (array.Length % 2 == 0)
			{
				for (int i = 1; i < array.Length; i += 2)
				{
					list.Add(new COMDLG_FILTERSPEC
					{
						pszName = array[i - 1],
						pszSpec = array[i]
					});
				}
			}
		}
		return list.ToArray();
	}

	private string[] GetFilterExtensions()
	{
		string filter = _filter;
		List<string> list = new List<string>();
		if (_defaultExtension != null)
		{
			list.Add(_defaultExtension);
		}
		if (filter != null)
		{
			string[] array = filter.Split('|', StringSplitOptions.RemoveEmptyEntries);
			int num = _filterIndex * 2 - 1;
			if (num >= array.Length)
			{
				throw new InvalidOperationException(SR.FileDialogInvalidFilterIndex);
			}
			if (_filterIndex > 0)
			{
				string[] array2 = array[num].Split(';');
				foreach (string text in array2)
				{
					int num2 = text.LastIndexOf('.');
					if (num2 >= 0)
					{
						list.Add(text.Substring(num2 + 1, text.Length - (num2 + 1)));
					}
				}
			}
		}
		return list.ToArray();
	}
}
