using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Interop;
using MS.Win32;

namespace Microsoft.Win32;

public abstract class CommonItemDialog : CommonDialog
{
	private protected sealed class VistaDialogEvents : IFileDialogEvents, IDisposable
	{
		public delegate bool OnOkCallback(IFileDialog dialog);

		private IFileDialog _dialog;

		private OnOkCallback _okCallback;

		private uint _eventCookie;

		public VistaDialogEvents(IFileDialog dialog, OnOkCallback okCallback)
		{
			_dialog = dialog;
			_eventCookie = dialog.Advise(this);
			_okCallback = okCallback;
		}

		MS.Internal.Interop.HRESULT IFileDialogEvents.OnFileOk(IFileDialog pfd)
		{
			if (!_okCallback(pfd))
			{
				return MS.Internal.Interop.HRESULT.S_FALSE;
			}
			return MS.Internal.Interop.HRESULT.S_OK;
		}

		MS.Internal.Interop.HRESULT IFileDialogEvents.OnFolderChanging(IFileDialog pfd, IShellItem psiFolder)
		{
			return MS.Internal.Interop.HRESULT.E_NOTIMPL;
		}

		MS.Internal.Interop.HRESULT IFileDialogEvents.OnFolderChange(IFileDialog pfd)
		{
			return MS.Internal.Interop.HRESULT.S_OK;
		}

		MS.Internal.Interop.HRESULT IFileDialogEvents.OnSelectionChange(IFileDialog pfd)
		{
			return MS.Internal.Interop.HRESULT.S_OK;
		}

		MS.Internal.Interop.HRESULT IFileDialogEvents.OnShareViolation(IFileDialog pfd, IShellItem psi, out FDESVR pResponse)
		{
			pResponse = FDESVR.DEFAULT;
			return MS.Internal.Interop.HRESULT.S_OK;
		}

		MS.Internal.Interop.HRESULT IFileDialogEvents.OnTypeChange(IFileDialog pfd)
		{
			return MS.Internal.Interop.HRESULT.S_OK;
		}

		MS.Internal.Interop.HRESULT IFileDialogEvents.OnOverwrite(IFileDialog pfd, IShellItem psi, out FDEOR pResponse)
		{
			pResponse = FDEOR.DEFAULT;
			return MS.Internal.Interop.HRESULT.S_OK;
		}

		void IDisposable.Dispose()
		{
			_dialog.Unadvise(_eventCookie);
		}
	}

	private MS.Internal.SecurityCriticalDataForSet<FOS> _dialogOptions;

	private MS.Internal.SecurityCriticalDataForSet<string> _title;

	private MS.Internal.SecurityCriticalDataForSet<string> _initialDirectory;

	private MS.Internal.SecurityCriticalDataForSet<string> _defaultDirectory;

	private MS.Internal.SecurityCriticalDataForSet<string> _rootDirectory;

	private nint _hwndFileDialog;

	private string[] _itemNames;

	public bool AddToRecent
	{
		get
		{
			return !GetOption(FOS.DONTADDTORECENT);
		}
		set
		{
			SetOption(FOS.DONTADDTORECENT, !value);
		}
	}

	public Guid? ClientGuid { get; set; }

	public string DefaultDirectory
	{
		get
		{
			if (_defaultDirectory.Value != null)
			{
				return _defaultDirectory.Value;
			}
			return string.Empty;
		}
		set
		{
			_defaultDirectory.Value = value;
		}
	}

	public bool DereferenceLinks
	{
		get
		{
			return !GetOption(FOS.NODEREFERENCELINKS);
		}
		set
		{
			SetOption(FOS.NODEREFERENCELINKS, !value);
		}
	}

	public string InitialDirectory
	{
		get
		{
			if (_initialDirectory.Value != null)
			{
				return _initialDirectory.Value;
			}
			return string.Empty;
		}
		set
		{
			_initialDirectory.Value = value;
		}
	}

	public string RootDirectory
	{
		get
		{
			if (_rootDirectory.Value != null)
			{
				return _rootDirectory.Value;
			}
			return string.Empty;
		}
		set
		{
			_rootDirectory.Value = value;
		}
	}

