using System.Windows.Threading;

namespace System.Windows.Interop;

/// <summary>Defines the interaction between Windows Presentation Foundation (WPF) applications that are hosting interoperation content and interpreted by the Windows Presentation Foundation (WPF) executable, and a host supplied error page. </summary>
public interface IErrorPage
{
	/// <summary>Gets or sets the path to an application's deployment manifest.</summary>
	/// <returns>The path to an application's deployment manifest.</returns>
	Uri DeploymentPath { get; set; }

	/// <summary>Gets or sets the string title of the error page.</summary>
	/// <returns>The string title of the error page.</returns>
	string ErrorTitle { get; set; }

	/// <summary>Gets or sets a verbose description of the error.</summary>
	/// <returns>Description of the error.</returns>
	string ErrorText { get; set; }

	/// <summary>Gets or sets a value that indicates whether this represents an error or some other condition such as a warning. true denotes an error.</summary>
	/// <returns>true denotes an error; false denotes another condition such as a warning.</returns>
	bool ErrorFlag { get; set; }

	/// <summary>Gets or sets the string path to the error's log file, if any.</summary>
	/// <returns>Path to an associated error file. May be an empty string.</returns>
	string LogFilePath { get; set; }

	/// <summary>Gets or sets a uniform resource identifier (URI) for support information associated with the error.</summary>
	/// <returns>A link for support information.</returns>
	Uri SupportUri { get; set; }

	/// <summary>Gets or sets a reference to a <see cref="T:System.Windows.Threading.DispatcherOperationCallback" /> handler, that can handle refresh of the error page.</summary>
	/// <returns>A <see cref="T:System.Windows.Threading.DispatcherOperationCallback" /> handler to handle refresh of error page.</returns>
	DispatcherOperationCallback RefreshCallback { get; set; }

	/// <summary>Gets or sets a reference to a <see cref="T:System.Windows.Threading.DispatcherOperationCallback" />  handler, which can handle requests for Microsoft .NET Framework runtime downloads.</summary>
	/// <returns>A <see cref="T:System.Windows.Threading.DispatcherOperationCallback" />  handler,</returns>
	DispatcherOperationCallback GetWinFxCallback { get; set; }
}
