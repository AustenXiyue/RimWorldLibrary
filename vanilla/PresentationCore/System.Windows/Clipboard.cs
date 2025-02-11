using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Media.Imaging;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows;

/// <summary>Provides static methods that facilitate transferring data to and from the system Clipboard.</summary>
public static class Clipboard
{
	private const int OleRetryCount = 10;

	private const int OleRetryDelay = 100;

	private const int OleFlushDelay = 10;

	private static int _isDeviceGuardEnabled;

	private static bool IsDeviceGuardEnabled
	{
		get
		{
			if (_isDeviceGuardEnabled < 0)
			{
				return false;
			}
			if (_isDeviceGuardEnabled > 0)
			{
				return true;
			}
			bool num = IsDynamicCodePolicyEnabled();
			_isDeviceGuardEnabled = (num ? 1 : (-1));
			return num;
		}
	}

	/// <summary>Clears any data from the system Clipboard.</summary>
	public static void Clear()
	{
		int num = 10;
		while (true)
		{
			int num2 = OleServicesContext.CurrentOleServicesContext.OleSetClipboard(null);
			if (!MS.Win32.NativeMethods.Succeeded(num2))
			{
				if (--num == 0)
				{
					Marshal.ThrowExceptionForHR(num2);
				}
				Thread.Sleep(100);
				continue;
			}
			break;
		}
	}

	/// <summary>Queries the Clipboard for the presence of data in the <see cref="F:System.Windows.DataFormats.WaveAudio" /> data format.</summary>
	/// <returns>true if the Clipboard contains data in the <see cref="F:System.Windows.DataFormats.WaveAudio" /> data format; otherwise, false.</returns>
	public static bool ContainsAudio()
	{
		return ContainsDataInternal(DataFormats.WaveAudio);
	}