	public bool ShowHiddenItems
	{
		get
		{
			return GetOption(FOS.FORCESHOWHIDDEN);
		}
		set
		{
			SetOption(FOS.FORCESHOWHIDDEN, value);
		}
	}

	public string Title
	{
		get
		{
			if (_title.Value != null)
			{
				return _title.Value;
			}
			return string.Empty;
		}
		set
		{
			_title.Value = value;
		}
	}

	public bool ValidateNames
	{
		get
		{
			return !GetOption(FOS.NOVALIDATE);
		}
		set
		{
			SetOption(FOS.NOVALIDATE, !value);
		}
	}

	public IList<FileDialogCustomPlace> CustomPlaces { get; set; }

	private protected string CriticalItemName
	{
		get
		{
			string[] itemNames = _itemNames;
			if (itemNames != null && itemNames.Length != 0)
			{
				return _itemNames[0];
			}
			return string.Empty;
		}
	}

	private protected string[] MutableItemNames
	{
		get
		{
			return _itemNames;
		}
		set
		{
			_itemNames = value;
		}
	}

	private string DialogCaption
	{
		get
		{
			if (!MS.Win32.UnsafeNativeMethods.IsWindow(new HandleRef(this, _hwndFileDialog)))
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(MS.Win32.UnsafeNativeMethods.GetWindowTextLength(new HandleRef(this, _hwndFileDialog)) + 1);
			MS.Win32.UnsafeNativeMethods.GetWindowText(new HandleRef(this, _hwndFileDialog), stringBuilder, stringBuilder.Capacity);
			return stringBuilder.ToString();
		}
	}

	private protected CommonItemDialog()
	{
		Initialize();
	}

	public override void Reset()
	{
		Initialize();
	}

	public override string ToString()
	{
		return base.ToString() + ": Title: " + Title;
	}

	protected virtual void OnItemOk(CancelEventArgs e)
	{
	}

	protected override bool RunDialog(nint hwndOwner)
	{
		IFileDialog fileDialog = CreateDialog();
		PrepareDialog(fileDialog);
		using (new VistaDialogEvents(fileDialog, HandleItemOk))
		{
			return fileDialog.Show(hwndOwner).Succeeded;
		}
	}

	internal bool GetOption(FOS option)
	{
		return (_dialogOptions.Value & option) != 0;
	}

	internal void SetOption(FOS option, bool value)
	{
		if (value)
		{
			_dialogOptions.Value |= option;
		}
		else
		{
			_dialogOptions.Value &= ~option;
		}
	}

	internal bool MessageBoxWithFocusRestore(string message, MessageBoxButton buttons, MessageBoxImage image)
	{
		bool flag = false;
		nint focus = MS.Win32.UnsafeNativeMethods.GetFocus();
		try
		{
			return MessageBox.Show(message, DialogCaption, buttons, image, MessageBoxResult.OK, MessageBoxOptions.None) == MessageBoxResult.Yes;
		}
		finally
		{
			MS.Win32.UnsafeNativeMethods.SetFocus(new HandleRef(this, focus));
		}
	}

	private protected abstract IFileDialog CreateDialog();

