namespace System.Windows;

/// <summary>Specifies how an application will shutdown. Used by the <see cref="P:System.Windows.Application.ShutdownMode" /> property.</summary>
public enum ShutdownMode : byte
{
	/// <summary>An application shuts down when either the last window closes, or <see cref="M:System.Windows.Application.Shutdown" /> is called.</summary>
	OnLastWindowClose,
	/// <summary>An application shuts down when either the main window closes, or <see cref="M:System.Windows.Application.Shutdown" /> is called.</summary>
	OnMainWindowClose,
	/// <summary>An application shuts down only when <see cref="M:System.Windows.Application.Shutdown" /> is called.</summary>
	OnExplicitShutdown
}
