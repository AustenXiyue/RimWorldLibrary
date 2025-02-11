namespace System.Windows.Controls;

/// <summary>Specifies the formats that an <see cref="T:System.Windows.Controls.InkCanvas" /> will accept from the Clipboard.</summary>
public enum InkCanvasClipboardFormat
{
	/// <summary>Indicates that the <see cref="T:System.Windows.Controls.InkCanvas" /> accepts Ink Serialized Format (ISF).</summary>
	InkSerializedFormat,
	/// <summary>Indicates that the <see cref="T:System.Windows.Controls.InkCanvas" /> accepts text.</summary>
	Text,
	/// <summary>Indicates that the <see cref="T:System.Windows.Controls.InkCanvas" /> accepts "Extensible Application Markup Language" (XAML) format.</summary>
	Xaml
}
