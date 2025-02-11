namespace System.Xaml;

/// <summary>Describes a service for reporting text line information in XAML reader implementations.</summary>
public interface IXamlLineInfo
{
	/// <summary>Gets a value that specifies whether line information is available.</summary>
	/// <returns>true if line information is available; otherwise, false.</returns>
	bool HasLineInfo { get; }

	/// <summary>Gets the line number to report.</summary>
	/// <returns>The line number to report.</returns>
	int LineNumber { get; }

	/// <summary>Gets the line position to report.</summary>
	/// <returns>The line position to report.</returns>
	int LinePosition { get; }
}
