using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Windows.Ink;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows;

/// <summary>Provides a set of predefined data format names that can be used to identify data formats available in the clipboard or drag-and-drop operations.</summary>
public static class DataFormats
{
	/// <summary>Specifies the ANSI text data format.</summary>
	/// <returns>A string denoting the ANSI text data format: "Text".</returns>
	public static readonly string Text = "Text";

	/// <summary>Specifies the Unicode text data format.</summary>
	/// <returns>A string denoting the Unicode text data format: "UnicodeText".</returns>
	public static readonly string UnicodeText = "UnicodeText";

	/// <summary>Specifies the device-independent bitmap (DIB) data format.</summary>
	/// <returns>A string denoting the DIB data format: "DeviceIndependentBitmap".</returns>
	public static readonly string Dib = "DeviceIndependentBitmap";

	/// <summary>Specifies a Microsoft Windows bitmap data format.</summary>
	/// <returns>A string denoting the Windows bitmap data format: "Bitmap".</returns>
	public static readonly string Bitmap = "Bitmap";

	/// <summary>Specifies the Windows enhanced metafile format.</summary>
	/// <returns>A string denoting the Windows enhanced metafile format: "EnhancedMetafile".</returns>
	public static readonly string EnhancedMetafile = "EnhancedMetafile";

	/// <summary>Specifies the Windows metafile picture data format.</summary>
	/// <returns>A string denoting the Windows metafile picture data format: "MetaFilePict".</returns>
	public static readonly string MetafilePicture = "MetaFilePict";

	/// <summary>Specifies the Windows symbolic link data format.</summary>
	/// <returns>A string denoting the Windows symbolic link data format: "SymbolicLink".</returns>
	public static readonly string SymbolicLink = "SymbolicLink";

	/// <summary>Specifies the Windows Data Interchange Format (DIF) data format.</summary>
	/// <returns>A string denoting the DIF data format: "DataInterchangeFormat".</returns>
	public static readonly string Dif = "DataInterchangeFormat";

	/// <summary>Specifies the Tagged Image File Format (TIFF) data format.</summary>
	/// <returns>A string denoting the TIFF data format: "TaggedImageFileFormat".</returns>
	public static readonly string Tiff = "TaggedImageFileFormat";

	/// <summary>Specifies the standard Windows OEM text data format.</summary>
	/// <returns>A string denoting the Windows OEM text data format: "OEMText".</returns>
	public static readonly string OemText = "OEMText";

	/// <summary>Specifies the Windows palette data format.</summary>
	/// <returns>A string denoting the Windows palette data format: "Palette".</returns>
	public static readonly string Palette = "Palette";

	/// <summary>Specifies the Windows pen data format.</summary>
	/// <returns>A string denoting the Windows pen data format: "PenData".</returns>
	public static readonly string PenData = "PenData";

	/// <summary>Specifies the Resource Interchange File Format (RIFF) audio data format.</summary>
	/// <returns>A string denoting the RIFF audio data format: "RiffAudio".</returns>
	public static readonly string Riff = "RiffAudio";

	/// <summary>Specifies the wave audio data format.</summary>
	/// <returns>A string denoting the wave audio format: "WaveAudio".</returns>
	public static readonly string WaveAudio = "WaveAudio";

	/// <summary>Specifies the Windows file drop format.</summary>
	/// <returns>A string denoting the Windows file drop format; "FileDrop".</returns>
	public static readonly string FileDrop = "FileDrop";

	/// <summary>Specifies the Windows locale (culture) data format.</summary>
	/// <returns>A string denoting the Windows locale format: "Locale".</returns>
	public static readonly string Locale = "Locale";

	/// <summary>Specifies the HTML data format.</summary>
	/// <returns>A string denoting the HTML data format: "HTML Format".</returns>
	public static readonly string Html = "HTML Format";

	/// <summary>Specifies the Rich Text Format (RTF) data format.</summary>
	/// <returns>A string denoting the RTF data format: "Rich Text Format".</returns>
	public static readonly string Rtf = "Rich Text Format";

	/// <summary>Specifies a comma-separated value (CSV) data format.</summary>
	/// <returns>A string denoting the comma-separated value data format: "CSV".</returns>
	public static readonly string CommaSeparatedValue = "CSV";

	/// <summary>Specifies the common language runtime (CLR) string class data format.</summary>
	/// <returns>A string denoting the CLR string class data format: "System.String".</returns>
	public static readonly string StringFormat = typeof(string).FullName;

	/// <summary>Specifies a data format that encapsulates any type of serializable data objects.</summary>
	/// <returns>A string denoting a serializable data format: "PersistentObject".</returns>
	public static readonly string Serializable = "PersistentObject";

	/// <summary>Specifies the Extensible Application Markup Language (XAML) data format.</summary>
	/// <returns>A string denoting the XAML data format: "Xaml".</returns>
	public static readonly string Xaml = "Xaml";

	/// <summary>Specifies the Extensible Application Markup Language (XAML) package data format.</summary>
	/// <returns>A string denoting the XAML data format: "XamlPackage".</returns>
	public static readonly string XamlPackage = "XamlPackage";

	internal const string ApplicationTrust = "ApplicationTrust";

	internal const string FileName = "FileName";

	internal const string FileNameW = "FileNameW";

	private static ArrayList _formatList;

	private static readonly object _formatListlock = new object();