	private protected virtual void PrepareDialog(IFileDialog dialog)
	{
		Guid? clientGuid = ClientGuid;
		if (clientGuid.HasValue)
		{
			Guid guid = clientGuid.GetValueOrDefault();
			dialog.SetClientGuid(ref guid);
		}
		if (!string.IsNullOrEmpty(DefaultDirectory))
		{
			IShellItem shellItemForPath = ShellUtil.GetShellItemForPath(DefaultDirectory);
			if (shellItemForPath != null)
			{
				dialog.SetDefaultFolder(shellItemForPath);
			}
		}
		if (!string.IsNullOrEmpty(InitialDirectory))
		{
			IShellItem shellItemForPath2 = ShellUtil.GetShellItemForPath(InitialDirectory);
			if (shellItemForPath2 != null)
			{
				if (string.IsNullOrEmpty(DefaultDirectory))
				{
					dialog.SetDefaultFolder(shellItemForPath2);
				}
				dialog.SetFolder(shellItemForPath2);
			}
		}
		if (!string.IsNullOrEmpty(RootDirectory))
		{
			IShellItem shellItemForPath3 = ShellUtil.GetShellItemForPath(RootDirectory);
			if (shellItemForPath3 != null && dialog is IFileDialog2 fileDialog)
			{
				fileDialog.SetNavigationRoot(shellItemForPath3);
			}
		}
		dialog.SetTitle(Title);
		dialog.SetFileName(CriticalItemName);
		FOS value = _dialogOptions.Value;
		dialog.SetOptions(value);
		IList<FileDialogCustomPlace> customPlaces = CustomPlaces;
		if (customPlaces == null || customPlaces.Count == 0)
		{
			return;
		}
		foreach (FileDialogCustomPlace item in customPlaces)
		{
			IShellItem shellItem = ResolveCustomPlace(item);
			if (shellItem != null)
			{
				try
				{
					dialog.AddPlace(shellItem, FDAP.BOTTOM);
				}
				catch (ArgumentException)
				{
				}
			}
		}
	}

	private protected virtual bool TryHandleItemOk(IFileDialog dialog, out object revertState)
	{
		revertState = null;
		return true;
	}

	private protected virtual void RevertItemOk(object state)
	{
	}

	private protected string[] CloneItemNames()
	{
		if (_itemNames == null)
		{
			return Array.Empty<string>();
		}
		return (string[])_itemNames.Clone();
	}

	private void Initialize()
	{
		_dialogOptions.Value = (FOS)0u;
		SetOption(FOS.PATHMUSTEXIST, value: true);
		SetOption(FOS.DEFAULTNOMINIMODE, value: true);
		SetOption(FOS.FORCEFILESYSTEM, value: true);
		_itemNames = null;
		_title.Value = null;
		_initialDirectory.Value = null;
		_defaultDirectory.Value = null;
		_rootDirectory.Value = null;
		CustomPlaces = new List<FileDialogCustomPlace>();
		ClientGuid = null;
	}

	private bool HandleItemOk(IFileDialog dialog)
	{
		((MS.Win32.UnsafeNativeMethods.IOleWindow)dialog).GetWindow(out _hwndFileDialog);
		string[] itemNames = _itemNames;
		object revertState = null;
		bool flag = false;
		try
		{
			IShellItem[] items = ResolveResults(dialog);
			_itemNames = GetParsingNames(items);
			if (TryHandleItemOk(dialog, out revertState))
			{
				CancelEventArgs cancelEventArgs = new CancelEventArgs();
				OnItemOk(cancelEventArgs);
				flag = !cancelEventArgs.Cancel;
			}
		}
		finally
		{
			if (!flag)
			{
				RevertItemOk(revertState);
				_itemNames = itemNames;
			}
		}
		return flag;
	}

	private static string[] GetParsingNames(IShellItem[] items)
	{
		if (items == null)
		{
			return null;
		}
		string[] array = new string[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			array[i] = items[i].GetDisplayName(SIGDN.DESKTOPABSOLUTEPARSING);
		}
		return array;
	}

	private static IShellItem[] ResolveResults(IFileDialog dialog)
	{
		if (dialog is IFileOpenDialog fileOpenDialog)
		{
			IShellItemArray results = fileOpenDialog.GetResults();
			uint count = results.GetCount();
			IShellItem[] array = new IShellItem[count];
			for (uint num = 0u; num < count; num++)
			{
				array[num] = results.GetItemAt(num);
			}
			return array;
		}
		IShellItem result = dialog.GetResult();
		return new IShellItem[1] { result };
	}

	private static IShellItem ResolveCustomPlace(FileDialogCustomPlace customPlace)
	{
		return ShellUtil.GetShellItemForPath(ShellUtil.GetPathForKnownFolder(customPlace.KnownFolder) ?? customPlace.Path);
	}
}
