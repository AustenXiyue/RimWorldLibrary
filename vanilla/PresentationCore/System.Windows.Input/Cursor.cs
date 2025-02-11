using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows.Input;

/// <summary>Represents the image used for the mouse pointer.</summary>
[TypeConverter(typeof(CursorConverter))]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class Cursor : IDisposable
{
	private const int BUFFERSIZE = 4096;

	private string _fileName = string.Empty;

	private CursorType _cursorType;

	private bool _scaleWithDpi;

	private SafeHandle _cursorHandle;

	private static readonly int[] CursorTypes = new int[28]
	{
		0, 32648, 32512, 32650, 32515, 32651, 32513, 32646, 32643, 32645,
		32642, 32644, 32516, 32514, 32649, 32631, 32652, 32653, 32654, 32655,
		32656, 32657, 32658, 32659, 32660, 32661, 32662, 32663
	};

	internal CursorType CursorType => _cursorType;

	internal SafeHandle Handle => _cursorHandle ?? MS.Win32.NativeMethods.CursorHandle.GetInvalidCursor();

	internal string FileName => _fileName;

	internal Cursor(CursorType cursorType)
	{
		if (IsValidCursorType(cursorType))
		{
			LoadCursorHelper(cursorType);
			return;
		}
		throw new ArgumentException(SR.Format(SR.InvalidCursorType, cursorType));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.Cursor" /> class from the specified .ani or a .cur file. </summary>
	/// <param name="cursorFile">The file that contains the cursor.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cursorFile" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="cursorFile" /> is not an .ani or .cur file name.</exception>
	public Cursor(string cursorFile)
		: this(cursorFile, scaleWithDpi: false)
	{
	}

	public Cursor(string cursorFile, bool scaleWithDpi)
	{
		_scaleWithDpi = scaleWithDpi;
		if (cursorFile == null)
		{
			throw new ArgumentNullException("cursorFile");
		}
		if (cursorFile != string.Empty && (cursorFile.EndsWith(".cur", StringComparison.OrdinalIgnoreCase) || cursorFile.EndsWith(".ani", StringComparison.OrdinalIgnoreCase)))
		{
			LoadFromFile(cursorFile);
			_fileName = cursorFile;
			return;
		}
		throw new ArgumentException(SR.Format(SR.Cursor_UnsupportedFormat, cursorFile));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.Cursor" /> class from the specified <see cref="T:System.IO.Stream" />. </summary>
	/// <param name="cursorStream">The <see cref="T:System.IO.Stream" /> that contains the cursor.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cursorStream" /> is null.</exception>
	/// <exception cref="T:System.IO.IOException">This constructor was unable to create a temporary file.</exception>
	public Cursor(Stream cursorStream)
		: this(cursorStream, scaleWithDpi: false)
	{
	}

	public Cursor(Stream cursorStream, bool scaleWithDpi)
	{
		_scaleWithDpi = scaleWithDpi;
		if (cursorStream == null)
		{
			throw new ArgumentNullException("cursorStream");
		}
		LoadFromStream(cursorStream);
	}

	[FriendAccessAllowed]
	internal Cursor(SafeHandle cursorHandle)
	{
		if (!cursorHandle.IsInvalid)
		{
			_cursorHandle = cursorHandle;
		}
	}

	~Cursor()
	{
		Dispose(disposing: false);
	}

	/// <summary>Releases the resources used by the <see cref="T:System.Windows.Input.Cursor" /> class. </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (_cursorHandle != null)
		{
			_cursorHandle.Dispose();
			_cursorHandle = null;
		}
	}

	private void LoadFromFile(string fileName)
	{
		_cursorHandle = MS.Win32.UnsafeNativeMethods.LoadImageCursor(IntPtr.Zero, fileName, 2, 0, 0, 0x10 | (_scaleWithDpi ? 64 : 0));
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (_cursorHandle == null || _cursorHandle.IsInvalid)
		{
			switch (lastWin32Error)
			{
			case 2:
			case 3:
				throw new Win32Exception(lastWin32Error, SR.Format(SR.Cursor_LoadImageFailure, fileName));
			default:
				throw new Win32Exception(lastWin32Error);
			case 0:
				throw new ArgumentException(SR.Format(SR.Cursor_LoadImageFailure, fileName));
			}
		}
	}

	private void LegacyLoadFromStream(Stream cursorStream)
	{
		string tempFileName = Path.GetTempFileName();
		try
		{
			using (BinaryReader binaryReader = new BinaryReader(cursorStream))
			{
				using FileStream fileStream = new FileStream(tempFileName, FileMode.Open, FileAccess.Write, FileShare.None);
				byte[] array = binaryReader.ReadBytes(4096);
				int num;
				for (num = array.Length; num >= 4096; num = binaryReader.Read(array, 0, 4096))
				{
					fileStream.Write(array, 0, 4096);
				}
				fileStream.Write(array, 0, num);
			}
			_cursorHandle = MS.Win32.UnsafeNativeMethods.LoadImageCursor(IntPtr.Zero, tempFileName, 2, 0, 0, 0x10 | (_scaleWithDpi ? 64 : 0));
			if (_cursorHandle == null || _cursorHandle.IsInvalid)
			{
				throw new ArgumentException(SR.Cursor_InvalidStream);
			}
		}
		finally
		{
			try
			{
				File.Delete(tempFileName);
			}
			catch (IOException)
			{
			}
		}
	}

	private void LoadFromStream(Stream cursorStream)
	{
		if (CoreAppContextSwitches.AllowExternalProcessToBlockAccessToTemporaryFiles)
		{
			LegacyLoadFromStream(cursorStream);
			return;
		}
		string filePath = null;
		try
		{
			using (FileStream destination = FileHelper.CreateAndOpenTemporaryFile(out filePath))
			{
				cursorStream.CopyTo(destination);
			}
			_cursorHandle = MS.Win32.UnsafeNativeMethods.LoadImageCursor(IntPtr.Zero, filePath, 2, 0, 0, 0x10 | (_scaleWithDpi ? 64 : 0));
			if (_cursorHandle == null || _cursorHandle.IsInvalid)
			{
				throw new ArgumentException(SR.Cursor_InvalidStream);
			}
		}
		finally
		{
			FileHelper.DeleteTemporaryFile(filePath);
		}
	}

	private void LoadCursorHelper(CursorType cursorType)
	{
		if (cursorType != 0)
		{
			_cursorHandle = SafeNativeMethods.LoadCursor(new HandleRef(this, IntPtr.Zero), CursorTypes[(int)cursorType]);
		}
		_cursorType = cursorType;
	}

	/// <summary>Returns the string representation of the <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>The name of the cursor.</returns>
	public override string ToString()
	{
		if (_fileName != string.Empty)
		{
			return _fileName;
		}
		return Enum.GetName(typeof(CursorType), _cursorType);
	}

	private bool IsValidCursorType(CursorType cursorType)
	{
		if (cursorType >= CursorType.None)
		{
			return cursorType <= CursorType.ArrowCD;
		}
		return false;
	}
}