	/// <summary>Returns a <see cref="T:System.Windows.DataFormat" /> object that defines a name and numeric ID for the specified data format. The desired data format is specified by numeric ID.</summary>
	/// <returns>A <see cref="T:System.Windows.DataFormat" /> object that contains the numeric ID and the name of the requested data format.</returns>
	/// <param name="id">The numeric ID of the data format to request a <see cref="T:System.Windows.DataFormat" /> object for.</param>
	public static DataFormat GetDataFormat(int id)
	{
		return InternalGetDataFormat(id);
	}

	/// <summary>Returns a <see cref="T:System.Windows.DataFormat" /> object that defines a name and numeric ID for the specified data format. The desired data format is specified by name (a string).</summary>
	/// <returns>A <see cref="T:System.Windows.DataFormat" /> object that contains the numeric ID and the name of the requested data format.</returns>
	/// <param name="format">The name of the data format to request a <see cref="T:System.Windows.DataFormat" /> object for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public static DataFormat GetDataFormat(string format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		EnsurePredefined();
		lock (_formatListlock)
		{
			for (int i = 0; i < _formatList.Count; i++)
			{
				DataFormat dataFormat = (DataFormat)_formatList[i];
				if (dataFormat.Name.Equals(format))
				{
					return dataFormat;
				}
			}
			for (int j = 0; j < _formatList.Count; j++)
			{
				DataFormat dataFormat2 = (DataFormat)_formatList[j];
				if (string.Equals(dataFormat2.Name, format, StringComparison.OrdinalIgnoreCase))
				{
					return dataFormat2;
				}
			}
			int num = MS.Win32.UnsafeNativeMethods.RegisterClipboardFormat(format);
			if (num == 0)
			{
				throw new Win32Exception();
			}
			int index = _formatList.Add(new DataFormat(format, num));
			return (DataFormat)_formatList[index];
		}
	}

	internal static string ConvertToDataFormats(TextDataFormat textDataformat)
	{
		string result = UnicodeText;
		switch (textDataformat)
		{
		case TextDataFormat.Text:
			result = Text;
			break;
		case TextDataFormat.UnicodeText:
			result = UnicodeText;
			break;
		case TextDataFormat.Rtf:
			result = Rtf;
			break;
		case TextDataFormat.Html:
			result = Html;
			break;
		case TextDataFormat.CommaSeparatedValue:
			result = CommaSeparatedValue;
			break;
		case TextDataFormat.Xaml:
			result = Xaml;
			break;
		}
		return result;
	}

	internal static bool IsValidTextDataFormat(TextDataFormat textDataFormat)
	{
		if (textDataFormat == TextDataFormat.Text || textDataFormat == TextDataFormat.UnicodeText || textDataFormat == TextDataFormat.Rtf || textDataFormat == TextDataFormat.Html || textDataFormat == TextDataFormat.CommaSeparatedValue || textDataFormat == TextDataFormat.Xaml)
		{
			return true;
		}
		return false;
	}

	private static DataFormat InternalGetDataFormat(int id)
	{
		EnsurePredefined();
		lock (_formatListlock)
		{
			for (int i = 0; i < _formatList.Count; i++)
			{
				DataFormat dataFormat = (DataFormat)_formatList[i];
				if ((dataFormat.Id & 0xFFFF) == (id & 0xFFFF))
				{
					return dataFormat;
				}
			}
			StringBuilder stringBuilder = new StringBuilder(260);
			if (MS.Win32.UnsafeNativeMethods.GetClipboardFormatName(id, stringBuilder, stringBuilder.Capacity) == 0)
			{
				stringBuilder.Length = 0;
				stringBuilder.Append("Format").Append(id);
			}
			int index = _formatList.Add(new DataFormat(stringBuilder.ToString(), id));
			return (DataFormat)_formatList[index];
		}
	}

	private static void EnsurePredefined()
	{
		lock (_formatListlock)
		{
			if (_formatList == null)
			{
				_formatList = new ArrayList(19);
				_formatList.Add(new DataFormat(UnicodeText, 13));
				_formatList.Add(new DataFormat(Text, 1));
				_formatList.Add(new DataFormat(Bitmap, 2));
				_formatList.Add(new DataFormat(MetafilePicture, 3));
				_formatList.Add(new DataFormat(EnhancedMetafile, 14));
				_formatList.Add(new DataFormat(Dif, 5));
				_formatList.Add(new DataFormat(Tiff, 6));
				_formatList.Add(new DataFormat(OemText, 7));
				_formatList.Add(new DataFormat(Dib, 8));
				_formatList.Add(new DataFormat(Palette, 9));
				_formatList.Add(new DataFormat(PenData, 10));
				_formatList.Add(new DataFormat(Riff, 11));
				_formatList.Add(new DataFormat(WaveAudio, 12));
				_formatList.Add(new DataFormat(SymbolicLink, 4));
				_formatList.Add(new DataFormat(FileDrop, 15));
				_formatList.Add(new DataFormat(Locale, 16));
				int num = MS.Win32.UnsafeNativeMethods.RegisterClipboardFormat(Xaml);
				if (num != 0)
				{
					_formatList.Add(new DataFormat(Xaml, num));
				}
				int num2 = MS.Win32.UnsafeNativeMethods.RegisterClipboardFormat("ApplicationTrust");
				if (num2 != 0)
				{
					_formatList.Add(new DataFormat("ApplicationTrust", num2));
				}
				int num3 = MS.Win32.UnsafeNativeMethods.RegisterClipboardFormat(StrokeCollection.InkSerializedFormat);
				if (num3 != 0)
				{
					_formatList.Add(new DataFormat(StrokeCollection.InkSerializedFormat, num3));
				}
			}
		}
	}
}