	/// <summary>Queries the Clipboard for the presence of data in a specified data format.</summary>
	/// <returns>true if data in the specified format is available on the Clipboard; otherwise, false. See Remarks.</returns>
	/// <param name="format">The format of the data to look for. See <see cref="T:System.Windows.DataFormats" /> for predefined formats. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public static bool ContainsData(string format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.Length == 0)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		return ContainsDataInternal(format);
	}

	/// <summary>Queries the Clipboard for the presence of data in the <see cref="F:System.Windows.DataFormats.FileDrop" /> data format.</summary>
	/// <returns>true if the Clipboard contains data in the <see cref="F:System.Windows.DataFormats.FileDrop" /> data format; otherwise, false.</returns>
	public static bool ContainsFileDropList()
	{
		return ContainsDataInternal(DataFormats.FileDrop);
	}

	/// <summary>Queries the Clipboard for the presence of data in the <see cref="F:System.Windows.DataFormats.Bitmap" /> data format.</summary>
	/// <returns>true if the Clipboard contains data in the <see cref="F:System.Windows.DataFormats.Bitmap" /> data format; otherwise, false.</returns>
	public static bool ContainsImage()
	{
		return ContainsDataInternal(DataFormats.Bitmap);
	}

	/// <summary>Queries the Clipboard for the presence of data in the <see cref="F:System.Windows.DataFormats.UnicodeText" /> format.</summary>
	/// <returns>true if the Clipboard contains data in the <see cref="F:System.Windows.DataFormats.UnicodeText" /> data format; otherwise, false.</returns>
	public static bool ContainsText()
	{
		return ContainsDataInternal(DataFormats.UnicodeText);
	}

	/// <summary>Queries the Clipboard for the presence of data in a text data format.</summary>
	/// <returns>true if the Clipboard contains data in the specified text data format; otherwise, false.</returns>
	/// <param name="format">A member of the <see cref="T:System.Windows.TextDataFormat" /> enumeration that specifies the text data format to query for.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="format" /> does not specify a valid member of <see cref="T:System.Windows.TextDataFormat" />.</exception>
	public static bool ContainsText(TextDataFormat format)
	{
		if (!DataFormats.IsValidTextDataFormat(format))
		{
			throw new InvalidEnumArgumentException("format", (int)format, typeof(TextDataFormat));
		}
		return ContainsDataInternal(DataFormats.ConvertToDataFormats(format));
	}

	/// <summary>Permanently adds the data that is on the <see cref="T:System.Windows.Clipboard" /> so that it is available after the data's original application closes.</summary>
	public static void Flush()
	{
		int num = 10;
		while (true)
		{
			int hr = OleServicesContext.CurrentOleServicesContext.OleFlushClipboard();
			if (!MS.Win32.NativeMethods.Succeeded(hr))
			{
				if (--num == 0)
				{
					SecurityHelper.ThrowExceptionForHR(hr);
				}
				Thread.Sleep(100);
				continue;
			}
			break;
		}
	}

	/// <summary>Returns a stream of Clipboard data in the <see cref="F:System.Windows.DataFormats.WaveAudio" /> data format.</summary>
	/// <returns>A stream that contains the data in the <see cref="F:System.Windows.DataFormats.WaveAudio" /> format, or null if the Clipboard does not contain data in this format.</returns>
	public static Stream GetAudioStream()
	{
		return GetDataInternal(DataFormats.WaveAudio) as Stream;
	}

	/// <summary>Retrieves data in a specified format from the Clipboard.</summary>
	/// <returns>An object that contains the data in the specified format, or null if the data is unavailable in the specified format.</returns>
	/// <param name="format">A string that specifies the format of the data to retrieve. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public static object GetData(string format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		return GetDataInternal(format);
	}

	/// <summary>Returns a string collection that contains a list of dropped files available on the Clipboard.</summary>
	/// <returns>A collection of strings, where each string specifies the name of a file in the list of dropped files on the Clipboard, or null if the data is unavailable in this format.</returns>
	public static StringCollection GetFileDropList()
	{
		StringCollection stringCollection = new StringCollection();
		if (GetDataInternal(DataFormats.FileDrop) is string[] value)
		{
			stringCollection.AddRange(value);
		}
		return stringCollection;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> object from the Clipboard that contains data in the <see cref="F:System.Windows.DataFormats.Bitmap" /> format.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> object that contains data in the <see cref="F:System.Windows.DataFormats.Bitmap" /> format, or null if the data is unavailable in this format.</returns>
	public static BitmapSource GetImage()
	{
		return GetDataInternal(DataFormats.Bitmap) as BitmapSource;
	}

	/// <summary>Returns a string containing the <see cref="F:System.Windows.DataFormats.UnicodeText" /> data on the Clipboard.</summary>
	/// <returns>A string containing the <see cref="F:System.Windows.DataFormats.UnicodeText" /> data , or an empty string if no <see cref="F:System.Windows.DataFormats.UnicodeText" /> data is available on the Clipboard.</returns>
	public static string GetText()
	{
		return GetText(TextDataFormat.UnicodeText);
	}

	/// <summary>Returns a string containing text data on the Clipboard. </summary>
	/// <returns>A string containing text data in the specified data format, or an empty string if no corresponding text data is available.</returns>
	/// <param name="format">A member of <see cref="T:System.Windows.TextDataFormat" /> that specifies the text data format to retrieve.</param>
	public static string GetText(TextDataFormat format)
	{
		if (!DataFormats.IsValidTextDataFormat(format))
		{
			throw new InvalidEnumArgumentException("format", (int)format, typeof(TextDataFormat));
		}
		string text = (string)GetDataInternal(DataFormats.ConvertToDataFormats(format));
		if (text != null)
		{
			return text;
		}
		return string.Empty;
	}

	/// <summary>Stores audio data (<see cref="F:System.Windows.DataFormats.WaveAudio" /> data format) on the Clipboard.  The audio data is specified as a byte array.</summary>
	/// <param name="audioBytes">A byte array that contains audio data to store on the Clipboard.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="audioBytes" /> is null.</exception>
	public static void SetAudio(byte[] audioBytes)
	{
		if (audioBytes == null)
		{
			throw new ArgumentNullException("audioBytes");
		}
		SetAudio(new MemoryStream(audioBytes));
	}

	/// <summary>Stores audio data (<see cref="F:System.Windows.DataFormats.WaveAudio" /> data format) on the Clipboard.  The audio data is specified as a stream.</summary>
	/// <param name="audioStream">A stream that contains audio data to store on the Clipboard.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="audioStream" /> is null.</exception>
	public static void SetAudio(Stream audioStream)
	{
		if (audioStream == null)
		{
			throw new ArgumentNullException("audioStream");
		}
		SetDataInternal(DataFormats.WaveAudio, audioStream);
	}

	/// <summary>Stores the specified data on the Clipboard in the specified format.</summary>
	/// <param name="format">A string that specifies the format to use to store the data. See the <see cref="T:System.Windows.DataFormats" /> class for a set of predefined data formats.</param>
	/// <param name="data">An object representing the data to store on the Clipboard.</param>
	public static void SetData(string format, object data)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		SetDataInternal(format, data);
	}

	/// <summary>Stores <see cref="F:System.Windows.DataFormats.FileDrop" /> data on the Clipboard.  The dropped file list is specified as a string collection.</summary>
	/// <param name="fileDropList">A string collection that contains the dropped file list to store in the data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileDropList" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fileDropList" /> contains zero strings, or the full path to file specified in the list cannot be resolved.</exception>
	public static void SetFileDropList(StringCollection fileDropList)
	{
		if (fileDropList == null)
		{
			throw new ArgumentNullException("fileDropList");
		}
		if (fileDropList.Count == 0)
		{
			throw new ArgumentException(SR.Format(SR.DataObject_FileDropListIsEmpty, fileDropList));
		}
		StringEnumerator enumerator = fileDropList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				try
				{
					Path.GetFullPath(current);
				}
				catch (ArgumentException)
				{
					throw new ArgumentException(SR.Format(SR.DataObject_FileDropListHasInvalidFileDropPath, fileDropList));
				}
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		string[] array = new string[fileDropList.Count];
		fileDropList.CopyTo(array, 0);
		SetDataInternal(DataFormats.FileDrop, array);
	}

	/// <summary>Stores <see cref="F:System.Windows.DataFormats.Bitmap" /> data on the Clipboard.  The image data is specified as a <see cref="T:System.Windows.Media.Imaging.BitmapSource" />.</summary>
	/// <param name="image">A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> object that contains the image data to store on the Clipboard.</param>
	public static void SetImage(BitmapSource image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		SetDataInternal(DataFormats.Bitmap, image);
	}

	/// <summary>Stores <see cref="F:System.Windows.DataFormats.UnicodeText" /> data on the Clipboard. </summary>
	/// <param name="text">A string that contains the <see cref="F:System.Windows.DataFormats.UnicodeText" /> data to store on the Clipboard.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="text" /> is null.</exception>
	public static void SetText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		SetText(text, TextDataFormat.UnicodeText);
	}

	/// <summary>Stores text data on the Clipboard in a specified text data format.  The <see cref="F:System.Windows.DataFormats.UnicodeText" /> data to store is specified as a string.</summary>
	/// <param name="text">A string that contains the text data to store on the Clipboard.</param>
	/// <param name="format">A member of <see cref="T:System.Windows.TextDataFormat" /> that specifies the specific text data format to store.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="text" /> is null.</exception>
	public static void SetText(string text, TextDataFormat format)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (!DataFormats.IsValidTextDataFormat(format))
		{
			throw new InvalidEnumArgumentException("format", (int)format, typeof(TextDataFormat));
		}
		SetDataInternal(DataFormats.ConvertToDataFormats(format), text);
	}

	/// <summary>Returns a data object that represents the entire contents of the Clipboard.</summary>
	/// <returns>A data object that enables access to the entire contents of the system Clipboard, or null if there is no data on the Clipboard.</returns>
	public static IDataObject GetDataObject()
	{
		return GetDataObjectInternal();
	}

	/// <summary>Compares a specified data object to the contents of the Clipboard.</summary>
	/// <returns>true if the specified data object matches what is on the system Clipboard; otherwise, false.</returns>
	/// <param name="data">A data object to compare to the contents of the system Clipboard.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">An error occurred when accessing the Clipboard. The exception details will include an HResult that identifies the specific error; see <see cref="P:System.Runtime.InteropServices.ErrorWrapper.ErrorCode" />.</exception>
	public static bool IsCurrent(IDataObject data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		bool result = false;
		if (data is System.Runtime.InteropServices.ComTypes.IDataObject)
		{
			int num = 10;
			int num2;
			while (true)
			{
				num2 = OleServicesContext.CurrentOleServicesContext.OleIsCurrentClipboard((System.Runtime.InteropServices.ComTypes.IDataObject)data);
				if (MS.Win32.NativeMethods.Succeeded(num2) || --num == 0)
				{
					break;
				}
				Thread.Sleep(100);
			}
			if (num2 == 0)
			{
				result = true;
			}
			else if (!MS.Win32.NativeMethods.Succeeded(num2))
			{
				throw new ExternalException("OleIsCurrentClipboard()", num2);
			}
		}
		return result;
	}

	/// <summary>Places a specified non-persistent data object on the system Clipboard.</summary>
	/// <param name="data">A data object (an object that implements <see cref="T:System.Windows.IDataObject" />) to place on the system Clipboard.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">An error occurred when accessing the Clipboard. The exception details will include an HResult that identifies the specific error; see <see cref="P:System.Runtime.InteropServices.ErrorWrapper.ErrorCode" />.</exception>
	public static void SetDataObject(object data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		SetDataObject(data, copy: false);
	}

	/// <summary>Places a specified data object on the system Clipboard and accepts a Boolean parameter that indicates whether the data object should be left on the Clipboard when the application exits.</summary>
	/// <param name="data">A data object (an object that implements <see cref="T:System.Windows.IDataObject" />) to place on the system Clipboard.</param>
	/// <param name="copy">true to leave the data on the system Clipboard when the application exits; false to clear the data from the system Clipboard when the application exits.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">An error occured when accessing the Clipboard.  The exception details will include an HResult that identifies the specific error; see <see cref="P:System.Runtime.InteropServices.ErrorWrapper.ErrorCode" />.</exception>
	public static void SetDataObject(object data, bool copy)
	{
		CriticalSetDataObject(data, copy);
	}

	[FriendAccessAllowed]
	internal static void CriticalSetDataObject(object data, bool copy)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		System.Runtime.InteropServices.ComTypes.IDataObject dataObject = ((data is DataObject) ? ((DataObject)data) : ((!(data is System.Runtime.InteropServices.ComTypes.IDataObject)) ? new DataObject(data) : ((System.Runtime.InteropServices.ComTypes.IDataObject)data)));
		int num = 10;
		while (true)
		{
			int num2 = OleServicesContext.CurrentOleServicesContext.OleSetClipboard(dataObject);
			if (MS.Win32.NativeMethods.Succeeded(num2))
			{
				break;
			}
			if (--num == 0)
			{
				Marshal.ThrowExceptionForHR(num2);
			}
			Thread.Sleep(100);
		}
		if (copy)
		{
			Thread.Sleep(10);
			Flush();
		}
	}

	[FriendAccessAllowed]
	internal static bool IsClipboardPopulated()
	{
		return GetDataObjectInternal() != null;
	}

	private static bool IsDynamicCodePolicyEnabled()
	{
		bool enabled = false;
		nint num = IntPtr.Zero;
		try
		{
			num = MS.Win32.LoadLibraryHelper.SecureLoadLibraryEx("wldp.dll", IntPtr.Zero, MS.Win32.UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32);
			if (num != IntPtr.Zero && MS.Win32.UnsafeNativeMethods.GetProcAddressNoThrow(new HandleRef(null, num), "WldpIsDynamicCodePolicyEnabled") != IntPtr.Zero && MS.Win32.UnsafeNativeMethods.WldpIsDynamicCodePolicyEnabled(out enabled) != 0)
			{
				enabled = false;
			}
		}
		catch
		{
		}
		finally
		{
			if (num != IntPtr.Zero)
			{
				MS.Win32.UnsafeNativeMethods.FreeLibrary(num);
			}
		}
		return enabled;
	}

	private static IDataObject GetDataObjectInternal()
	{
		int num = 10;
		System.Runtime.InteropServices.ComTypes.IDataObject dataObject;
		while (true)
		{
			dataObject = null;
			int num2 = OleServicesContext.CurrentOleServicesContext.OleGetClipboard(ref dataObject);
			if (MS.Win32.NativeMethods.Succeeded(num2))
			{
				break;
			}
			if (--num == 0)
			{
				Marshal.ThrowExceptionForHR(num2);
			}
			Thread.Sleep(100);
		}
		if (dataObject is IDataObject && !Marshal.IsComObject(dataObject))
		{
			return (IDataObject)dataObject;
		}
		if (dataObject != null)
		{
			return new DataObject(dataObject);
		}
		return null;
	}

	private static bool ContainsDataInternal(string format)
	{
		bool result = false;
		if (IsDataFormatAutoConvert(format))
		{
			string[] mappedFormats = DataObject.GetMappedFormats(format);
			for (int i = 0; i < mappedFormats.Length; i++)
			{
				if (SafeNativeMethods.IsClipboardFormatAvailable(DataFormats.GetDataFormat(mappedFormats[i]).Id))
				{
					result = true;
					break;
				}
			}
		}
		else
		{
			result = SafeNativeMethods.IsClipboardFormatAvailable(DataFormats.GetDataFormat(format).Id);
		}
		return result;
	}

	private static object GetDataInternal(string format)
	{
		IDataObject dataObject = GetDataObject();
		if (dataObject != null)
		{
			bool autoConvert = (IsDataFormatAutoConvert(format) ? true : false);
			return dataObject.GetData(format, autoConvert);
		}
		return null;
	}

	private static void SetDataInternal(string format, object data)
	{
		bool autoConvert = (IsDataFormatAutoConvert(format) ? true : false);
		DataObject dataObject = new DataObject();
		((IDataObject)dataObject).SetData(format, data, autoConvert);
		SetDataObject(dataObject, copy: true);
	}

	private static bool IsDataFormatAutoConvert(string format)
	{
		if (string.Equals(format, DataFormats.FileDrop, StringComparison.Ordinal) || string.Equals(format, DataFormats.Bitmap, StringComparison.Ordinal))
		{
			return true;
		}
		return false;
	}
}
