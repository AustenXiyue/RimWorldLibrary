using System;
using System.ComponentModel;
using System.IO;
using MS.Internal.AppModel;
using MS.Internal.Interop;

namespace Microsoft.Win32;

public sealed class OpenFolderDialog : CommonItemDialog
{
	public string SafeFolderName
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

	public string[] SafeFolderNames
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

	public string FolderName
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

	public string[] FolderNames => CloneItemNames();

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

	public event CancelEventHandler FolderOk;

	public OpenFolderDialog()
	{
		Initialize();
	}

	public override void Reset()
	{
		base.Reset();
		Initialize();
	}

	public override string ToString()
	{
		return base.ToString() + ", FolderName: " + FolderName;
	}

	protected override void OnItemOk(CancelEventArgs e)
	{
		if (this.FolderOk != null)
		{
			this.FolderOk(this, e);
		}
	}

	private protected override IFileDialog CreateDialog()
	{
		return (IFileDialog)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")));
	}

	private void Initialize()
	{
		SetOption(FOS.FILEMUSTEXIST, value: true);
		SetOption(FOS.PICKFOLDERS, value: true);
	}
}
